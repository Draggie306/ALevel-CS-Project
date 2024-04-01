using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>
/// Quits the player when the user hits escape
/// https://forum.unity.com/threads/quitting-game-by-pressing-esc-escape-key.562033/
/// </summary>

public class EscapeKeyHandler : MonoBehaviour

{
    void Start()
    {
        Debug.Log($"[QuitGameOnEscape] Starting on GameObject: {gameObject.name}. Make sure there is only one of these in the scene!");
    }
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                Debug.Log("[QuitGameOnEscape] Quitting game from main menu");
                Application.Quit();
            }
            else
            {
                Debug.Log("[QuitGameOnEscape] Loading main menu.");
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            }
        }
    }
}