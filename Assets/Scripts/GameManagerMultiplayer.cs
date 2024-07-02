using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GameManagerMultiplayer : MonoBehaviour
{
    private const string baseURL = "https://twinwolves-studio.com/api/tictactoe/";
    private const string authToken = "0d18e169-1708-4f25-b0da-e2c6434bc3af";

    [SerializeField]
    private GameObject[] buttons;
    [SerializeField]
    private GameObject OWinMessage;
    [SerializeField]
    private GameObject XWinMessage;
    [SerializeField]
    private GameObject drawMessage;

    private Symbol lastPlaced = Symbol.O;

    /// <summary>
    /// Restarts the match by reloading the current scene
    /// Called when the player press the restart button
    /// </summary>
    public void RestartMatch()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Place the O symbol onto the cell button with the given index then checks if somewone won
    /// Called when player presses one cell button
    /// </summary>
    /// <param name="index">The index of the button cell to place the O symbol on</param>
    public void PlayerPlace(int index)
    {
        if (lastPlaced == Symbol.O)
            Place(index, Symbol.X);
        else
            Place(index, Symbol.O);

        CheckVictory();
    }

    /// <summary>
    /// Places the given symbol onto the cell button with the given index
    /// </summary>
    /// <param name="index">The index of the button cell to place the symbol on</param>
    /// <param name="symbol">The symbol to be placed</param>
    private void Place(int index, Symbol symbol)
    {
        if (symbol == Symbol.Empty)
            return;

        lastPlaced = symbol;

        buttons[index].transform.GetChild(0).GetComponent<Text>().text = symbol.ToString();
        buttons[index].GetComponent<Button>().interactable = false;
    }

    /// <summary>
    /// Serializes the field into a string with the following symbols: O->0, X->1, Empty->_
    /// </summary>
    /// <returns>The serialized string representing the field</returns>
    private string GetSerializedField()
    {
        string field = "";
        foreach (GameObject button in buttons)
            switch (button.GetComponentInChildren<Text>().text)
            {
                case "":
                    field += "_";
                    break;
                case "O":
                    field += "0";
                    break;
                case "X":
                    field += "1";
                    break;
            }

        return field;
    }

    /// <summary>
    /// Requests the web service if someone won by passing the current field state
    /// </summary>
    private void CheckVictory()
    {
        string requestURL = baseURL + "result.php?field=" + GetSerializedField();
        StartCoroutine(DoWebRequestCoroutine(requestURL, OnCheckVictoryRequestSuccess));
    }

    /// <summary>
    /// Checks the web service response and triggers the right behaviour if someone won or not
    /// </summary>
    /// <param name="result"></param>
    private void OnCheckVictoryRequestSuccess(Dictionary<string, object> result)
    {
        string winner = JsonConvert.DeserializeObject<Dictionary<string, object>>(result["data"].ToString())["result"] as string;

        switch (winner)
        {
            case "_":
                break;
            case "x":
                ShowDrawMessage();
                break;
            case "0":
                ShowOWinMessage();
                break;
            case "1":
                ShowXWinMessage();
                break;
        }
    }

    /// <summary>
    /// Performs a web request to the given url and invokes the given callback if the the request succedes
    /// </summary>
    /// <param name="url">The requested url</param>
    /// <param name="resultCallback">The callback to call if the the request succedes</param>
    private IEnumerator DoWebRequestCoroutine(string url, Action<Dictionary<string, object>> resultCallback)
    {
        UnityWebRequest webRequest = new UnityWebRequest(url);
        webRequest.SetRequestHeader("auth", authToken);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        yield return webRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            yield break;
        }

        string jsonResponse = (webRequest.downloadHandler as DownloadHandlerBuffer).text;
        Dictionary<string, object> responseData = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonResponse);
        resultCallback.Invoke(responseData);
    }

    /// <summary>
    /// Disables all button cells so that they are no more interactable
    /// </summary>
    private void DisableAllCells()
    {
        foreach (GameObject button in buttons)
            button.GetComponent<Button>().interactable = false;
    }

    /// <summary>
    /// Shows up the message telling the match ended up with a draw
    /// </summary>
    private void ShowDrawMessage()
    {
        drawMessage.SetActive(true);
        DisableAllCells();
    }

    /// <summary>
    /// Shows up the message telling the match ended up with the player O as winner
    /// </summary>
    private void ShowXWinMessage()
    {
        XWinMessage.SetActive(true);
        DisableAllCells();
    }

    /// <summary>
    /// Shows up the message telling the match ended up with the player X as winner
    /// </summary>
    private void ShowOWinMessage()
    {
        OWinMessage.SetActive(true);
        DisableAllCells();
    }
}
