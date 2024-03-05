using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeSceneBack : MonoBehaviour

// TODO (05/03/2024): Please make this not log the user out, if they are logged in when they click the credits, make sure 
// that the scope can be interacted with on click. Not sure how to make this persistent, use playerprefs if needed
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
