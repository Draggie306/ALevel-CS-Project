using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Chad script that just loads the game main scene
/// </summary>

public class LoadGame : MonoBehaviour
{
    public TextMeshProUGUI LoginButtonText;

    public async void LoadGameAsync()
    {
        Debug.Log("LoadGameAsync() called");
        LoginButtonText.text = "Loading...";
        SceneManager.LoadScene("MainScene");
    }
}
