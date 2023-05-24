using System.Collections;
using UnityEngine;
using Unity.Services.Core;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
//using Firebase.Analytics;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Analytics;
using Unity.Services.Analytics;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

public class AuthManager : MonoBehaviour
{
    //Firebase variables
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;    
    public FirebaseUser User;

    //Login variables
    [Header("Login")]
    public InputField emailLoginField;
    public InputField passwordLoginField;
    public Text warningLoginText;
    public Text confirmLoginText;

    //Register variables
    [Header("Register")]
    public InputField usernameRegisterField;
    public InputField emailRegisterField;
    public InputField passwordRegisterField;
    public InputField passwordRegisterVerifyField;
    public Text warningRegisterText;

    //Register variables
    [Header("ForgetPassword")]
    public InputField forgetPassEmail;
    public Text notif_Title_Text;
    public Text notif_Message_Text;

    private static int loginSuccessCount = 0;
    private static int loginFailureCount = 0;

    private static int registrationSuccessCount = 0;
    private static int registrationFailureCount = 0;

    async void Awake()
    {
        await UnityServices.InitializeAsync();
        //Check that all of the necessary dependencies for Firebase are present on the system
        /**
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
        **/
    }

    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        //Set the authentication instance object
        auth = FirebaseAuth.DefaultInstance;
        //FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

        Debug.Log("Done Setting up Firebase Auth");
    }

    //Function for the login button
    public void LoginButton()
    {
        Debug.Log("Login Button Clicked");
        //Call the login coroutine passing the email and password
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }

    public void SwitchSceneToAR()
    {
        Debug.Log("Switch To AR Camera Scene Button Clicked");
        //Call the login coroutine passing the email and password
        SceneManager.LoadScene("AR_TB_7821");
    }

    public void SwitchSceneToHelpPage()
    {
        Debug.Log("Help Page Button Clicked");
        //Call the login coroutine passing the email and password
        SceneManager.LoadScene("CameraSceneBegin");
    }

    public void SwitchSceneToLogin()
    {
        Debug.Log("Switch To Login Scene Button Clicked");
        //Call the login coroutine passing the email and password
        SceneManager.LoadScene("LogInScene");
    }

    //Function for the continue button
    public void ContinueButton()
    {
        Debug.Log("Register Button Clicked");
        //Call the register coroutine passing the email, password, and username
        SceneManager.LoadScene("SignInScenePt2");
    }

    //Function for the register button
    public void RegisterButton()
    {
        Debug.Log("Register Button Clicked");
        //Call the register coroutine passing the email, password, and username
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    }

    private IEnumerator Login(string _email, string _password)
    {
        Debug.Log("email is "+_email+" password is "+_password);
        _email = _email.Trim();
        _password = _password.Trim();
        //Call the Firebase auth signin function passing the email and password
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist";
                    break;
            }
            warningLoginText.text = message;
            confirmLoginText.text = "";

            loginFailureCount++;
            Dictionary<string, object> parameters = new Dictionary<string, object>(){{"loginFailureCount", loginFailureCount}};
 
            AnalyticsService.Instance.CustomData("loginFailureCount", parameters); 

            AnalyticsService.Instance.Flush();

            //Firebase.Analytics.FirebaseAnalytics.LogEvent("loginFailureCount", new Parameter("count", loginFailureCount));
        }
        else
        {
            //User is now logged in
            //Now get the result
            User = LoginTask.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            warningLoginText.text = "";
            confirmLoginText.text = "Logged In";

            loginSuccessCount++;
            Dictionary<string, object> parameters = new Dictionary<string, object>(){{"loginSuccessCount", loginSuccessCount}};
 
            AnalyticsService.Instance.CustomData("loginSuccessCount", parameters); 

            AnalyticsService.Instance.Flush();

            //Firebase.Analytics.FirebaseAnalytics.LogEvent("loginSuccessCount", new Parameter("count", loginSuccessCount));

            SceneManager.LoadScene("AR_TB_7821");
        }
    }

    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
        {
            //If the username field is blank show a warning
            warningRegisterText.text = "Missing Username";
            confirmLoginText.text = "";
        }
        else if(passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            //If the password does not match show a warning
            warningRegisterText.text = "Password Does Not Match!";
            confirmLoginText.text = "";
        }
        else 
        {
            //Call the Firebase auth signin function passing the email and password
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
                        message = "Email Already In Use";
                        break;
                }
                warningRegisterText.text = message;
                confirmLoginText.text = "";
            }
            else
            {
                //User has now been created
                //Now get the result
                User = RegisterTask.Result;

                if (User != null)
                {
                    //Create a user profile and set the username
                    UserProfile profile = new UserProfile{DisplayName = _username};

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
                        confirmLoginText.text = "";

                        registrationFailureCount++;
                        Dictionary<string, object> parameters = new Dictionary<string, object>(){{"RegistrationFailureCount", registrationFailureCount}};
 
                        Events.CustomData("RegistrationFailureCount", parameters); 

                        Events.Flush();

                        //Firebase.Analytics.FirebaseAnalytics.LogEvent("registrationFailureCount", new Parameter("count", registrationFailureCount));
                    }
                    else
                    {
                        //Username is now set
                        //Now return to login screen
                        UIManager.instance.LoginScreen();
                        warningRegisterText.text = "";
                        confirmLoginText.text = "Registered!";

                        registrationSuccessCount++;
                        Dictionary<string, object> parameters = new Dictionary<string, object>(){{"RegistrationSuccessCount", registrationSuccessCount}};
 
                        AnalyticsService.Instance.CustomData("RegistrationSuccessCount", parameters); 

                        AnalyticsService.Instance.Flush();

                        //Firebase.Analytics.FirebaseAnalytics.LogEvent("registrationSuccessCount", new Parameter("count", registrationSuccessCount));
                    }
                }
            }
        }
    }

    public void forgetPass(){
        if(string.IsNullOrEmpty(forgetPassEmail.text)){
            showNotificationMessage("Error", "Fields Empty! Please Input Details In All Fields");
            return;
        }

        forgotPasswordSubmit(forgetPassEmail.text);
    }

    void forgotPasswordSubmit(string forgotPasswordEmail){
            auth.SendPasswordResetEmailAsync(forgotPasswordEmail).ContinueWithOnMainThread(task=>{
                if(task.IsCanceled){
                    Debug.LogError("SendPasswordRestEmailAsync was cancelled");
                }
                
                if(task.IsFaulted){
                    foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
                    {
                        Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                        if(firebaseEx != null){
                            var errorCode = (AuthError) firebaseEx.ErrorCode;
                            showNotificationMessage("Error", GetErrorMessage(errorCode));
                        }
                    }
                }

                showNotificationMessage("Alert", "Successfully Send Email For Reset Password");


            });
    }

    private void showNotificationMessage(string title, string message){
        notif_Title_Text.text = "" + title;
        notif_Message_Text.text = "" + message;
    }

    private static string GetErrorMessage(AuthError errorCode){
        var message = "";
        switch(errorCode){
            case AuthError.AccountExistsWithDifferentCredentials:
                message = "Account Does Not Exist";
                break;
            case AuthError.MissingPassword:
                message = "Account Does Not Exist";
                break;
            case AuthError.WeakPassword:
                message = "Password So Weak";
                break;
            case AuthError.WrongPassword:
                message = "Wrong Password";
                break;
            case AuthError.EmailAlreadyInUse:
                message = "Your Email Already in Use";
                break;
            case AuthError.InvalidEmail:
                message = "Your Email is Invalid";
                break;
            case AuthError.MissingEmail:
                message = "Your Email is Invalid";
                break;
            default:
                message = "Invalid Error";
                break;
        }
        return message;
    }
}