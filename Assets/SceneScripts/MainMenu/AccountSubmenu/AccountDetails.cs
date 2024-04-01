using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.Networking;
using Newtonsoft.Json;

/// <summary>
/// Gets the account details from the user as determined by the token from the server and diplays them in the submenu.
/// </summary>

public class AccountDetails : MonoBehaviour
{
    public TextMeshProUGUI TextAreaToDisplay;
    private void SetTextMeshProUGUI(string text)
    {
        var textMeshProUGUI = TextAreaToDisplay;
        string existingCOntent = textMeshProUGUI.text;
        textMeshProUGUI.SetText(existingCOntent + "\n" + text);
    }

    private void ClearTextMeshProUGUI()
    {
        var textMeshProUGUI = TextAreaToDisplay;
        textMeshProUGUI.SetText("");
    }

    public string baseUrl;

    void Start()
    {
        Debug.Log($"Initialised AccountDetails.cs on gameObject: {gameObject.name}");
    }

    public void OnGetDetailsButtonClicked()
    {
        var ErrorText = GameObject.Find("ErrorText").GetComponent<TextMeshProUGUI>();
        Debug.Log("Get Details button clicked");
        var TextToChange = GameObject.Find("GetDetailsButtonText").GetComponent<TextMeshProUGUI>();
        try
        {
            TextToChange.SetText("Loading...");
            StartCoroutine(GetAccountDetails());
            TextToChange.SetText("Refresh details");
        }
        catch (Exception e)
        {
            Debug.Log(e);
            ErrorText.SetText("Error: " + e);
            TextToChange.SetText("Update Account Details");
        }
    }

    IEnumerator GetAccountDetails()
    {
        ClearTextMeshProUGUI();
        const string ValidationPath = "/api/v1/saturnian/game/accountInfo";
        string token = UnityEngine.PlayerPrefs.GetString("accessToken");

        Debug.Log($"Token: {token}");

        UnityWebRequest request = new(baseUrl + ValidationPath, "GET");

        request.SetRequestHeader("Authorisation", token);
        request.SetRequestHeader("User-Agent", "unity/draggiegames-compsciproject");
        request.SetRequestHeader("Content-Type", "application/json");

        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            var ErrorText = GameObject.Find("ErrorText").GetComponent<TextMeshProUGUI>();
            ErrorText.SetText("Error: " + request.error);
        }
        else
        {
            string StrResponse = request.downloadHandler.text;
            Debug.Log($"Response: {StrResponse}");

            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(StrResponse);
            Debug.Log($"Dict: {dict}");

            foreach (var item in dict)
            {
                string valueStr = JsonConvert.SerializeObject(item.Value);
                Debug.Log($"Key: {item.Key}, Value: {valueStr}");
                SetTextMeshProUGUI($"{item.Key}: {valueStr}");
            }
        }
    }

    // holds the response data from the server
    [Serializable] // because it is required for JsonUtility to work
    public class Response
    {
        public bool error;
        public string message;
        public string username;
        public string id;
        public string email;
        public bool verified;
        public string verified_date;
        public string entitlements;
        public string current_token_expiration;
        public string codes_redeemed;
        public string status;
        public bool verification_pending;
        public string user_lang;
        public string last_activity;
    }
}