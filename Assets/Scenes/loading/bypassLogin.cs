using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class bypassLogin : MonoBehaviour
{
    public bool bypassEnabled = true;
    public void updateInformationMessage(string message)
    {
        var informationText = GameObject.Find("StatusText");
        informationText.GetComponent<TextMeshProUGUI>().text = message;
        Debug.Log($"[updateInformationMessage] Displayed {message}");
    }

    public async void OnBypassButtonClick()
    {
        Debug.Log("Bypass button clicked");
        updateInformationMessage("Bypassing login...");
        SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("byPassLogin script loaded");
    }

}
