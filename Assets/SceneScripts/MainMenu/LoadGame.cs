using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script that just loads the game main scene
/// </summary>

public class LoadGame : MonoBehaviour
{
    public TextMeshProUGUI LoginButtonText;

    void Start() { Debug.Log($"Initialised LoadGame on {gameObject.name}"); }

    public void LoadGameAsync()
    {
        Debug.Log("LoadGameAsync() called");
        LoginButtonText.text = "Loading";
        SceneManager.LoadScene("MainScene");
    }
}
