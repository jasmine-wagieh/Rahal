using Firebase.Auth;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeManager : MonoBehaviour
{
    public TMP_Text welcomeText;

    private FirebaseAuth auth;

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;

        if (auth.CurrentUser != null)
        {
            welcomeText.text =
                "Welcome to Rahal\n" + auth.CurrentUser.Email;
        }
        else
        {
            SceneManager.LoadScene("Login");
        }
    }

    public void OpenCairo()
    {
        PlayerPrefs.SetString("SelectedCity", "Cairo");
        SceneManager.LoadScene("City");
    }

    public void OpenLondon()
    {
        PlayerPrefs.SetString("SelectedCity", "London");
        SceneManager.LoadScene("City");
    }

    public void OpenParis()
    {
        PlayerPrefs.SetString("SelectedCity", "Paris");
        SceneManager.LoadScene("City");
    }

    public void OpenAddPlace()
    {
        SceneManager.LoadScene("AddPlace");
    }

    public void Logout()
    {
        auth.SignOut();
        SceneManager.LoadScene("Login");
    }

    public void OpenProfile()
    {
        SceneManager.LoadScene("Profile");
        }
}
