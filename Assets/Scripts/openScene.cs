using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class openScene : MonoBehaviour
{
    public string whichScene;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void goToNextScene(string whichScene)
    {
        SceneManager.LoadScene(whichScene);
        Debug.Log("NextSceneTriggered " + whichScene);

    }
}
