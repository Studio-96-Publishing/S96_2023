using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Networking;
using Unity.Services.Core;
using Unity.Services.Analytics;
using UnityEngine.Analytics;
using Unity.Services.Analytics;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using Firebase.Firestore;
using Firebase.Extensions;
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


public class CenterFold_SimpleCloudRecoEventHandler : MonoBehaviour
{
    
    CloudRecoBehaviour mCloudRecoBehaviour;
    private ObjectTracker mImageTracker;
    bool mIsScanning = false;
    string mTargetMetadata = "";

    public GameObject accessDB;

    public ImageTargetBehaviour ImageTargetTemplate;

    private static int scanCount = 0;

    public string userEmail = "";

    public string winnerUser;

    FirebaseFirestore database;

    private IAmazonDynamoDB client;
    private DynamoDBContext context;
    private AWSCredentials credentials;

    private string aws_cognito_id = "us-east-1:ab5f71b6-28e0-4b43-8446-a2515d583680";

    private static Table sheKicksTable;
    private static string tableNameDB = "she-kicks-links-db";

    public string stockxlink = "";
    public string goatlink = "";
    public string brandlink = "";

    public string buttonOneLink = "";
    public string buttonTwoLink = "";
    public string buttonThreeLink = "";
    public string buttonFourLink = "";
    public string buttonFiveLink = "";


    List<string> links = new List<string>();


    public Sprite stockXSprite;
    public Sprite goatSprite;
    public Sprite flightClubSprite;
    public Sprite kicksCrewSprite;
    public Sprite stadiumGoodsSprite;

    public string videolink = "";

    public bool isShoeBoxBool;
    public bool isVideoBool;

    private float width = 230f;
    private float height = 230f;

    public void Start()
    {
        print("Shoebox at start is: "+isShoeBoxBool);

        UnityInitializer.AttachToGameObject(this.gameObject);

	    credentials = new CognitoAWSCredentials (
		    aws_cognito_id, // Identity Pool ID
		    RegionEndpoint.USEast1 // Region
	    );

	    client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast1);
	    context = new DynamoDBContext(client);

        Debug.Log("CONNECTED TO DB");
    }

    // Register cloud reco callbacks
    async void Awake()
    {
        database = FirebaseFirestore.DefaultInstance;

        if(RegisterManager.userEmail != null){
            print("RegisterManager userEmail");
            userEmail = RegisterManager.userEmail;
        }
        else if(LoginManager.userEmail != null){
            print("LoginManager userEmail");
            userEmail = LoginManager.userEmail;
        }
        else{
            print("SignInHandler userEmail");
            userEmail = SignInHandler.userEmail;
        }

        print("UserEmail is: "+userEmail);

        mCloudRecoBehaviour = GetComponent<CloudRecoBehaviour>();
        mCloudRecoBehaviour.RegisterOnInitializedEventHandler(OnInitialized);
        mCloudRecoBehaviour.RegisterOnInitErrorEventHandler(OnInitError);
        mCloudRecoBehaviour.RegisterOnUpdateErrorEventHandler(OnUpdateError);
        mCloudRecoBehaviour.RegisterOnStateChangedEventHandler(OnStateChanged);
        mCloudRecoBehaviour.RegisterOnNewSearchResultEventHandler(OnNewSearchResult);

        await UnityServices.InitializeAsync();
    }
    //Unregister cloud reco callbacks when the handler is destroyed
    void OnDestroy()
    {
        mCloudRecoBehaviour.UnregisterOnInitializedEventHandler(OnInitialized);
        mCloudRecoBehaviour.UnregisterOnInitErrorEventHandler(OnInitError);
        mCloudRecoBehaviour.UnregisterOnUpdateErrorEventHandler(OnUpdateError);
        mCloudRecoBehaviour.UnregisterOnStateChangedEventHandler(OnStateChanged);
        mCloudRecoBehaviour.UnregisterOnNewSearchResultEventHandler(OnNewSearchResult);
    }

    public void OnInitialized(TargetFinder targetFinder)
    {
        Debug.Log("Cloud Reco initialized");
    }
    public void OnInitError(TargetFinder.InitState initError)
    {
        Debug.Log("Cloud Reco init error " + initError.ToString());
    }
    public void OnUpdateError(TargetFinder.UpdateState updateError)
    {
        Debug.Log("Cloud Reco update error " + updateError.ToString());
    }

    public void OnStateChanged(bool scanning)
    {
        mIsScanning = scanning;
        if (scanning)
        {
            // clear all known trackables
            var tracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
            tracker.GetTargetFinder<ImageTargetFinder>().ClearTrackables(false);
        }
    }

    
    // WILL WORK ON THIS SOME MORE
    // Here we handle a cloud target recognition event
    public void OnNewSearchResult(TargetFinder.TargetSearchResult targetSearchResult)
    {
         TargetFinder.CloudRecoSearchResult cloudRecoSearchResult =
            (TargetFinder.CloudRecoSearchResult)targetSearchResult;
        // do something with the target metadata
        print("cloud result is: "+cloudRecoSearchResult);
        mTargetMetadata = cloudRecoSearchResult.TargetName;
        print("Metadata is "+mTargetMetadata);

        //mCloudRecoBehaviour.enabled = false;

        // implement analytics hashmap
        scanCount++;
        Dictionary<string, object> parameters = new Dictionary<string, object>(){{"scanCount", scanCount}};
 
        AnalyticsService.Instance.CustomData(mTargetMetadata, parameters); 

        AnalyticsService.Instance.Flush();

        string replyId = mTargetMetadata.Trim();

        GameObject textObject = ImageTargetTemplate.gameObject.transform.GetChild(0).gameObject;
        //textObject.GetComponent<TextMeshPro>().SetText(sneakerNameTarget);

        print("Calling Get Operation with: "+mTargetMetadata);

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

        PerformGetOperation(targetSearchResult, mTargetMetadata, ImageTargetTemplate, request);

    }


    public IEnumerator OnNewSearchResultCoroutine(TargetFinder.TargetSearchResult targetSearchResult, ImageTargetBehaviour ImageTargetTemplate){

        if (ImageTargetTemplate)
        {
            print("inside ImageTargetTemplate");

            if(isShoeBoxBool){
                print("image Is ShoeBox");

                //yield return new WaitForSeconds(1);

                yield return StartCoroutine(getWinner());

                if(isWinner(userEmail)){
                    SceneManager.LoadScene("ShoeBoxWin");
                }
                else{
                    SceneManager.LoadScene("ShoeBoxLose");
                }

                yield return null;
            }
            else{

                print("image is not shoebox");

                //yield return new WaitForSeconds(1);
                // enable the new result with the same ImageTargetBehaviour: 
                ObjectTracker tracker = TrackerManager.Instance.GetTracker<ObjectTracker>();

                //yield return new WaitForSeconds(2);

                tracker.GetTargetFinder<ImageTargetFinder>().EnableTracking(targetSearchResult, ImageTargetTemplate.gameObject);

                bool isVideoVar = videolink.Length > 0;

                print("image has video? "+isVideoVar);

                if(isVideoVar) {
                    print("Playing Video");
                    //yield return new WaitForSeconds(1);
                    //playVideo();

                    yield return null;
                }
                else{ print("Is Shop Target"); }

                //yield return new WaitForSeconds(2);

                yield return null;
            }
        }
        yield break;
    }

    public void OnNewSearchResult(TargetFinder.TargetSearchResult targetSearchResult, ImageTargetBehaviour ImageTargetTemplate){

        if (ImageTargetTemplate)
        {
            
               // if(isVideoBool) {
                    //print("In Play Video");

                    //GameObject ImageTarget = GameObject.Find("ImageTarget");

                    //ImageTarget.GetComponentInChildren<VideoPlayer>().url = videolink;

                    //ObjectTracker tracker = TrackerManager.Instance.GetTracker<ObjectTracker>();

                    //tracker.GetTargetFinder<ImageTargetFinder>().EnableTracking(targetSearchResult, ImageTargetTemplate.gameObject);

                    //return;
              //  }

                print("image is not shoebox");

                //yield return new WaitForSeconds(1);
                // enable the new result with the same ImageTargetBehaviour: 
                ObjectTracker tracker = TrackerManager.Instance.GetTracker<ObjectTracker>();

                //yield return new WaitForSeconds(2);

                tracker.GetTargetFinder<ImageTargetFinder>().EnableTracking(targetSearchResult, ImageTargetTemplate.gameObject);


                /**
                bool isVideoVar = videolink.Length > 0;

                print("image has video? "+isVideoVar);

                if(isVideoVar) {
                    print("Playing Video");
                    //yield return new WaitForSeconds(1);
                    playVideo();
                }
                else{ 
                    print("Is Shop Target"); 
                    fillButtons();
                }
                **/
        }
    }

    public void getWinnerVoid(string userEmail){
        print("In Get Winner Void, UserEmail is: "+userEmail);

        //userEmail = "smpatt@gmail.com";

        print("is database null? "+database == null);

        try {
            database.Document("lotteryWinners/currentWinner").GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                CurrentWinner currentWinner = task.Result.ConvertTo<CurrentWinner>();
                winnerUser = currentWinner.winner;
                print("monthly winner is: "+winnerUser);

                if(string.Equals(userEmail, winnerUser)){
                    SceneManager.LoadScene("ShoeBoxWin");
                }
                else{
                    SceneManager.LoadScene("ShoeBoxLose");
                }
            });
        }
        catch(Exception e){
            Debug.LogError("GetWinnerVoid Exception is: "+e);
        }

    }

    public IEnumerator getWinner(){
        print("In Get Winner");

        yield return database.Document("lotteryWinners/currentWinner").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            CurrentWinner currentWinner = task.Result.ConvertTo<CurrentWinner>();
            winnerUser = currentWinner.winner;
            print("monthly winner is: "+winnerUser);
        });

        yield return null;
    }

    public bool isWinner(string userEmail){
        return string.Equals(userEmail, winnerUser);
    }

    public void playVideo(UnityEngine.Video.VideoPlayer videoPlayer, string videolink){

        // Play on awake defaults to true. Set it to false to avoid the url set
        // below to auto-start playback since we're in Start().
        videoPlayer.playOnAwake = false;

        // By default, VideoPlayers added to a camera will use the far plane.
        // Let's target the near plane instead.
        //videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.CameraNearPlane;

        // This will cause our Scene to be visible through the video being played.
        videoPlayer.targetCameraAlpha = 0.99F;

        // Set the video to play. URL supports local absolute or relative paths.
        // Here, using absolute.

        videoPlayer.url = videolink;

        //videoPlayer.url = "https://dl.dropbox.com/s/40waa54u9zho8r7/165.mp4";

        // Skip the first 100 frames.
        videoPlayer.frame = 100;

        // Restart from beginning when done.
        videoPlayer.isLooping = true;

        // Each time we reach the end, we slow down the playback by a factor of 10.
        videoPlayer.loopPointReached += EndReached;

        // Start playback. This means the VideoPlayer may have to prepare (reserve
        // resources, pre-load a few frames, etc.). To better control the delays
        // associated with this preparation one can use videoPlayer.Prepare() along with
        // its prepareCompleted event.
        videoPlayer.Play();

        
    }

    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        vp.playbackSpeed = vp.playbackSpeed / 10.0F;
    }

    void OnGUI()
    {
        // Display current 'scanning' status
        //GUI.Box(new Rect(100, 100, 200, 50), mIsScanning ? "Scanning" : "Not scanning");
        // Display metadata of latest detected cloud-target
        //GUI.Box(new Rect(100, 200, 200, 50), "Metadata: " + mTargetMetadata);
        // If not scanning, show button
        // so that user can restart cloud scanning
        
        if (!mIsScanning)
        {
           // if (GUI.Button(new Rect(100, 300, 200, 50), "Restart Scanning")){
                // Restart TargetFinder
                mCloudRecoBehaviour.CloudRecoEnabled = true;
            //}
        }
        
    }

    
    public void restartScannerOn(){
        print("is scanning: "+mIsScanning);

        if (!mIsScanning)
        {
            // Restart TargetFinder
            mCloudRecoBehaviour.CloudRecoEnabled = true;
        }
    }

    public void restartScannerOff(){
        print("is scanning: "+mIsScanning);

        if (mIsScanning)
        {
            // Restart TargetFinder
            mCloudRecoBehaviour.CloudRecoEnabled = false;
        }
    }
    


     public IEnumerator PerformGetOperationAsync(TargetFinder.TargetSearchResult targetSearchResult, string sneakerNameTarget, ImageTargetBehaviour ImageTargetTemplate, QueryRequest request){
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

                    

                    OnNewSearchResult(targetSearchResult, ImageTargetTemplate);
                }
            }
        });

        print("Finished Get Methods");

        //StartCoroutine(OnNewSearchResultCoroutine(targetSearchResult, ImageTargetTemplate));

        yield break;
     }

     private IEnumerator loadVideoFromThisURL(string _url, UnityEngine.Video.VideoPlayer myVideoPlayer)
     {
        UnityWebRequest _videoRequest = UnityWebRequest.Get (_url);

        yield return _videoRequest.SendWebRequest();

        if (_videoRequest.isDone == false || _videoRequest.error != null)
        {   Debug.Log ("Request = " + _videoRequest.error );}

        Debug.Log ("Video Done - " + _videoRequest.isDone);

        byte[] _videoBytes = _videoRequest.downloadHandler.data;

        string _pathToFile = Path.Combine (Application.persistentDataPath, "movie.mp4");
        File.WriteAllBytes (_pathToFile, _videoBytes);
        Debug.Log (_pathToFile);
        StartCoroutine(this.playThisURLInVideo (_pathToFile, myVideoPlayer));
        yield return null;
     }


     private IEnumerator playThisURLInVideo(string _url, UnityEngine.Video.VideoPlayer myVideoPlayer)
     {
        Debug.Log ("Inside playThisURLInVideo()");
        myVideoPlayer.source = UnityEngine.Video.VideoSource.Url;
        myVideoPlayer.url = _url;
        myVideoPlayer.Prepare ();

        while (myVideoPlayer.isPrepared == false)
        {   yield return null;}

       //myVideoPlayer.gameObject.GetComponent<MeshRenderer>().enabled = true;

        Debug.Log ("Video should play");
        //myVideoPlayer.Play ();
     }

    /**
     private void WeblPrepared(VideoPlayer source)
    {
        startedPlayingWebgl = true;

        StartCoroutine(WebGLPlay());
        logTest = "Playing!!";
    }

    IEnumerator WebGLPlay() //The prepare not respond so, i forced to play after some seconds
    {
        yield return new WaitForSeconds(2f);
        StartPlayingWebgl();
    }

    private void StartPlayingWebgl()
    {
        _events.OnVideoReadyToStart.Invoke();

        if (playUsingInternalDevicePlayer && Application.isMobilePlatform) //Works in mobiles only!!
        {
            //Play using the internal player of the device 
            StartCoroutine(HandHeldPlayback());
        }
        else
        {
            StartPlayback();
        }
    }

    private void StartPlayback()
    {
        //Render to more materials
        if (objectsToRenderTheVideoImage.Length > 0)
        {
            foreach (GameObject obj in objectsToRenderTheVideoImage)
            {
                    obj.GetComponent<Renderer>().material.mainTexture = videoPlayer.texture;
            }
        }
    }

    **/

     public void PerformGetOperation(TargetFinder.TargetSearchResult targetSearchResult, string sneakerNameTarget, ImageTargetBehaviour ImageTargetTemplate, QueryRequest request){
        string prefix = "ARTriggers_SheKicks/ImageTarget/Canvas/ButtonPanel/Button";
        client.QueryAsync(request,(result)=>{
            //resultText.text = string.Format("No. of reads used (by query in FindRepliesForAThread) {0}\n",result.Response.ConsumedCapacity.CapacityUnits);
            print("result count is "+result.Response.Items.Count);

            int count = 0;
            foreach (Dictionary<string, AttributeValue> item in result.Response.Items)
            {
                print("item is "+item);

                foreach (string s in item.Keys) {
                    print("key is: "+s);

                    if(string.Equals(s, "videolink")) {
                        videolink = item["videolink"].S;
                        if(videolink.Length > 0) {
                            //videolink = videolink.Substring(0, videolink.Length-5);
                            //videolink = videolink.Replace("www", "dl");
                            print("videolink is "+videolink);
                            InAppBrowser.OpenURL(videolink);

                            //GameObject ImageTarget = GameObject.Find("ImageTarget");
                            //UnityEngine.Video.VideoPlayer video =  ImageTarget.GetComponentInChildren<VideoPlayer>();
                            //playVideo(video, videolink);
                            //StartCoroutine(loadVideoFromThisURL(videolink, video));
                        }
                        return;        
                   }
                   else if(string.Equals(s, "isShoeBox")){
                        string shoeboxVal = item["isShoeBox"].S;
                        print("shoeboxVal is "+shoeboxVal);
                        //isShoeBoxBool = true;
                        if(string.Equals(shoeboxVal, userEmail)) isShoeBoxBool = true; else isShoeBoxBool = false;
                        print("is shoebox marker? "+isShoeBoxBool);
                        if(isShoeBoxBool) SceneManager.LoadScene("ShoeBoxWin"); else SceneManager.LoadScene("ShoeBoxLose");
                        //if(isShoeBoxBool) getWinnerVoid(userEmail);
                        return;
                    }
                    else {
                        if(s.Contains("link")){
                            string link = item[s].S;
                            count++;
                            string shopButtonName = prefix + count.ToString();
                            GameObject currentButton = GameObject.Find(shopButtonName);
                            currentButton.SetActive(true);

                            if(count == 1) {
                                buttonOneLink = link;
                                print("buttonOne link is: "+buttonOneLink);
                            }
                            else if(count == 2) {
                                buttonTwoLink = link;
                                print("buttonTwo link is: "+buttonTwoLink);
                            }
                            else if(count == 3) {
                                buttonThreeLink = link;
                                print("buttonThree link is: "+buttonThreeLink);
                            }
                            else if(count == 4) {
                                buttonFourLink = link;
                                print("buttonFour link is: "+buttonFourLink);
                            }
                            else {
                                buttonFiveLink = link;
                                print("buttonFive link is: "+buttonFiveLink);
                            }

                            if(link.Contains("souvrn")) {
                                if(string.Equals(s, "stockx-link")) {
                                    UnityEngine.UI.Image m_Image = currentButton.GetComponent<UnityEngine.UI.Image>();
                                    m_Image.sprite = stockXSprite;
                                    var theBarRectTransform = m_Image.transform as RectTransform;
                                    theBarRectTransform.sizeDelta = new Vector2 (width, height);
                                }
                                
                                if(string.Equals(s, "goat-link")) {
                                    UnityEngine.UI.Image m_Image = currentButton.GetComponent<UnityEngine.UI.Image>();
                                    m_Image.sprite = goatSprite;
                                    var theBarRectTransform = m_Image.transform as RectTransform;
                                    theBarRectTransform.sizeDelta = new Vector2 (width, height);
                                }

                                if(string.Equals(s, "flightclub-link")) {
                                    UnityEngine.UI.Image m_Image = currentButton.GetComponent<UnityEngine.UI.Image>();
                                    m_Image.sprite = flightClubSprite;
                                    var theBarRectTransform = m_Image.transform as RectTransform;
                                    theBarRectTransform.sizeDelta = new Vector2 (width, height);
                                }

                                if(string.Equals(s, "kickscrew-link")) {
                                    UnityEngine.UI.Image m_Image = currentButton.GetComponent<UnityEngine.UI.Image>();
                                    m_Image.sprite = kicksCrewSprite;
                                    var theBarRectTransform = m_Image.transform as RectTransform;
                                    theBarRectTransform.sizeDelta = new Vector2 (width, height);
                                }

                                if(string.Equals(s, "stadiumgoods-link")) {
                                    UnityEngine.UI.Image m_Image = currentButton.GetComponent<UnityEngine.UI.Image>();
                                    m_Image.sprite = stadiumGoodsSprite;
                                    var theBarRectTransform = m_Image.transform as RectTransform;
                                    theBarRectTransform.sizeDelta = new Vector2 (width, height);
                                }
                            }
                            else {
                                if(string.Equals(s, "stockx-link")) {
                                    UnityEngine.UI.Image m_Image = currentButton.GetComponent<UnityEngine.UI.Image>();
                                    m_Image.sprite = stockXSprite;
                                    var theBarRectTransform = m_Image.transform as RectTransform;
                                    theBarRectTransform.sizeDelta = new Vector2 (width, height);
                                }
                                
                                if(string.Equals(s, "goat-link")) {
                                    UnityEngine.UI.Image m_Image = currentButton.GetComponent<UnityEngine.UI.Image>();
                                    m_Image.sprite = goatSprite;
                                    var theBarRectTransform = m_Image.transform as RectTransform;
                                    theBarRectTransform.sizeDelta = new Vector2 (width, height);
                                }

                                if(string.Equals(s, "flightclub-link")) {
                                    UnityEngine.UI.Image m_Image = currentButton.GetComponent<UnityEngine.UI.Image>();
                                    m_Image.sprite = flightClubSprite;
                                    var theBarRectTransform = m_Image.transform as RectTransform;
                                    theBarRectTransform.sizeDelta = new Vector2 (width, height);
                                }

                                if(string.Equals(s, "kickscrew-link")) {
                                    UnityEngine.UI.Image m_Image = currentButton.GetComponent<UnityEngine.UI.Image>();
                                    m_Image.sprite = kicksCrewSprite;
                                    var theBarRectTransform = m_Image.transform as RectTransform;
                                    theBarRectTransform.sizeDelta = new Vector2 (width, height);
                                }

                                if(string.Equals(s, "stadiumgoods-link")) {
                                    UnityEngine.UI.Image m_Image = currentButton.GetComponent<UnityEngine.UI.Image>();
                                    m_Image.sprite = stadiumGoodsSprite;
                                    var theBarRectTransform = m_Image.transform as RectTransform;
                                    theBarRectTransform.sizeDelta = new Vector2 (width, height);
                                }
                            }
                        }
                    }

                }
            }

            //fillButtons();

            //OnNewSearchResult(targetSearchResult, ImageTargetTemplate);
        });

        //Invoke("OnNewSearchResult(targetSearchResult, ImageTargetTemplate)", 5f);

        OnNewSearchResult(targetSearchResult, ImageTargetTemplate);

        //fillButtons();
     }

    public void callStockXLink() {
        print("Calling in App Browser");
        //Application.OpenURL(stockxlink);
        InAppBrowser.OpenURL(stockxlink);
    }

    public void callGoatLink() {
        print("Calling in App Browser");
        //Application.OpenURL(stockxlink);
        InAppBrowser.OpenURL(goatlink);
    }

    public void callBrandLink() {
        print("Calling in App Browser");
        //Application.OpenURL(stockxlink);
        InAppBrowser.OpenURL(brandlink);
    }

    public void callButtonOneLink() {
        print("Calling in App Browser");
        //Application.OpenURL(stockxlink);
        InAppBrowser.OpenURL(buttonOneLink);
    }

    public void callButtonTwoLink() {
        print("Calling in App Browser");
        //Application.OpenURL(stockxlink);
        InAppBrowser.OpenURL(buttonTwoLink);
    }

    public void callButtonThreeLink() {
        print("Calling in App Browser");
        //Application.OpenURL(stockxlink);
        InAppBrowser.OpenURL(buttonThreeLink);
    }

    public void callButtonFourLink() {
        print("Calling in App Browser");
        //Application.OpenURL(stockxlink);
        InAppBrowser.OpenURL(buttonFourLink);
    }

    public void callButtonFiveLink() {
        print("Calling in App Browser");
        //Application.OpenURL(stockxlink);
        InAppBrowser.OpenURL(buttonFiveLink);
    }

    public void resetLinksOff(){
        buttonOneLink = "";
        buttonTwoLink = "";
        buttonThreeLink = "";
        buttonFourLink = "";
        buttonFiveLink = "";
    }

    public void fillButtons() {
        print("CALLING FILL BUTTONS");
        int count = 0;
        string prefix = "ARTriggers_SheKicks/ImageTarget/Canvas/ShopButton";
        print("size of list is "+links.Count);
        foreach (string s in links) {
            print("link is "+s);
            count++;
            string shopButtonName = prefix + count.ToString();
            print("buttonName is "+shopButtonName);
            GameObject currentButton = GameObject.Find(shopButtonName);
            currentButton.SetActive(true);

            print(shopButtonName + " set to active");
            UnityEngine.UI.Image m_Image = currentButton.GetComponent<UnityEngine.UI.Image>();

            if(s.Contains("stockx")) m_Image.sprite = stockXSprite;
            else if(s.Contains("goat")) m_Image.sprite = goatSprite;
            else  m_Image.sprite = flightClubSprite;
        }
    }
}

