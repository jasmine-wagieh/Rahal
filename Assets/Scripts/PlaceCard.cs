using System.Collections;
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

    private string placeName;
    private string placeCity;

    public void Setup(
        string newPlaceName,
        string category,
        string description,
        string imageUrl,
        string city
    )
    {
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
    }

    public void OpenInGoogleMaps()
    {
        string searchText = placeName + " " + placeCity;
        string encodedSearch = UnityWebRequest.EscapeURL(searchText);

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