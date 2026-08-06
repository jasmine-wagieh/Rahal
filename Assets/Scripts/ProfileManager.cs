using System;
using System.Text;
using Firebase.Auth;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProfileManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text titleText;
    public TMP_Text likedPlacesText;
    public TMP_Text uploadedPlacesText;

    private FirebaseAuth auth;
    private FirebaseFirestore database;

    private async void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        database = FirebaseFirestore.DefaultInstance;

        if (auth.CurrentUser == null)
        {
            SceneManager.LoadScene("Login");
            return;
        }

        if (titleText != null)
        {
            titleText.text = "My Profile\n" + auth.CurrentUser.Email;
        }

        await LoadLikedPlaces();
        await LoadUploadedPlaces();
    }

    private async System.Threading.Tasks.Task LoadLikedPlaces()
    {
        if (likedPlacesText != null)
        {
            likedPlacesText.text = "Loading liked places...";
        }

        try
        {
            QuerySnapshot snapshot = await database
                .Collection("users")
                .Document(auth.CurrentUser.UserId)
                .Collection("likedPlaces")
                .GetSnapshotAsync();

            if (snapshot.Count == 0)
            {
                likedPlacesText.text = "Liked Places\n\nNo liked places yet.";
                return;
            }

            StringBuilder result = new StringBuilder();
            result.AppendLine("Liked Places");
            result.AppendLine();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                result.AppendLine("♥ " + GetString(document, "name"));
                result.AppendLine(GetString(document, "city"));
                result.AppendLine();
            }

            likedPlacesText.text = result.ToString();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            likedPlacesText.text = "Could not load liked places.";
        }
    }

    private async System.Threading.Tasks.Task LoadUploadedPlaces()
    {
        if (uploadedPlacesText != null)
        {
            uploadedPlacesText.text = "Loading uploaded places...";
        }

        try
        {
            QuerySnapshot snapshot = await database
                .Collection("places")
                .WhereEqualTo(
                    "uploadedBy",
                    auth.CurrentUser.UserId
                )
                .GetSnapshotAsync();

            if (snapshot.Count == 0)
            {
                uploadedPlacesText.text =
                    "Places I Uploaded\n\nNo uploaded places yet.";
                return;
            }

            StringBuilder result = new StringBuilder();
            result.AppendLine("Places I Uploaded");
            result.AppendLine();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                result.AppendLine(GetString(document, "name"));
                result.AppendLine(
                    GetString(document, "category") +
                    " — " +
                    GetString(document, "city")
                );
                result.AppendLine();
            }

            uploadedPlacesText.text = result.ToString();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            uploadedPlacesText.text =
                "Could not load uploaded places.";
        }
    }

    private string GetString(
        DocumentSnapshot document,
        string fieldName
    )
    {
        if (document.ContainsField(fieldName))
        {
            return document.GetValue<string>(fieldName);
        }

        return "";
    }

    public void GoBack()
    {
        SceneManager.LoadScene("Home");
    }
}