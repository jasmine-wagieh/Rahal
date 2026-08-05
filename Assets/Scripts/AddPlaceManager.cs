using System;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AddPlaceManager : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField nameInput;
    public TMP_InputField cityInput;
    public TMP_InputField categoryInput;
    public TMP_InputField descriptionInput;
    public TMP_InputField imageUrlInput;

    [Header("UI")]
    public TMP_Text statusText;

    private FirebaseFirestore database;
    private FirebaseAuth auth;
    private bool isUploading;

    private void Start()
    {
        database = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;

        SetStatus("");
    }

    public async void UploadPlace()
    {
        if (isUploading)
        {
            return;
        }

        string placeName = nameInput.text.Trim();
        string city = NormaliseCity(cityInput.text);
        string category = NormaliseCategory(categoryInput.text);
        string description = descriptionInput.text.Trim();
        string imageUrl = imageUrlInput.text.Trim();

        if (string.IsNullOrEmpty(placeName))
        {
            SetStatus("Enter the place name.");
            return;
        }

        if (string.IsNullOrEmpty(city))
        {
            SetStatus("City must be Cairo, London or Paris.");
            return;
        }

        if (string.IsNullOrEmpty(category))
        {
            SetStatus(
                "Category must be Cafe, Museum, Theatre or Restaurant."
            );
            return;
        }

        if (string.IsNullOrEmpty(description))
        {
            SetStatus("Enter a description.");
            return;
        }

        if (string.IsNullOrEmpty(imageUrl))
        {
            SetStatus("Enter a direct image URL.");
            return;
        }

        if (
            !imageUrl.StartsWith("http://") &&
            !imageUrl.StartsWith("https://")
        )
        {
            SetStatus("Enter a valid image URL.");
            return;
        }

        isUploading = true;
        SetStatus("Uploading place...");

        try
        {
            string userId = "unknown";

            if (auth.CurrentUser != null)
            {
                userId = auth.CurrentUser.UserId;
            }

            Dictionary<string, object> placeData =
                new Dictionary<string, object>
                {
                    { "name", placeName },
                    { "city", city },
                    { "category", category },
                    { "description", description },
                    { "imageUrl", imageUrl },
                    { "uploadedBy", userId },
                    { "createdAt", Timestamp.GetCurrentTimestamp() }
                };

            await database
                .Collection("places")
                .AddAsync(placeData);

            SetStatus("Place uploaded successfully!");

            ClearInputs();

            PlayerPrefs.SetString("SelectedCity", city);

            await System.Threading.Tasks.Task.Delay(1000);

            SceneManager.LoadScene("City");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            SetStatus("Upload failed. Check the Unity Console.");
        }
        finally
        {
            isUploading = false;
        }
    }

    private string NormaliseCity(string value)
    {
        string city = value.Trim().ToLower();

        switch (city)
        {
            case "cairo":
                return "Cairo";

            case "london":
                return "London";

            case "paris":
                return "Paris";

            default:
                return "";
        }
    }

    private string NormaliseCategory(string value)
    {
        string category = value.Trim().ToLower();

        switch (category)
        {
            case "cafe":
            case "cafes":
            case "café":
            case "cafés":
                return "Cafe";

            case "museum":
            case "museums":
                return "Museum";

            case "theatre":
            case "theater":
            case "theatres":
            case "theaters":
                return "Theatre";

            case "restaurant":
            case "restaurants":
                return "Restaurant";

            default:
                return "";
        }
    }

    private void ClearInputs()
    {
        nameInput.text = "";
        cityInput.text = "";
        categoryInput.text = "";
        descriptionInput.text = "";
        imageUrlInput.text = "";
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    public void GoBack()
    {
        SceneManager.LoadScene("Home");
    }
}