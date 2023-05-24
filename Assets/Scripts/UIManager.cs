using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    //Screen object variables
    public GameObject loginUI;
    public GameObject registerUI;
    public GameObject buttonPanel;

    public GameObject buttonOne;
    public GameObject buttonTwo;
    public GameObject buttonThree;
    public GameObject buttonFour;
    public GameObject buttonFive;

    string prefix = "ARTriggers_SheKicks/ImageTarget/Canvas/ButtonPanel/Button";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    //Functions to change the login screen UI
    public void LoginScreen() //Back button
    {
        loginUI.SetActive(true);
        registerUI.SetActive(false);
    }
    public void RegisterScreen() // Regester button
    {
        loginUI.SetActive(false);
        registerUI.SetActive(true);
    }

    public void S96LogIn(){
        print("S96 Log Here In Button Clicked");
        SceneManager.LoadScene("GoogleSignInScene");
    }

    public void turnOffPanel(){
        
        
        for(int count=1; count<=5; count++){
            string shopButtonName = prefix + count.ToString();
            GameObject currentButton = GameObject.Find(shopButtonName);
            //UnityEngine.UI.Image m_Image = currentButton.GetComponent<UnityEngine.UI.Image>();
            //m_Image.sprite = null;
            if(currentButton.active) currentButton.SetActive(false);
        }
        
    
        buttonPanel.SetActive(false);
    }

    public void turnOnPanel(){
        
        /**
        for(int count=1; count<=5; count++){
            string shopButtonName = prefix + count.ToString();
            GameObject currentButton = GameObject.Find(shopButtonName);
            currentButton.SetActive(true);
        }
        **/
        
        buttonPanel.SetActive(true);
    }

    public void claimButton(){
        InAppBrowser.OpenURL("https://learnmore.studio96publishing.com/she-kicks-lottery-winner");
        SceneManager.LoadScene("CameraScreenBegin");
    }

    public void s96DontHaveABook(){
        InAppBrowser.OpenURL("https://studio96publishing.com/collections/titles");
        SceneManager.LoadScene("CameraScreenBegin");
    }

    public void s96PrintSampleImages(){
        InAppBrowser.OpenURL("https://learnmore.studio96publishing.com/ar-samples");
        SceneManager.LoadScene("CameraScreenBegin");
    }

    public void switchToAboutPage(){
        print("About Button Clicked");
        SceneManager.LoadScene("CameraScreenBegin");
    }

    public void switchSceneToLogInPage(){
        print("Log Out Button Clicked");
        SceneManager.LoadScene("LogInScene");
    }
}
