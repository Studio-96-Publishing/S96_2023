using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    AsyncOperation async;
    public bool mIsAppLeft;
    //public string thisURL;

    void Start()
    {
        async = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        async.allowSceneActivation = false;
    }

    void Update()
    {

    }
    public void PlayButton()
    {
        async.allowSceneActivation = true;
    }
    public void OpenAppStore() //OtherGame in the tutorial
    {
#if PLATFORM_ANDROID
        Application.OpenURL("https://play.google.com/store/apps/details?id=com.instagram.android");
#else
        Application.OpenURL("https://apps.apple.com/us/app/instagram/id389801252");
#endif
    }

    void OnApplicationFocus(bool hasFocus)
    {
        mIsAppLeft = !hasFocus;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        mIsAppLeft = pauseStatus;
    }
    public void Instagram()
    {
        StartCoroutine(OpenInstagram()); //string AppURL, string AppWebpage
    }

    IEnumerator OpenInstagram()//string AppURL, string AppWebpage)
    {
        Application.OpenURL("instagram://user?username=indirawrs"); //AppURL);
        yield return new WaitForSeconds(1f);
        if (mIsAppLeft)
            mIsAppLeft = false;
        else
            Application.OpenURL("https://www.instagram.com/indirawrs");//AppWebpage
    }

    public void Twitter()
    {
        StartCoroutine(OpenTwitter()); //string AppURL, string AppWebpage
    }

    IEnumerator OpenTwitter()//string AppURL, string AppWebpage)
    {
        Application.OpenURL("twitter://user?screen_name=indirawrs"); //AppURL);
        yield return new WaitForSeconds(1f);
        if (mIsAppLeft)
            mIsAppLeft = false;
        else
            Application.OpenURL("https://www.twitter.com/indirawrs");//AppWebpage
    }

    public void Spotify()
    {
        StartCoroutine(OpenSpotify()); //string AppURL, string AppWebpage
    }

    IEnumerator OpenSpotify()//string AppURL, string AppWebpage)
    {
        Application.OpenURL("https://open.spotify.com/album/1wIuFndiINuEZLFg3OIWGf"); //AppURL);
        yield return new WaitForSeconds(1f);
        /*if (mIsAppLeft)
            mIsAppLeft = false;
        else
            Application.OpenURL("https://open.spotify.com/album/1wIuFndiINuEZLFg3OIWGf");//AppWebpage
    */
    }

    public void AnyLink(string thisURL)
    {
        StartCoroutine(OpenAnyLink(thisURL)); //string AppURL, string AppWebpage
    }

    IEnumerator OpenAnyLink(string thisURL)//string AppURL, string AppWebpage)
    {
        Application.OpenURL(thisURL);
        yield return new WaitForSeconds(1f);
        /*if (mIsAppLeft)
            mIsAppLeft = false;
        else
            Application.OpenURL("https://open.spotify.com/album/1wIuFndiINuEZLFg3OIWGf");//AppWebpage
    */
    }
}
