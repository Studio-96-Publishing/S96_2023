using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using System.Threading.Tasks;
using System.Text;
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Auth;
using UnityEngine.UI;
using Google;
using System.Net.Http;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using System.Reflection;
using UnityEditor.iOS.Xcode;
using AppleAuth.Editor;
#endif

public class SignInHandler : MonoBehaviour

{
    public static SignInHandler Instance { get; private set; }

    private IAppleAuthManager appleAuthManager;

    private bool isAppleAuthInitialized { get { return appleAuthManager != null; } }

    // SheKicksWebApp Client Id: 792019190702-kj4o7qjf2neplboa0tml4ndrpon1ab4r.apps.googleusercontent.com
    //public string GoogleWebAPI = "240268915398-k3efmlouh1srd6l5ljej0efa300snjma.apps.googleusercontent.com";

    public string GoogleWebAPI = "240268915398-3ch0hhbsiiuhffpcn0j77hh9jbsr9csv.apps.googleusercontent.com"; 


    private GoogleSignInConfiguration configuration;

    Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;
    
    public Firebase.Auth.FirebaseAuth auth;
    public Firebase.Auth.FirebaseUser user;

    private Text UsernameTxt,UserEmailTxt;

    private Text forgetPassEmail;

    private Image UserProfilePic;

    private string imageUrl;

    private GameObject LoginScreen, ProfileScreen;

    public GameObject CameraScreenBegin, SignInScreen;

    bool switchToBegin = false;

    public string AppleUserIdKey {get; private set; }

    public static string userEmail;

    void Awake()
    {
        GoogleWebAPI = "240268915398-3ch0hhbsiiuhffpcn0j77hh9jbsr9csv.apps.googleusercontent.com"; 
        
        configuration=new GoogleSignInConfiguration
        {
            WebClientId=GoogleWebAPI,
            RequestIdToken=true
        };

        Instance = this;

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                //If they are avalible Initialize Firebase
                InitFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    void Start()
    {
        //Check that all of the necessary dependencies for Firebase are present on the system

        if (AppleAuthManager.IsCurrentPlatformSupported)
        {
            // Creates a default JSON deserializer, to transform JSON Native responses to C# instances
            var deserializer = new PayloadDeserializer();
            // Creates an Apple Authentication manager with the deserializer
            this.appleAuthManager = new AppleAuthManager(deserializer);    
        }
    }

    void Update()
    {
        Scene m_Scene = SceneManager.GetActiveScene();
        string sceneName = m_Scene.name;

        //Debug.Log("Scene is: "+sceneName); 

        if(switchToBegin && !string.Equals("CameraScreenBegin", sceneName)) {
            Debug.Log("About to switch to camera scene");
            SceneManager.LoadScene("CameraScreenBegin");
            Debug.Log("Switching to Camera scene");
        }

        // Updates the AppleAuthManager instance to execute
        // pending callbacks inside Unity's execution loop
        if (isAppleAuthInitialized)
        {
            appleAuthManager.Update();
        }
    }

    
    public void Initialize()
    {
        if (isAppleAuthInitialized)
        {
            //lastUserID = GetLastSavedUserID();
            PerformQuickLoginWithFirebase();
        }
        else
        {
            Debug.LogError("Platform not compatible");
        }
    }
    

    public void SignInApple()
    {
        Debug.Log("Calling Apple SignIn");

        if (isAppleAuthInitialized)
        {
            PerformLoginWithAppleIdAndFirebase();   
        }
        else
        {
            Debug.Log("System is not natively compatible with the Apple SignIn, so the web view is used");

            //AuthenticationManager.Instance.AuthenticateWithAppleFromWebView();
        }
    }

    public void SignOutApple()
    {
        Debug.Log("Calling Apple SignOut");

        UnregisterRevokedCallback();
    }

    private void RegisterRevokedCallback()
    {
        if (isAppleAuthInitialized)
        {
            appleAuthManager.SetCredentialsRevokedCallback(result =>
            {
                // Sign in with Apple Credentials were revoked.
                // Discard credentials/user id and go to login screen.
                //AuthenticationManager.Instance.SignOut();
                auth.SignOut();
                switchToBegin = false;
                SceneManager.LoadScene("LogInScene");
            });
        }
    }

    private void UnregisterRevokedCallback()
    {
        if (isAppleAuthInitialized)
        {
            appleAuthManager.SetCredentialsRevokedCallback(null);
        }
    }

    void InitFirebase()
    {
        Debug.Log("Inside InitFirebase");
        auth=Firebase.Auth.FirebaseAuth.DefaultInstance;
        DatabaseReference DBreference = Firebase.Database.FirebaseDatabase.DefaultInstance.RootReference;

        if (auth.CurrentUser != user) {
            Debug.Log("Inside if condition");
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && user != null) {
                Debug.Log("Signed out " + user.UserId);
                SceneManager.LoadScene("GoogleSignInScene");
                switchToBegin = false;
            }
            user = auth.CurrentUser;
            userEmail = user.UserId;
            if (signedIn) {
                Debug.Log("Signed in " + user.UserId);
                try{
                    //SceneManager.LoadScene("CameraScreenBegin");
                    switchToBegin = true;
                    Debug.Log("switchToBegin is: " + switchToBegin);
                }
                catch(Exception e){
                    Debug.LogError("Exception is: "+e);
                }
            }
        }
        //AuthStateChanged(this, null);
        //auth.StateChanged += AuthStateChanged;
    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user) {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && user != null) {
                Debug.Log("Signed out " + user.UserId);
                CameraScreenBegin.SetActive(false);
                SignInScreen.SetActive(true);
            }
            user = auth.CurrentUser;
            userEmail = user.UserId;
            if (signedIn) {
                Debug.Log("Signed in " + user.UserId);
                CameraScreenBegin.SetActive(true);
                SignInScreen.SetActive(false);
            }
        }
    }

    public void GoogleSignInClick()
    {
        Debug.Log("WebClientId is: ");
        Debug.Log(configuration.WebClientId);
        GoogleSignIn.Configuration=configuration;
        GoogleSignIn.Configuration.UseGameSignIn=false;
        GoogleSignIn.Configuration.RequestIdToken=true;
        GoogleSignIn.Configuration.RequestEmail=true;

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnGoogleAuthenticatedFinished);
    }

    void OnGoogleAuthenticatedFinished(Task<GoogleSignInUser> task)
    {
        if(task.IsFaulted)
        {
            Debug.LogError("Fault");
        }
        else if(task.IsCanceled)
        {
            Debug.LogError("Login Cancel");
        }
        else{
            Firebase.Auth.Credential credential = Firebase.Auth.GoogleAuthProvider.GetCredential(task.Result.IdToken, null);

            auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
            {
                if(task.IsCanceled) {
                    Debug.LogError("SignInWithCredentialAsync was canceled");
                    return;
                }
                if(task.IsFaulted){
                    Debug.LogError("SignInWithCredentialAsync encountered an error: ");
                    return;
                }

                user = auth.CurrentUser;

                userEmail = user.UserId;

                SceneManager.LoadScene("CameraScreenBegin");

                //UsernameTxt.text=user.DisplayName;
                //UserEmailTxt.text=user.Email;

                //LoginScreen.SetActive(false);
                //ProfileScreen.SetActive(true);

                //StartCoroutine(LoadImage(CheckImageUrl(user.PhotoUrl.ToString())));

            });
        }
    }

    private string CheckImageUrl(string url){
        if(!string.IsNullOrEmpty(url)){
            return url;
        }

        return imageUrl;
    }

    IEnumerator LoadImage(string imageUri)
    {
        WWW www = new WWW(imageUri);
        yield return www;

        UserProfilePic.sprite = Sprite.Create(www.texture, new Rect(0,0,www.texture.width,www.texture.height), new Vector2(0,0));
    }

    public void S96SignIn(){
        print("S96 Sign In Button Clicked");
        SceneManager.LoadScene("LogInScene");
    }

    public void switchSceneToLogInPage(){
        if(auth != null && user != null) {
            auth.SignOut();
            switchToBegin = false;
        }

        SceneManager.LoadScene("LogInScene");
    }

    private static string GenerateRandomString(int length)
    {
        if (length <= 0)
        {
            throw new Exception("Expected nonce to have positive length");
        }         

        const string charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVXYZabcdefghijklmnopqrstuvwxyz-._";
        var cryptographicallySecureRandomNumberGenerator = new RNGCryptoServiceProvider();
        var result = string.Empty;
        var remainingLength = length;

        var randomNumberHolder = new byte[1];
        while (remainingLength > 0)
        {
            var randomNumbers = new List<int>(16);
            for (var randomNumberCount = 0; randomNumberCount < 16; randomNumberCount++)
            {
                cryptographicallySecureRandomNumberGenerator.GetBytes(randomNumberHolder);
                randomNumbers.Add(randomNumberHolder[0]);
            }

            for (var randomNumberIndex = 0; randomNumberIndex < randomNumbers.Count; randomNumberIndex++)
            {
                if (remainingLength == 0)
                {
                    break;
                }

                var randomNumber = randomNumbers[randomNumberIndex];
                if (randomNumber < charset.Length)
                {
                    result += charset[randomNumber];
                    remainingLength--;
                }
            }
        }

        return result;
    }

    private static string GenerateSHA256NonceFromRawNonce(string rawNonce)
    {
        var sha = new SHA256Managed();
        var utf8RawNonce = Encoding.UTF8.GetBytes(rawNonce);
        var hash = sha.ComputeHash(utf8RawNonce);

        var result = string.Empty;
        for (var i = 0; i < hash.Length; i++)
        {
            result += hash[i].ToString("x2");
        }

        return result;
    }

    public void PerformLoginWithAppleIdAndFirebase()
    {
        Debug.Log("Inside PerformLoginWithAppleIdAndFirebase");

        var rawNonce = GenerateRandomString(32);
        var nonce = GenerateSHA256NonceFromRawNonce(rawNonce);

        var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName, nonce);

        //var loginArgs = new AppleAuthLoginArgs(LoginOptions.None, nonce);

        //Debug.Log("Email is "+LoginOptions.IncludeEmail.ToString());
        //Debug.Log("Full Name is "+LoginOptions.IncludeFullName.ToString());
        //Debug.Log("loginArgs is "+loginArgs.ToString());

        this.appleAuthManager.LoginWithAppleId(
            loginArgs,
            credential =>
            {
                Debug.Log("login response received");
                var appleIdCredential = credential as IAppleIDCredential;
                if (appleIdCredential != null)
                {
                    Debug.Log("About to call PerformFirebaseAuthentication");
                    this.PerformFirebaseAuthentication(appleIdCredential, rawNonce);
                }
                else{
                    Debug.LogError("Credentials are null");
                }
            },
            error =>
            {
                // Something went wrong
                var v_authorizationErrorCode = error.GetAuthorizationErrorCode();
                Debug.LogError("Sign in with Apple failed " + v_authorizationErrorCode + " "+v_authorizationErrorCode.ToString() + " " + error.ToString());
                Debug.LogError("Apple Login encountered an error: " + error.LocalizedDescription + "\n" + error.LocalizedFailureReason + "\n" + error.LocalizedRecoverySuggestion);
            });
    }

    public void PerformQuickLoginWithFirebase()
    {
        Debug.Log("Inside PerformQuickLoginWithFirebase");

        var rawNonce = GenerateRandomString(32);
        var nonce = GenerateSHA256NonceFromRawNonce(rawNonce);

        var quickLoginArgs = new AppleAuthQuickLoginArgs(nonce);

        Debug.Log("quickloginArgs is "+quickLoginArgs.ToString());

        this.appleAuthManager.QuickLogin(
            quickLoginArgs,
            credential =>
            {
                var appleIdCredential = credential as IAppleIDCredential;
                Debug.Log("Apple Credentials Are: "+appleIdCredential.ToString());
                if (appleIdCredential != null)
                {
                    Debug.Log("About to call PerformFirebaseAuthentication");
                    this.PerformFirebaseAuthentication(appleIdCredential, rawNonce);
                }
            },
            error =>
            {
                // Something went wrong
                Debug.LogError("Error in PerformQuickLoginWithFirebase: "+error.ToString());
                Debug.LogError("Apple Quick Login encountered an error: " + error.LocalizedDescription + "\n" + error.LocalizedFailureReason + "\n" + error.LocalizedRecoverySuggestion);
            });
    }

    private void PerformFirebaseAuthentication(IAppleIDCredential appleIdCredential, string rawNonce)
    {
        Debug.Log("Inside PerformFirebaseAuthentication");

        var identityToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken);
        var authorizationCode = Encoding.UTF8.GetString(appleIdCredential.AuthorizationCode);
        var firebaseCredential = OAuthProvider.GetCredential("apple.com", identityToken, rawNonce, authorizationCode);

        Debug.Log("Firebase Credentials Are: "+firebaseCredential.ToString());

        try {
            auth.SignInWithCredentialAsync(firebaseCredential).ContinueWithOnMainThread(task => 
            {
                if (task.IsCanceled)
                {
                    Debug.Log("Firebase auth was canceled");
                }
                else if (task.IsFaulted)
                {
                    Debug.Log("Firebase auth failed");
                }
                else
                {
                    var firebaseUser = task.Result;
                    Debug.Log("Firebase auth completed | User ID:" + firebaseUser.UserId);
                    user = auth.CurrentUser;
                    userEmail = user.UserId;
                    SceneManager.LoadScene("CameraScreenBegin");

                    RegisterRevokedCallback();
                }
            });
        }
        catch(Exception e){
            Debug.LogError("Error in PerformFirebaseAuthentication: "+e);
        }
    }

    private void CheckCredentialStatus(string userId)
    {
        appleAuthManager.GetCredentialState(
            userId,
            state =>
            {
                switch (state)
                {
                    case CredentialState.Authorized:
                        // User ID is still valid. Login the user.
                        break;

                    case CredentialState.Revoked:
                        // User ID was revoked. Go to login screen.
                        break;

                    case CredentialState.NotFound:
                        // User ID was not found. Go to login screen.
                        break;
                }
            },
            error =>
            {
                Debug.LogError("Apple Check Credential Status encountered an error: " + error.LocalizedDescription + "\n" + error.LocalizedFailureReason + "\n" + error.LocalizedRecoverySuggestion);
            });
    }

#if UNITY_EDITOR
[PostProcessBuild(1)]
public static void OnPostProcessBuild(BuildTarget target, string path)
{
    if (target != BuildTarget.iOS) return;

    var projectPath = PBXProject.GetPBXProjectPath(path);

    // Adds entitlement depending on the Unity version used
#if UNITY_2019_3_OR_NEWER
        var project = new PBXProject();
        project.ReadFromString(System.IO.File.ReadAllText(projectPath));
        var manager = new ProjectCapabilityManager(projectPath, "Entitlements.entitlements", null, project.GetUnityMainTargetGuid());
        manager.AddSignInWithAppleWithCompatibility(project.GetUnityFrameworkTargetGuid());
        manager.WriteToFile();
#else
        var manager = new ProjectCapabilityManager(projectPath, "Entitlements.entitlements", PBXProject.GetUnityTargetName());
        manager.AddAppleSignIn();
        manager.WriteToFile();
#endif
}

    
#endif
}

public enum AppleSignInStatus
{
    NotLogged,
    QuickLoginCompleted,
    QuickLoginFailed,
    LoginCompleted,
    LoginFailed
}

#if UNITY_EDITOR

public static class CustomProjectCapabilityManagerExtension
{
    private const string EntitlementsArrayKey = "com.apple.developer.applesignin";
    private const string DefaultAccessLevel = "Default";
    private const string AuthenticationServicesFramework = "AuthenticationServices.framework";
    private const BindingFlags NonPublicInstanceBinding = BindingFlags.NonPublic | BindingFlags.Instance;

    /// <summary>
    /// Extension method for ProjectCapabilityManager to add the Sign In With Apple capability in compatibility mode.
    /// In particular, adds the AuthenticationServices.framework as an Optional framework, preventing crashes in
    /// iOS versions previous to 13.0
    /// </summary>
    /// <param name="manager">The manager for the main target to use when adding the Sign In With Apple capability.</param>
    /// <param name="unityFrameworkTargetGuid">The GUID for the UnityFramework target. If null, it will use the main target GUID.</param>
    public static void AddAppleSignIn(this ProjectCapabilityManager manager, string unityFrameworkTargetGuid = null)
    {
        var managerType = typeof(ProjectCapabilityManager);
        var capabilityTypeType = typeof(PBXCapabilityType);

        var projectField = managerType.GetField("project", NonPublicInstanceBinding);
        var targetGuidField = managerType.GetField("m_TargetGuid", NonPublicInstanceBinding);
        var entitlementFilePathField = managerType.GetField("m_EntitlementFilePath", NonPublicInstanceBinding);
        var getOrCreateEntitlementDocMethod = managerType.GetMethod("GetOrCreateEntitlementDoc", NonPublicInstanceBinding);
        var constructorInfo = capabilityTypeType.GetConstructor(
            NonPublicInstanceBinding,
            null,
            new[] { typeof(string), typeof(bool), typeof(string), typeof(bool) },
            null);

        if (projectField == null || targetGuidField == null || entitlementFilePathField == null ||
            getOrCreateEntitlementDocMethod == null || constructorInfo == null)
            throw new Exception("Can't Add Sign In With Apple programatically in this Unity version");

        var entitlementFilePath = entitlementFilePathField.GetValue(manager) as string;
        var entitlementDoc = getOrCreateEntitlementDocMethod.Invoke(manager, new object[] { }) as PlistDocument;
        if (entitlementDoc != null)
        {
            var plistArray = new PlistElementArray();
            plistArray.AddString(DefaultAccessLevel);
            entitlementDoc.root[EntitlementsArrayKey] = plistArray;
        }

        var project = projectField.GetValue(manager) as PBXProject;
        if (project != null)
        {
            var mainTargetGuid = targetGuidField.GetValue(manager) as string;
            var capabilityType = constructorInfo.Invoke(new object[] { "com.apple.developer.applesignin.custom", true, string.Empty, true }) as PBXCapabilityType;

            var targetGuidToAddFramework = unityFrameworkTargetGuid;
            if (targetGuidToAddFramework == null)
            {
                targetGuidToAddFramework = mainTargetGuid;
            }

            project.AddFrameworkToProject(targetGuidToAddFramework, AuthenticationServicesFramework, true);
            project.AddCapability(mainTargetGuid, capabilityType, entitlementFilePath, false);
        }
    }
}
#endif

