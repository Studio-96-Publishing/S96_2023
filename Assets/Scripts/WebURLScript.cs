using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebURLScript : MonoBehaviour
{
    //public string triggerThisURL;

    // Start is called before the first frame update
    void Start()
    {
        
    }


    public void ImageTriggerURL(string triggerThisURL)
    {
        Application.OpenURL(triggerThisURL);
        Debug.Log("this url is " + triggerThisURL);
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
