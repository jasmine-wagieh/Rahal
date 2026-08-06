using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AddPlaceManager : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField nameInput;
    public TMP_InputField cityInput;
    public TMP_InputField categoryInput;
    public TMP_InputField descriptionInput;

    [Header("Photo UI")]
    public RawImage photoPreview;

    [Header("Other UI")]
    public TMP_Text statusText;

    private const string CloudName = "p6hzbgbe";
    private const string UploadPreset = "i9ftxhc1";

    private FirebaseFirestore database;
    private FirebaseAuth auth;

    private string selectedImagePath;
    private bool isUploading;

    private void Start()
    {
        database = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;

        if (photoPreview != null)
        {
            photoPreview.gameObject.SetActive(false);
        }

        SetStatus("");
    }

    public void ChoosePhoto()
    {
        NativeGallery.GetImageFromGallery(
            imagePath =>
            {
                if (string.IsNullOrEmpty(imagePath))
                {
                    SetStatus("No photo selected.");
                    return;
                }

                selectedImagePath = imagePath;

                Texture2D selectedTexture =
                    NativeGallery.LoadImageAtPath(
                        imagePath,
                        1024,
                        false,
                        false
                    );

                if (selectedTexture == null)
                {
                    selectedImagePath = "";
                    SetStatus("Could not load the selected photo.");
                    return;
                }

                if (photoPreview != null)
                {
                    photoPreview.texture = selectedTexture;
                    photoPreview.gameObject.SetActive(true);
                }

                SetStatus("Photo selected.");
            },
            "Choose a place photo",
            "image/*"
        );
    }

    public void UploadPlace()
    {
        if (isUploading)
        {
            return;
        }

        string placeName =
            nameInput != null ? nameInput.text.Trim() : "";

        string city =
            cityInput != null
                ? NormaliseCity(cityInput.text)
                : "";

        string category =
            categoryInput != null
                ? NormaliseCategory(categoryInput.text)
                : "";

        string description =
            descriptionInput != null
                ? descriptionInput.text.Trim()
                : "";

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

        if (string.IsNullOrEmpty(selectedImagePath))
        {
            SetStatus("Choose a photo first.");
            return;
        }

        StartCoroutine(
            UploadPhotoAndSavePlace(
                placeName,
                city,
                category,
                description
            )
        );
    }

    private IEnumerator UploadPhotoAndSavePlace(
        string placeName,
        string city,
        string category,
        string description
    )
    {
        isUploading = true;
        SetStatus("Uploading photo...");

        byte[] imageBytes;

        try
        {
            imageBytes =
                System.IO.File.ReadAllBytes(selectedImagePath);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            SetStatus("Could not read the selected photo.");
            isUploading = false;
            yield break;
        }

        string uploadUrl =
            "https://api.cloudinary.com/v1_1/" +
            CloudName +
            "/image/upload";

        WWWForm form = new WWWForm();

        form.AddField("upload_preset", UploadPreset);

        form.AddBinaryData(
            "file",
            imageBytes,
            "rahal_place.jpg",
            "image/jpeg"
        );

        using UnityWebRequest request =
            UnityWebRequest.Post(uploadUrl, form);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(
                "Cloudinary upload failed: " +
                request.error +
                "\n" +
                request.downloadHandler.text
            );

            SetStatus("Photo upload failed.");
            isUploading = false;
            yield break;
        }

        CloudinaryResponse response;

        try
        {
            response = JsonUtility.FromJson<CloudinaryResponse>(
                request.downloadHandler.text
            );
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            SetStatus("Could not read the upload response.");
            isUploading = false;
            yield break;
        }

        if (
            response == null ||
            string.IsNullOrEmpty(response.secure_url)
        )
        {
            Debug.LogError(
                "Cloudinary response did not contain secure_url:\n" +
                request.downloadHandler.text
            );

            SetStatus("Photo upload returned no URL.");
            isUploading = false;
            yield break;
        }

        SetStatus("Saving place...");

        SavePlaceToFirestore(
            placeName,
            city,
            category,
            description,
            response.secure_url
        );
    }

    private async void SavePlaceToFirestore(
        string placeName,
        string city,
        string category,
        string description,
        string imageUrl
    )
    {
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
                    {
                        "createdAt",
                        Timestamp.GetCurrentTimestamp()
                    }
                };

            await database
                .Collection("places")
                .AddAsync(placeData);

            SetStatus("Place uploaded successfully!");

            PlayerPrefs.SetString("SelectedCity", city);

            ClearInputs();

            await System.Threading.Tasks.Task.Delay(1000);

            SceneManager.LoadScene("City");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            SetStatus("Place upload failed.");
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
        if (nameInput != null)
            nameInput.text = "";

        if (cityInput != null)
            cityInput.text = "";

        if (categoryInput != null)
            categoryInput.text = "";

        if (descriptionInput != null)
            descriptionInput.text = "";

        selectedImagePath = "";

        if (photoPreview != null)
        {
            photoPreview.texture = null;
            photoPreview.gameObject.SetActive(false);
        }
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

    [Serializable]
    private class CloudinaryResponse
    {
        public string secure_url;
    }
}