using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class CityManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text cityTitle;
    public Transform placesContent;
    public GameObject placeCardPrefab;
    public TMP_Text emptyMessage;

    private FirebaseFirestore database;

    private string selectedCity;
    private string selectedCategory = "All";

    private Coroutine loadingCoroutine;

    private void Start()
    {
        database = FirebaseFirestore.DefaultInstance;

        selectedCity = PlayerPrefs.GetString(
            "SelectedCity",
            "Cairo"
        );

        if (cityTitle != null)
        {
            cityTitle.text = selectedCity;
        }

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

    private void LoadPlaces()
    {
        if (placesContent == null)
        {
            Debug.LogError(
                "Places Content is not assigned in City Manager."
            );
            return;
        }

        if (placeCardPrefab == null)
        {
            Debug.LogError(
                "Place Card Prefab is not assigned in City Manager."
            );
            return;
        }

        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
        }

        ClearCards();

        if (emptyMessage != null)
        {
            emptyMessage.gameObject.SetActive(true);
            emptyMessage.text = "Loading places...";
        }

        loadingCoroutine = StartCoroutine(
            LoadPlacesFromApi()
        );
    }

    private IEnumerator LoadPlacesFromApi()
    {
        HashSet<string> loadedPlaceKeys =
            new HashSet<string>();

        int totalLoaded = 0;

        string apiUrl =
            ApiTestManager.BaseUrl + "/places";

        using UnityWebRequest request =
            UnityWebRequest.Get(apiUrl);

        yield return request.SendWebRequest();

        if (
            request.result ==
            UnityWebRequest.Result.Success
        )
        {
            string wrappedJson =
                "{\"items\":" +
                request.downloadHandler.text +
                "}";

            ApiPlaceList response =
                JsonUtility.FromJson<ApiPlaceList>(
                    wrappedJson
                );

            if (
                response != null &&
                response.items != null
            )
            {
                foreach (ApiPlace place in response.items)
                {
                    if (!MatchesCurrentFilter(place))
                    {
                        continue;
                    }

                    string placeKey = CreatePlaceKey(
                        place.name,
                        place.city,
                        place.category
                    );

                    if (loadedPlaceKeys.Contains(placeKey))
                    {
                        continue;
                    }

                    CreatePlaceCard(
                        "api-" + place.id,
                        place.name,
                        place.category,
                        place.description,
                        place.imageUrl,
                        place.city
                    );

                    loadedPlaceKeys.Add(placeKey);
                    totalLoaded++;
                }
            }

            Debug.Log(
                "REST API places loaded successfully."
            );
        }
        else
        {
            Debug.LogWarning(
                "REST API request failed. " +
                "Firestore will still be used. Error: " +
                request.error
            );
        }

        LoadPlacesFromFirestore(
            loadedPlaceKeys,
            totalLoaded
        );
    }

    private async void LoadPlacesFromFirestore(
        HashSet<string> loadedPlaceKeys,
        int totalLoaded
    )
    {
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

            QuerySnapshot snapshot =
                await query.GetSnapshotAsync();

            foreach (
                DocumentSnapshot document
                in snapshot.Documents
            )
            {
                string name =
                    GetString(document, "name");

                string city =
                    GetString(document, "city");

                string category =
                    GetString(document, "category");

                string placeKey = CreatePlaceKey(
                    name,
                    city,
                    category
                );

                if (loadedPlaceKeys.Contains(placeKey))
                {
                    continue;
                }

                CreatePlaceCard(
                    document.Id,
                    name,
                    category,
                    GetString(document, "description"),
                    GetString(document, "imageUrl"),
                    city
                );

                loadedPlaceKeys.Add(placeKey);
                totalLoaded++;
            }

            if (totalLoaded == 0)
            {
                if (emptyMessage != null)
                {
                    emptyMessage.gameObject.SetActive(true);
                    emptyMessage.text =
                        "No places found in " +
                        selectedCity +
                        ".";
                }

                return;
            }

            if (emptyMessage != null)
            {
                emptyMessage.gameObject.SetActive(false);
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);

            if (
                totalLoaded == 0 &&
                emptyMessage != null
            )
            {
                emptyMessage.gameObject.SetActive(true);
                emptyMessage.text =
                    "Could not load places.";
            }
        }
    }

    private bool MatchesCurrentFilter(
        ApiPlace place
    )
    {
        if (
            !string.Equals(
                place.city,
                selectedCity,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            return false;
        }

        if (selectedCategory == "All")
        {
            return true;
        }

        return string.Equals(
            place.category,
            selectedCategory,
            StringComparison.OrdinalIgnoreCase
        );
    }

    private void CreatePlaceCard(
        string placeId,
        string name,
        string category,
        string description,
        string imageUrl,
        string city
    )
    {
        GameObject cardObject = Instantiate(
            placeCardPrefab,
            placesContent
        );

        PlaceCard placeCard =
            cardObject.GetComponent<PlaceCard>();

        if (placeCard == null)
        {
            Debug.LogError(
                "PlaceCard script is missing from the prefab."
            );

            Destroy(cardObject);
            return;
        }

        placeCard.Setup(
            placeId,
            name,
            category,
            description,
            imageUrl,
            city
        );
    }

    private string CreatePlaceKey(
        string name,
        string city,
        string category
    )
    {
        return (
            name.Trim() +
            "|" +
            city.Trim() +
            "|" +
            category.Trim()
        ).ToLowerInvariant();
    }

    private void ClearCards()
    {
        foreach (Transform child in placesContent)
        {
            Destroy(child.gameObject);
        }
    }

    private string GetString(
        DocumentSnapshot document,
        string fieldName
    )
    {
        if (document.ContainsField(fieldName))
        {
            return document.GetValue<string>(
                fieldName
            );
        }

        return "";
    }

    public void GoBack()
    {
        SceneManager.LoadScene("Home");
    }

    [Serializable]
    private class ApiPlace
    {
        public int id;
        public string name;
        public string city;
        public string category;
        public string description;
        public string imageUrl;
        public string uploadedBy;
    }

    [Serializable]
    private class ApiPlaceList
    {
        public ApiPlace[] items;
    }
}