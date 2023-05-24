using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapUIManager : MonoBehaviour
{
    public GameObject[] blackPins;
    public GameObject[] pinPanels;

    public void DisableBlackPinsAndPinPanels(){
        
        foreach(GameObject blackPin in blackPins){
            blackPin.SetActive(false);
        }
        foreach(GameObject pinPanel in pinPanels){
            pinPanel.SetActive(false);
        }
    }
}
