using System.Collections;
using UnityEngine;
using Unity.Services.Core;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
//using Firebase.Analytics;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Analytics;
using Unity.Services.Analytics;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Amazon.Runtime.Internal;
using Amazon.Util;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.CognitoIdentity;
using Amazon.Runtime;
using Amazon;

public class RegisterManager : MonoBehaviour
{
    private IAmazonDynamoDB client;
    private DynamoDBContext context;
    private AWSCredentials credentials;

    private string aws_cognito_id = "us-east-1:ab5f71b6-28e0-4b43-8446-a2515d583680";

    private static Table userInfoTable;
    private static string tableNameDB = "userInfoDB";

    //Firebase variables
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;    
    public FirebaseUser User;
    public FirebaseFirestore database;

    //Register variables
    [Header("Register")]
    public InputField emailRegisterField;
    public InputField passwordRegisterField;
    public InputField passwordRegisterVerifyField;
    public InputField firstNameRegisterField;
    public InputField lastNameRegisterField;
    public Dropdown genderRegisterField;
    public InputField ageRegisterField;
    public InputField zipCodeRegisterField;
    public GameObject registerUIPtOne;
    public GameObject registerUIPtTwo;
    public Text warningRegisterText;
    public Text confirmRegistrationText;

    private static int registrationSuccessCount = 0;
    private static int registrationFailureCount = 0;

    public static string userEmail;
    private string password;

    public GameObject logInRedirectTwo;

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

    async void Awake()
    {
        logInRedirectTwo.SetActive(false);
        await UnityServices.InitializeAsync();
        
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                //If they are avalible Initialize Firebase
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });

        //Check that all of the necessary dependencies for Firebase are present on the system
    }

    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        //Set the authentication instance object
        auth = FirebaseAuth.DefaultInstance;

        database = FirebaseFirestore.DefaultInstance;
        //FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

        Debug.Log("Done Setting up Firebase Auth");
    }

    public void switchSceneCamera(){
         SceneManager.LoadScene("CameraScreenBegin");
    }

    public void FinishButton()
    {
        Debug.Log("Finish Button Clicked");
        
        addOtherUserInfoAWS();
        //Call the register coroutine passing the email, password, and username
        SceneManager.LoadScene("CameraScreenBegin");
    }

    public void addOtherUserInfo()
    {
        Debug.Log("Adding User Info");
        int genderVal = genderRegisterField.value;
        string genderText = genderRegisterField.options[genderVal].text;
        Dictionary<string, object> userInfo = new Dictionary<string, object>
        {
            {"useremail",userEmail},
            {"password",password},
            {"firstname", firstNameRegisterField.text},
            {"lastname", lastNameRegisterField.text},
            {"gender", genderText},
            {"age", ageRegisterField.text},
            {"zipcode", zipCodeRegisterField.text}
        };
        DocumentReference userDoc = database.Collection("userData").Document(userEmail);
        userDoc.SetAsync(userInfo).ContinueWithOnMainThread(task =>
        {
            Debug.Log("Updated Profile");
        });
    }

    public void addOtherUserInfoAWS()
    {
        int genderVal = genderRegisterField.value;
        string genderText = genderRegisterField.options[genderVal].text;
        UserInfo userInfo = new UserInfo
        {
            userEmail = userEmail,
            password = password,
            firstname = firstNameRegisterField.text,
            lastname = lastNameRegisterField.text,
            gender = genderText,
            age = ageRegisterField.text,
            zipcode = zipCodeRegisterField.text
        };
            
            // Save the book.
        context.SaveAsync(userInfo,(result)=>{
            if(result.Exception == null) print("user info updated");
        });
    }

    //Function for the register button
    public void RegisterButton()
    {
        Debug.Log("Register Button Clicked");
        //Call the register coroutine passing the email, password, and username
        print("Register Info is: "+emailRegisterField.text + " "+passwordRegisterField.text);
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text));
    }

    private IEnumerator Register(string _email, string _password)
    {
        if (_email == "")
        {
            //If the username field is blank show a warning
            warningRegisterText.text = "Missing Username";
            confirmRegistrationText.text = "";
        }
        else if(passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            //If the password does not match show a warning
            warningRegisterText.text = "Password Does Not Match!";
            confirmRegistrationText.text = "";
        }
        else 
        {
            userEmail = _email;
            password = _password;
            //Call the Firebase auth signin function passing the email and password
            print("is auth null: "+auth == null);
            print("is email null: "+_email == null);
            print("is password null: "+_password == null);

            var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            //Wait until the task completes
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                //If there are errors handle them
                Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Register Failed!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WeakPassword:
                        message = "Weak Password";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "";
                        logInRedirectTwo.SetActive(true);
                        //message = "Email is already in use. Click here to log in.";
                        break;
                }
                warningRegisterText.text = message;
                confirmRegistrationText.text = "";
            }
            else
            {
                //User has now been created
                //Now get the result
                User = RegisterTask.Result;

                if (User != null)
                {
                    //Create a user profile and set the username
                    UserProfile profile = new UserProfile{DisplayName = _email};

                    //Call the Firebase auth update user profile function passing the profile with the username
                    var ProfileTask = User.UpdateUserProfileAsync(profile);
                    //Wait until the task completes
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        //If there are errors handle them
                        Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        FirebaseException firebaseEx = ProfileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                        warningRegisterText.text = "Username Set Failed!";
                        confirmRegistrationText.text = "";

                        print("RegistrationFailed");

                        registrationFailureCount++;
                        Dictionary<string, object> parameters = new Dictionary<string, object>(){{"RegistrationFailureCount", registrationFailureCount}};
 
                        AnalyticsService.Instance.CustomData("RegistrationFailureCount", parameters); 

                        AnalyticsService.Instance.Flush();

                        //Firebase.Analytics.FirebaseAnalytics.LogEvent("registrationFailureCount", new Parameter("count", registrationFailureCount));
                    }
                    else
                    {
                        //Username is now set
                        //Now return to login screen
                        //UIManager.instance.LoginScreen();
                        warningRegisterText.text = "";
                        confirmRegistrationText.text = "Registered!";

                        print("RegistrationSuccess");

                        registrationSuccessCount++;
                        Dictionary<string, object> parameters = new Dictionary<string, object>(){{"RegistrationSuccessCount", registrationSuccessCount}};
 
                        AnalyticsService.Instance.CustomData("RegistrationSuccessCount", parameters); 

                        AnalyticsService.Instance.Flush();

                        registerUIPtOne.SetActive(false);
                        registerUIPtTwo.SetActive(true);
                        
                        //SceneManager.LoadScene("CameraScreenBegin");

                        //Firebase.Analytics.FirebaseAnalytics.LogEvent("registrationSuccessCount", new Parameter("count", registrationSuccessCount));
                    }
                }
            }
        }
    }
}

[DynamoDBTable("userInfoDB")]
public class UserInfo
{
    [DynamoDBHashKey]   // Hash key.
    public string userEmail { get; set; }
    [DynamoDBProperty]
    public string password { get; set; }
    [DynamoDBProperty]
    public string firstname { get; set; }
    [DynamoDBProperty]     
    public string lastname { get; set; }
    [DynamoDBProperty]    
    public string gender { get; set; }
    [DynamoDBProperty]     
    public string age { get; set; }
    [DynamoDBProperty]    
    public string zipcode { get; set; }
}
