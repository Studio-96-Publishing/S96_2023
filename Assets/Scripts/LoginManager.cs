using System.Collections;
using UnityEngine;
using Unity.Services.Core;
using Firebase;
using Firebase.Auth;
//using Firebase.Analytics;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Analytics;
using Unity.Services.Analytics;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Firebase.Extensions;
using System.Threading.Tasks;
using System;

public class LoginManager : MonoBehaviour
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

    private static int loginSuccessCount = 0;
    private static int loginFailureCount = 0;

    public static string userEmail;

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
            userEmail = User.Email;
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            warningLoginText.text = "";
            confirmLoginText.text = "Logged In";

            loginSuccessCount++;
            Dictionary<string, object> parameters = new Dictionary<string, object>(){{"loginSuccessCount", loginSuccessCount}};
 
            AnalyticsService.Instance.CustomData("loginSuccessCount", parameters); 

            AnalyticsService.Instance.Flush();

            //Firebase.Analytics.FirebaseAnalytics.LogEvent("loginSuccessCount", new Parameter("count", loginSuccessCount));

            SceneManager.LoadScene("CameraScreenBegin");
        }
    }

    public void forgetPass(){
        if(string.IsNullOrEmpty(emailLoginField.text)){
            showNotificationMessage("Error", "Fields Empty! Please Input Details In All Fields");
            return;
        }

        print("Email to send code to is: "+emailLoginField.text);
        forgotPasswordSubmit(emailLoginField.text);
    }

    void forgotPasswordSubmit(string forgotPasswordEmail){
            auth.SendPasswordResetEmailAsync(forgotPasswordEmail).ContinueWithOnMainThread(task=>{
                if(task.IsCanceled){
                    print("Task is Cancelled");
                    Debug.LogError("SendPasswordRestEmailAsync was cancelled");
                    return;
                }
                
                if(task.IsFaulted){
                    print("Task is Faulted");
                    foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
                    {
                        print("Iterating through exceptions");
                        Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                        if(firebaseEx != null){
                            var errorCode = (AuthError) firebaseEx.ErrorCode;
                            print("firebase Exception is "+firebaseEx+" "+errorCode);
                            showNotificationMessage("Error", GetErrorMessage(errorCode));
                        }
                    }
                    return;
                }

                showNotificationMessage("Alert", "Successfully Send Email For Reset Password");
            });

        SceneManager.LoadScene("ForgotPasswordEmailConfirmation");
    }

    private void showNotificationMessage(string title, string message){
        warningLoginText.text = "" + title + ": " + message;
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

    public void switchSceneToForgotPassword(){
        SceneManager.LoadScene("ForgotPassword");
    }

    public void switchSceneToLogInPage(){
        if(auth != null && User != null) auth.SignOut();
        SceneManager.LoadScene("LogInScene");
    }
}
