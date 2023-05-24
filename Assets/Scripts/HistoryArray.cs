using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HistoryArray : MonoBehaviour
{
    public GameObject[] historyList;

    void Start()
    {
        for (int i = historyList.Length; i < historyList.Length; i++)
        {
            historyList[i].SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
