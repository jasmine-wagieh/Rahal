using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlaceCard : MonoBehaviour
{
    [Header("UI")]
    public RawImage placeImage;
    public TMP_Text nameText;
    public TMP_Text categoryText;
    public TMP_Text descriptionText;
    public TMP_Text likeButtonText;

    private FirebaseFirestore database;
    private FirebaseAuth auth;

    private string placeId;
    private string placeName;
    private string placeCity;
    private bool isLiked;
    private bool isChangingLike;

    public void Setup(
        string newPlaceId,
        string newPlaceName,
        string category,
        string description,
        string imageUrl,
        string city
    )
    {
        database = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;

        placeId = newPlaceId;
        placeName = newPlaceName;
        placeCity = city;

        nameText.text = newPlaceName;
        categoryText.text = category;
        descriptionText.text = description;

        if (placeImage != null)
        {
            placeImage.gameObject.SetActive(false);
        }

        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            StartCoroutine(LoadImage(imageUrl));
        }

        CheckLikeStatus();
    }

    private async void CheckLikeStatus()
    {
        if (auth.CurrentUser == null || string.IsNullOrEmpty(placeId))
        {
            UpdateLikeText();
            return;
        }

        try
        {
            DocumentReference likeReference = database
                .Collection("users")
                .Document(auth.CurrentUser.UserId)
                .Collection("likedPlaces")
                .Document(placeId);

            DocumentSnapshot snapshot =
                await likeReference.GetSnapshotAsync();

            isLiked = snapshot.Exists;
            UpdateLikeText();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            UpdateLikeText();
        }
    }

    public async void ToggleLike()
    {
        if (isChangingLike)
            return;

        if (auth.CurrentUser == null)
        {
            Debug.LogError("The user must be logged in to like a place.");
            return;
        }

        isChangingLike = true;

        try
        {
            DocumentReference likeReference = database
                .Collection("users")
                .Document(auth.CurrentUser.UserId)
                .Collection("likedPlaces")
                .Document(placeId);

            if (isLiked)
            {
                await likeReference.DeleteAsync();
                isLiked = false;
            }
            else
            {
                Dictionary<string, object> likeData =
                    new Dictionary<string, object>
                    {
                        { "placeId", placeId },
                        { "name", placeName },
                        { "city", placeCity },
                        { "likedAt", Timestamp.GetCurrentTimestamp() }
                    };

                await likeReference.SetAsync(likeData);
                isLiked = true;
            }

            UpdateLikeText();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
        finally
        {
            isChangingLike = false;
        }
    }

    private void UpdateLikeText()
    {
        if (likeButtonText != null)
        {
            likeButtonText.text = isLiked ? "♥ Liked" : "♡ Like";
        }
    }

    public void OpenInGoogleMaps()
    {
        string searchText = placeName + " " + placeCity;
        string encodedSearch =
            UnityWebRequest.EscapeURL(searchText);

        string mapsUrl =
            "https://www.google.com/maps/search/?api=1&query=" +
            encodedSearch;

        Application.OpenURL(mapsUrl);
    }

    private IEnumerator LoadImage(string imageUrl)
    {
        using UnityWebRequest request =
            UnityWebRequestTexture.GetTexture(imageUrl);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(
                "Could not load image: " +
                request.error +
                "\nURL: " +
                imageUrl
            );

            yield break;
        }

        Texture2D texture =
            DownloadHandlerTexture.GetContent(request);

        placeImage.texture = texture;
        placeImage.gameObject.SetActive(true);
    }
}