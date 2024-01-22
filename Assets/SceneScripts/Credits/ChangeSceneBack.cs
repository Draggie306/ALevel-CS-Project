using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeSceneBack : MonoBehaviour
{
    public bool isLoggedOut = true;

    // Start is called before the first frame update
    public void OnButtonIsClickedToChangeSceneBackFromTheCreditsScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("initialLoading");

        
    }
}

/*
if (isLoggedOut)
        {
            Debug.Log("Loading signin scene");
            UnityEngine.SceneManagement.SceneManager.LoadScene("initialLoading");
        } else
        {
            Debug.Log("Loading main scene as user is logged in");
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

*/