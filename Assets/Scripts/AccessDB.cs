using UnityEngine;
using TMPro;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Amazon.Runtime.Internal;
using Amazon.Util;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.CognitoIdentity;
using Amazon.Runtime;
using Amazon;
using Vuforia;

public class AccessDB: MonoBehaviour {

    private IAmazonDynamoDB client;
    private DynamoDBContext context;
    private AWSCredentials credentials;

    private string aws_cognito_id = "us-east-1:ab5f71b6-28e0-4b43-8446-a2515d583680";

    private static Table sheKicksTable;
    private static string tableNameDB = "she-kicks-links-db";

    public string stockxlink = "";

    public string videolink = "";

    public bool isShoeBoxBool;

    public void Start()
    {
        UnityInitializer.AttachToGameObject(this.gameObject);

	    credentials = new CognitoAWSCredentials (
		    aws_cognito_id, // Identity Pool ID
		    RegionEndpoint.USEast1 // Region
	    );

	    client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast1);
	    context = new DynamoDBContext(client);

        Debug.Log("CONNECTED TO DB");
    }

    //Call this from somewhere
    public IEnumerator PerformGetOperationAsync(string sneakerNameTarget, ImageTargetBehaviour ImageTargetTemplate)
    {
        yield return new WaitForSeconds(2);

        string replyId = sneakerNameTarget.Trim();

        yield return new WaitForSeconds(2);

        GameObject textObject = ImageTargetTemplate.gameObject.transform.GetChild(0).gameObject;
        //textObject.GetComponent<TextMeshPro>().SetText(sneakerNameTarget);

        yield return new WaitForSeconds(2);

        print("Calling Get Operation with: "+sneakerNameTarget);

        yield return new WaitForSeconds(2);

        var request = new QueryRequest
        {
            TableName = tableNameDB,
            ReturnConsumedCapacity = "TOTAL",
            KeyConditions = new Dictionary<string, Condition>()
            {
                {
                    "sneaker-name",
                    new Condition
                    {
                        ComparisonOperator = "EQ",
                        AttributeValueList = new List<AttributeValue>()
                        {
                            new AttributeValue { S = replyId }
                        }
                    }
                }
            }
        };
        
        yield return new WaitForSeconds(2);

        client.QueryAsync(request,(result)=>{
            //resultText.text = string.Format("No. of reads used (by query in FindRepliesForAThread) {0}\n",result.Response.ConsumedCapacity.CapacityUnits);
            print("result count is "+result.Response.Items.Count);

            foreach (Dictionary<string, AttributeValue> item in result.Response.Items)
            {
                print("item is "+item);

                foreach (string s in item.Keys) {
                    print("key is: "+s);

                    if(string.Equals(s, "stockx-link")) {
                        stockxlink = item["stockx-link"].S;
                        print("stockx-link is "+stockxlink);
                    }

                    if(string.Equals(s, "videolink")) {
                        videolink = item["videolink"].S;
                        print("videolink is "+videolink);
                    }
                }
            }
        });

        yield return new WaitForSeconds(2);

        print("Finished Get Methods");
        
        yield return null;
    }

    public void PerformGetOperation(string sneakerNameTarget, ImageTargetBehaviour ImageTargetTemplate)
    {
        string replyId = sneakerNameTarget.Trim();

        GameObject textObject = ImageTargetTemplate.gameObject.transform.GetChild(0).gameObject;
        //textObject.GetComponent<TextMeshPro>().SetText(sneakerNameTarget);

        print("Calling Get Operation with: "+sneakerNameTarget);
        var request = new QueryRequest
        {
            TableName = tableNameDB,
            ReturnConsumedCapacity = "TOTAL",
            KeyConditions = new Dictionary<string, Condition>()
            {
                {
                    "sneaker-name",
                    new Condition
                    {
                        ComparisonOperator = "EQ",
                        AttributeValueList = new List<AttributeValue>()
                        {
                            new AttributeValue { S = replyId }
                        }
                    }
                }
            }
        };
        

        client.QueryAsync(request,(result)=>{
            //resultText.text = string.Format("No. of reads used (by query in FindRepliesForAThread) {0}\n",result.Response.ConsumedCapacity.CapacityUnits);
            print("result count is "+result.Response.Items.Count);

            foreach (Dictionary<string, AttributeValue> item in result.Response.Items)
            {
                print("item is "+item);

                foreach (string s in item.Keys) {
                    print("key is: "+s);

                    if(string.Equals(s, "stockx-link")) {
                        stockxlink = item["stockx-link"].S;
                        print("stockx-link is "+stockxlink);
                    }

                    if(string.Equals(s, "videolink")) {
                        videolink = item["videolink"].S;
                        print("videolink is "+videolink);
                    }
                }
            }
        });

        print("Finished Get Methods");
    }

    public void callLink() {
        print("Calling in App Browser");
        InAppBrowser.OpenURL(stockxlink);
    }

    public bool isVideo() {
        return videolink.Length > 0;
    }

    public bool isShoeBox() {
        return isShoeBoxBool;
    }

    public string getVideoLink() {
        return videolink;
    }
}




/**
[DynamoDBTable("she-kicks-links-db")]
public class sheKicksLinksDB
{
    [DynamoDBHashKey]   // Hash key.
    public string HashName { get; set; }
    [DynamoDBProperty]  
    public string sneaker-name { get; set; }
    [DynamoDBProperty]
    public string brand-link { get; set; }
    [DynamoDBProperty]
    public string colorway { get; set; }
    [DynamoDBProperty]
    public string goat-link { get; set; }
    [DynamoDBProperty]
    public string other-link { get; set; }
    [DynamoDBProperty]
    public string stockx-link { get; set; }
}
**/
