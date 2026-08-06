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

        selectedCity = PlayerPrefs.GetString("SelectedCity", "Cairo");

        if (cityTitle != null)
            cityTitle.text = selectedCity;

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
        foreach (Transform child in placesContent)
        {
            Destroy(child.gameObject);
        }

        if (emptyMessage != null)
        {
            emptyMessage.gameObject.SetActive(true);
            emptyMessage.text = "Loading...";
        }

        try
        {
            Query query = database
                .Collection("places")
                .WhereEqualTo("city", selectedCity);

            if (selectedCategory != "All")
            {
                query = query.WhereEqualTo("category", selectedCategory);
            }

            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            if (snapshot.Count == 0)
            {
                if (emptyMessage != null)
                    emptyMessage.text = "No places found.";

                return;
            }

            if (emptyMessage != null)
                emptyMessage.gameObject.SetActive(false);

            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                GameObject card = Instantiate(placeCardPrefab, placesContent);

                PlaceCard placeCard = card.GetComponent<PlaceCard>();

                if (placeCard != null)
                {
                    placeCard.Setup(
                        GetValue(doc, "name"),
                        GetValue(doc, "category"),
                        GetValue(doc, "description"),
                        GetValue(doc, "imageUrl"),
                        selectedCity
                    );
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);

            if (emptyMessage != null)
            {
                emptyMessage.gameObject.SetActive(true);
                emptyMessage.text = "Error loading places.";
            }
        }
    }

    private string GetValue(DocumentSnapshot doc, string field)
    {
        if (doc.ContainsField(field))
            return doc.GetValue<string>(field);

        return "";
    }

    public void GoBack()
    {
        SceneManager.LoadScene("Home");
    }
}