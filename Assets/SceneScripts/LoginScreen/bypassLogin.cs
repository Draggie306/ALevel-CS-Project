using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
/// <summary>
/// bypasslogin is a hidden button that bypasses the login screen
/// this is in case of server errors or issues, and was useful when REplit changed thier plans to make eeverything cost more and not accessible without paying money
/// also jus easier for testign

/// the button is located at the same location as the "build version" numbering in the bottom left cornet :)
/// </summary>

public class bypassLogin : MonoBehaviour
{
    public bool bypassEnabled = true;
    public void updateInformationMessage(string message)
    {
        var informationText = GameObject.Find("StatusText");
        informationText.GetComponent<TextMeshProUGUI>().text = message;
        Debug.Log($"[updateInformationMessage] Displayed {message}");
    }

    public void OnMainSceneLoadSuccessChangeText()
    {
        Debug.Log("Main scene loaded successfully");
        var LoggedInAS = GameObject.Find("LoggedInAS");
        LoggedInAS.GetComponent<TextMeshProUGUI>().text = $"Logged in as: [Bypassed] ([Unknown])";
        Debug.Log("Unloaded initialLoading scene");
    }

    public void OnBypassButtonClick()
    {
        Debug.Log("Bypass button clicked");
        updateInformationMessage("Bypassing login...");

        try {
            var LoadingObjectInsideLoadingScene = GameObject.Find("LoadingScene");
            LoadingObjectInsideLoadingScene.GetComponent<LoadingScreen>().LoadScene(1);
            // SceneManager.LoadScene("MainScene", LoadSceneMode.Single); // this will load the main scene whilst unloading the login scene
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single); // this will load the main scene whilst unloading the login scene
            OnMainSceneLoadSuccessChangeText();
        } catch (Exception e) {
            Debug.LogError($"[OnBypassButtonClick] Cool new loading screen not found. Using old one. Error: {e}");
            // SceneManager.LoadScene("MainScene", LoadSceneMode.Single); // this will load the main scene whilst unloading the login scene
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single); // this will load the main scene whilst unloading the login scene
            OnMainSceneLoadSuccessChangeText();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("byPassLogin script loaded");
    }

}
