using Firebase;
using Firebase.Auth;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    [Header("Messages")]
    public TMP_Text statusMessage;

    private FirebaseAuth auth;
    private bool firebaseReady;

    private async void Start()
    {
        SetStatus("Connecting to Firebase...");

        DependencyStatus dependencyStatus =
            await FirebaseApp.CheckAndFixDependenciesAsync();

        if (dependencyStatus == DependencyStatus.Available)
        {
            auth = FirebaseAuth.DefaultInstance;
            firebaseReady = true;
            SetStatus("Ready");
        }
        else
        {
            firebaseReady = false;
            SetStatus("Firebase could not start.");
            Debug.LogError(
                "Could not resolve Firebase dependencies: " +
                dependencyStatus
            );
        }
    }

    public async void Login()
    {
        if (!firebaseReady)
        {
            SetStatus("Firebase is not ready yet.");
            return;
        }

        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(password))
        {
            SetStatus("Please enter your email and password.");
            return;
        }

        SetStatus("Logging in...");

        try
        {
            AuthResult result =
                await auth.SignInWithEmailAndPasswordAsync(
                    email,
                    password
                );

            Debug.Log(
                "Logged in user: " +
                result.User.UserId
            );

            SetStatus("Login successful!");

            SceneManager.LoadScene("Home");
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception);
            SetStatus(GetFriendlyError(exception));
        }
    }

    public async void Register()
    {
        if (!firebaseReady)
        {
            SetStatus("Firebase is not ready yet.");
            return;
        }

        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(password))
        {
            SetStatus("Please enter your email and password.");
            return;
        }

        if (password.Length < 6)
        {
            SetStatus("Password must contain at least 6 characters.");
            return;
        }

        SetStatus("Creating account...");

        try
        {
            AuthResult result =
                await auth.CreateUserWithEmailAndPasswordAsync(
                    email,
                    password
                );

            Debug.Log(
                "Created user: " +
                result.User.UserId
            );

            SetStatus("Account created!");

            SceneManager.LoadScene("Home");
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception);
            SetStatus(GetFriendlyError(exception));
        }
    }

    private void SetStatus(string message)
    {
        if (statusMessage != null)
        {
            statusMessage.text = message;
        }
    }

    private string GetFriendlyError(System.Exception exception)
    {
        string error = exception.ToString().ToLower();

        if (error.Contains("email_already_in_use"))
            return "An account already uses this email.";

        if (error.Contains("invalid_email"))
            return "Please enter a valid email address.";

        if (error.Contains("wrong_password") ||
            error.Contains("invalid_login_credentials") ||
            error.Contains("invalid_credential"))
            return "Incorrect email or password.";

        if (error.Contains("user_not_found"))
            return "No account was found with this email.";

        if (error.Contains("weak_password"))
            return "Password must contain at least 6 characters.";

        if (error.Contains("network"))
            return "Check your internet connection.";

        return "Something went wrong. Check the Unity Console.";
    }
}