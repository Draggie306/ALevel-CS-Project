using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Quits the player when the user hits escape
/// https://forum.unity.com/threads/quitting-game-by-pressing-esc-escape-key.562033/
/// </summary>

public class QuitGameOnEscape : MonoBehaviour

{
    void Start()
    {
        Debug.Log($"[QuitGameOnEscape] Starting on GameObject: {gameObject.name}. Make sure there is only one of these in the scene!");
    }
    void Update()
    {
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
    }
}