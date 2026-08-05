using System;
using System.Collections;
using System.Text;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CityManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text cityTitle;
    public TMP_Text placesText;
    public RawImage placeImage;

    private FirebaseFirestore database;
    private string selectedCity;
    private string selectedCategory = "All";

    private void Start()
    {
        selectedCity = PlayerPrefs.GetString("SelectedCity", "Cairo");

        if (cityTitle != null)
        {
            cityTitle.text = selectedCity;
        }

        if (placeImage != null)
        {
            placeImage.gameObject.SetActive(false);
        }

        database = FirebaseFirestore.DefaultInstance;
        LoadPlaces();
    }

    public void ShowAll()
    {
        selectedCategory = "All";
        LoadPlaces();
    }

    public void ShowCafes()
    {
        selectedCategory = "Cafe";
        LoadPlaces();
    }

    public void ShowMuseums()
    {
        selectedCategory = "Museum";
        LoadPlaces();
    }

    public void ShowTheatres()
    {
        selectedCategory = "Theatre";
        LoadPlaces();
    }

    public void ShowRestaurants()
    {
        selectedCategory = "Restaurant";
        LoadPlaces();
    }

    private async void LoadPlaces()
    {
        if (placesText != null)
        {
            placesText.text = "Loading places...";
        }

        if (placeImage != null)
        {
            placeImage.gameObject.SetActive(false);
        }

        try
        {
            Query query = database
                .Collection("places")
                .WhereEqualTo("city", selectedCity);

            if (selectedCategory != "All")
            {
                query = query.WhereEqualTo(
                    "category",
                    selectedCategory
                );
            }

            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            if (snapshot.Count == 0)
            {
                if (placesText != null)
                {
                    placesText.text =
                        "No " +
                        selectedCategory.ToLower() +
                        " places found in " +
                        selectedCity +
                        ".";
                }

                return;
            }

            StringBuilder result = new StringBuilder();
            bool firstImageLoaded = false;

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                string name = GetString(document, "name");
                string category = GetString(document, "category");
                string description = GetString(
                    document,
                    "description"
                );
                string imageUrl = GetString(
                    document,
                    "imageUrl"
                );

                result.AppendLine(name);
                result.AppendLine(category);
                result.AppendLine(description);
                result.AppendLine();
                result.AppendLine("--------------------");
                result.AppendLine();

                if (
                    !firstImageLoaded &&
                    !string.IsNullOrEmpty(imageUrl)
                )
                {
                    firstImageLoaded = true;
                    StartCoroutine(LoadImage(imageUrl));
                }
            }

            if (placesText != null)
            {
                placesText.text = result.ToString();
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);

            if (placesText != null)
            {
                placesText.text =
                    "Could not load places. Check the Unity Console.";
            }
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

    private IEnumerator LoadImage(string imageUrl)
    {
        if (placeImage == null)
        {
            Debug.LogError(
                "Place Image is not connected in City Manager."
            );
            yield break;
        }

        using UnityWebRequest request =
            UnityWebRequestTexture.GetTexture(imageUrl);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(
                "Image failed to load: " +
                request.error +
                "\nURL: " +
                imageUrl
            );

            placeImage.gameObject.SetActive(false);
            yield break;
        }

        Texture2D texture =
            DownloadHandlerTexture.GetContent(request);

        placeImage.texture = texture;
        placeImage.gameObject.SetActive(true);
    }

    public void GoBack()
    {
        SceneManager.LoadScene("Home");
    }
}