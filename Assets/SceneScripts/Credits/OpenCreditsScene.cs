using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenCreditsScene : MonoBehaviour
{
    public bool isLoggedOut = true;

    public void OpenUpTheCreditsScene()
    {
        Debug.Log("Loading credits scene");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Credits");
    }
}
