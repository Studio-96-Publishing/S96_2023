using UnityEngine;
using System;
using System.Threading;
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

public class TestScript: MonoBehaviour {

/**
    private IAmazonDynamoDB client;
    private DynamoDBContext context;
    private AWSCredentials credentials;

    private string aws_cognito_id = "us-east-1:ab5f71b6-28e0-4b43-8446-a2515d583680";

    private static Table sheKicksTable;
    private static string tableNameDB = "she-kicks-links-db";

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
    public void PerformGetOperation(string sneakerNameTarget)
    {
        string replyId = sneakerNameTarget;
            
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
            
        string stockxlink = "";

        client.QueryAsync(request,(result)=>{
            //resultText.text = string.Format("No. of reads used (by query in FindRepliesForAThread) {0}\n",result.Response.ConsumedCapacity.CapacityUnits);
            foreach (Dictionary<string, AttributeValue> item
                        in result.Response.Items)
            {
                stockxlink = item["stockx-link"].S;
            }
        });

        return stockxlink;
    }
    **/
}
