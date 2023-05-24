using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SouvrnLinkManager : MonoBehaviour
{
    private string apiKey = "6d1f251c76db10d5a5797c1f6845b50f";
    // Start is called before the first frame update
    void Start()
    {
        string link = "https://stockx.com/adidas-yeezy-boost-350-turtle-dove-2022";
        StartCoroutine(affiliateAPIcall(link));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public string souvrnAffiliateLinkConverter(string link)
    {
        link = WWW.EscapeURL(link);
        print("encoded link is "+link);
        string affiliateUrl = "http://redirect.viglink.com?u="+link+"&key="+apiKey;
        return affiliateUrl;
    }


    IEnumerator affiliateAPIcall(string uri)
    {
        uri = souvrnAffiliateLinkConverter(uri);
        print("sovrn link is "+uri);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    break;
            }
        }
    }
}
