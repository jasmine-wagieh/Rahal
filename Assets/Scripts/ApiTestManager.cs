using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ApiTestManager : MonoBehaviour
{
    public static ApiTestManager Instance { get; private set; }

    public const string BaseUrl =
        "http://192.168.0.119:5208/api";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator GetPlaces(
        System.Action<string> onSuccess,
        System.Action<string> onError
    )
    {
        using UnityWebRequest request =
            UnityWebRequest.Get(BaseUrl + "/places");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.error);
            yield break;
        }

        onSuccess?.Invoke(request.downloadHandler.text);
    }
}