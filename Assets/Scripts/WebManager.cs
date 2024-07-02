using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[DisallowMultipleComponent]
public class WebManager : MonoBehaviour
{
    private static WebManager _instance;

    public string baseURL = "";
    public string authToken = "";
    public string nextMoveURL = "nextmove.php";
    public string checkVictoryURL = "result.php";
    public float timeRequestIsConsideredLong = 1;

    public static WebManager Instance { get => _instance; }

    void Awake()
    {
        if (_instance == null)
            _instance = this;
        else if (_instance != this)
            Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    /// <summary>
    /// Requests the web service AI the next move to make based on the current field state
    /// <param name="successCallback">The callback called if the request succedes</param>
    /// <param name="progressCallback">The callback called if the request is in progress since a set long time</param>
    /// <param name="failCallback">The callback called if the the request fails</param>
    /// </summary>
    public void RequestAPINextMove(string serializedField, Action<Dictionary<string, object>> successCallback, Action<float> progressCallback, Action<string> failCallback)
    {
        string requestURL = baseURL + nextMoveURL + "?field=" + serializedField;
        StartCoroutine(DoWebRequestCoroutine(requestURL, successCallback, progressCallback, failCallback));
    }


    /// <summary>
    /// Requests the web service if someone won by passing the current field state
    /// <param name="successCallback">The callback called if the request succedes</param>
    /// <param name="progressCallback">The callback called if the request is in progress since a set long time</param>
    /// <param name="failCallback">The callback called if the the request fails</param>
    /// </summary>
    public void CheckVictory(string serializedField, Action<Dictionary<string, object>> successCallback, Action<float> progressCallback, Action<string> failCallback)
    {
        string requestURL = baseURL + checkVictoryURL + "?field=" + serializedField;
        StartCoroutine(DoWebRequestCoroutine(requestURL, successCallback, progressCallback, failCallback));
    }

    /// <summary>
    /// Performs a web request to the given url and invokes the given callback if the the request succedes
    /// </summary>
    /// <param name="url">The requested url</param>
    /// <param name="successCallback">The callback called if the request succedes</param>
    /// <param name="progressCallback">The callback called if the request is in progress since a set long time</param>
    /// <param name="failCallback">The callback called if the the request fails</param>
    public IEnumerator DoWebRequestCoroutine(string url, Action<Dictionary<string, object>> successCallback, Action<float> progressCallback, Action<string> failCallback)
    {
        UnityWebRequest webRequest = new UnityWebRequest(url);
        webRequest.SetRequestHeader("auth", authToken);
        webRequest.downloadHandler = new DownloadHandlerBuffer();

        // Start request
        UnityWebRequestAsyncOperation webRequestResult = webRequest.SendWebRequest();
        DateTime startTime = DateTime.Now;
        
        // Request progress, if lasts too long calls on progress
        bool progressCalled = false;
        while (!webRequestResult.isDone)
        {
            if (!progressCalled && DateTime.Now.Subtract(startTime).Seconds >= timeRequestIsConsideredLong)
            {
                progressCallback.Invoke(webRequest.downloadProgress);
                progressCalled = true;
            }
            yield return null;
        }

        // Request failed
        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            failCallback.Invoke($"Network error (Request result: {webRequest.result})");
            yield break;
        }

        // Request succeded
        string jsonResponse = (webRequest.downloadHandler as DownloadHandlerBuffer).text;
        Dictionary<string, object> responseData = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonResponse);

        if (responseData["status"] as string != "success")
        {
            failCallback.Invoke(responseData["message"] as string);
            yield break;
        }

        successCallback.Invoke(responseData);
    }

}
