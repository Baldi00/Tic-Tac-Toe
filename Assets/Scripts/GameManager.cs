using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    [Serializable]
    private enum AIMode
    {
        API,
        MinimaxEasy,
        MinimaxMedium,
        MinimaxHard
    }

    [Serializable]
    private enum FirstMovePlayer
    {
        PlayerFirst,
        AIFirst
    }

    [SerializeField]
    private AIMode aiMode = AIMode.API;
    [SerializeField]
    private FirstMovePlayer firstMovePlayer = FirstMovePlayer.PlayerFirst;
    [SerializeField]
    private GameObject[] buttons;
    [SerializeField]
    private GameObject youLoseMessage;
    [SerializeField]
    private GameObject youWinMessage;
    [SerializeField]
    private GameObject drawMessage;
    [SerializeField]
    private GameObject errorMessage;
    [SerializeField]
    private GameObject networkWaitAnimation;

    private Symbol lastPlaced;
    private TTTMiniMaxAI minimaxAI;

    void Awake()
    {
        minimaxAI = new TTTMiniMaxAI();
    }

    void Start()
    {
        if (firstMovePlayer == FirstMovePlayer.AIFirst)
        {
            DisableAllCells();
            DoAINextMove();
        }
    }

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
        Place(index, Symbol.O);
        DisableAllCells();
        WebManager.Instance.CheckVictory(GetSerializedField(), OnCheckVictoryRequestSuccess, OnNetworkProgress, OnWebRequestFail);
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
    /// Requests and applies the AI next move based on the current AI mode
    /// </summary>
    private void DoAINextMove()
    {
        switch (aiMode)
        {
            case AIMode.API:
                WebManager.Instance.RequestAPINextMove(GetSerializedField(), OnAPINextMoveRequestSuccess, OnNetworkProgress, OnWebRequestFail);
                break;
            case AIMode.MinimaxEasy:
                ApplyAINextMove(minimaxAI.GetNextMove(GetSerializedField(), 1));
                break;
            case AIMode.MinimaxMedium:
                ApplyAINextMove(minimaxAI.GetNextMove(GetSerializedField(), 5));
                break;
            case AIMode.MinimaxHard:
                ApplyAINextMove(minimaxAI.GetNextMove(GetSerializedField(), 9));
                break;
        }
    }

    /// <summary>
    /// Applies the next move based on web service AI response than checks if someone won
    /// </summary>
    /// <param name="result"></param>
    private void OnAPINextMoveRequestSuccess(Dictionary<string, object> result)
    {
        networkWaitAnimation.SetActive(false);
        int nextMove = (int)(long)JsonConvert.DeserializeObject<Dictionary<string, object>>(result["data"].ToString())["nextMove"];
        ApplyAINextMove(nextMove);
    }

    /// <summary>
    /// Places the X symbol on the AI chosen position, checks if someone won and re-enables the cells
    /// </summary>
    /// <param name="nextMovePosition">The position to place the symbol on</param>
    private void ApplyAINextMove(int nextMovePosition)
    {
        Place(nextMovePosition, Symbol.X);
        WebManager.Instance.CheckVictory(GetSerializedField(), OnCheckVictoryRequestSuccess, OnNetworkProgress, OnWebRequestFail);
        EnableAvailablesCells();
    }

    /// <summary>
    /// Checks the web service response and triggers the right behaviour if someone won or not
    /// </summary>
    /// <param name="result"></param>
    private void OnCheckVictoryRequestSuccess(Dictionary<string, object> result)
    {
        networkWaitAnimation.SetActive(false);
        string winner = JsonConvert.DeserializeObject<Dictionary<string, object>>(result["data"].ToString())["result"] as string;

        switch (winner)
        {
            case "_":
                if (lastPlaced == Symbol.O)
                    DoAINextMove();
                break;
            case "x":
                ShowDrawMessage();
                break;
            case "0":
                ShowYouWinMessage();
                break;
            case "1":
                ShowYouLoseMessage();
                break;
        }
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
    /// Enables button cells that haven't already got a symbol on them
    /// </summary>
    private void EnableAvailablesCells()
    {
        foreach (GameObject button in buttons)
            if (button.transform.GetChild(0).GetComponent<Text>().text == "")
                button.GetComponent<Button>().interactable = true;
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
    /// Shows up the message telling the match ended up with the player as loser
    /// </summary>
    private void ShowYouLoseMessage()
    {
        youLoseMessage.SetActive(true);
        DisableAllCells();
    }

    /// <summary>
    /// Shows up the message telling the match ended up with the player as winner
    /// </summary>
    private void ShowYouWinMessage()
    {
        youWinMessage.SetActive(true);
        DisableAllCells();
    }

    private void OnWebRequestFail(string message)
    {
        DisableAllCells();
        errorMessage.GetComponent<Text>().text = message + "\nPlease restart";
        errorMessage.SetActive(true);
        networkWaitAnimation.SetActive(false);
    }

    private void OnNetworkProgress(float progress)
    {
        networkWaitAnimation.SetActive(true);
    }
}
