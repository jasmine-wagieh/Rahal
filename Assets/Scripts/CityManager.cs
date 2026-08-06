using System;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
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

    private async void LoadPlaces()
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

        ClearCards();

        if (emptyMessage != null)
        {
            emptyMessage.gameObject.SetActive(true);
            emptyMessage.text = "Loading places...";
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

            QuerySnapshot snapshot =
                await query.GetSnapshotAsync();

            if (snapshot.Count == 0)
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

            foreach (
                DocumentSnapshot document
                in snapshot.Documents
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
                    continue;
                }

                placeCard.Setup(
                    document.Id,
                    GetString(document, "name"),
                    GetString(document, "category"),
                    GetString(document, "description"),
                    GetString(document, "imageUrl"),
                    selectedCity
                );
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);

            if (emptyMessage != null)
            {
                emptyMessage.gameObject.SetActive(true);
                emptyMessage.text =
                    "Could not load places.";
            }
        }
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
            return document.GetValue<string>(fieldName);
        }

        return "";
    }

    public void GoBack()
    {
        SceneManager.LoadScene("Home");
    }
}