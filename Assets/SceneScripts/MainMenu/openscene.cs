using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class openscene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"[openscene] Loaded on gameObject {gameObject.name}");
    }

    public void OpenScene(string sceneName)
    {
        Debug.Log($"[openscene] Opening scene {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
}
