using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.Networking;

/// <summary>
/// Work-in-progress script to get account details from the server
/// Need to creatre a custom endpoint for this
/// Very secure hehehe
/// </summary>

public class AccountDetails : MonoBehaviour
{
    private void SetTextMeshProUGUI(string gameObjectName, string text)
    {
        TextMeshProUGUI textMeshProUGUI = GameObject.Find(gameObjectName).GetComponent<TextMeshProUGUI>();
        textMeshProUGUI.SetText(text);
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
        const string ValidationPath = "/api/v1/saturnian/game/gameData/licenses/validation";
        /*                         
                        client.DefaultRequestHeaders.Add("Authorisation", $"{accessToken}");
                        client.DefaultRequestHeaders.Add("User-Agent", "unity/draggiegames-compsciproject");
                        client.DefaultRequestHeaders.Add("Content-Type", "application/json");

        */

        string token = UnityEngine.PlayerPrefs.GetString("accessToken");

        Debug.Log($"Token: {token}");

        UnityWebRequest request = new UnityWebRequest(baseUrl + ValidationPath, "GET");

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
            var response = JsonUtility.FromJson<Response>(request.downloadHandler.text);

            SetTextMeshProUGUI("UsernameDisplay", response.username);
            SetTextMeshProUGUI("EmailDisplay", response.email);
            SetTextMeshProUGUI("VerifiedDisplay", response.verified.ToString());
            SetTextMeshProUGUI("VerifiedDateDisplay", response.verified_date);
            SetTextMeshProUGUI("EntitlementsDisplay", response.entitlements);
            SetTextMeshProUGUI("CurrentTokenExpirationDisplay", response.current_token_expiration);
            SetTextMeshProUGUI("CodesRedeemedDisplay", response.codes_redeemed);
            SetTextMeshProUGUI("StatusDisplay", response.status);
            SetTextMeshProUGUI("VerificationPendingDisplay", response.verification_pending.ToString());
            SetTextMeshProUGUI("UserLangDisplay", response.user_lang);
            SetTextMeshProUGUI("LastActivityDisplay", response.last_activity);
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