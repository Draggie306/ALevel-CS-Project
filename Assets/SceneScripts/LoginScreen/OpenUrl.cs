using UnityEngine;

/// <summary>
/// Opens up the URL defined in the inspector
/// </summary>

// https://docs.unity3d.com/ScriptReference/Application.OpenURL.html

public class OpenACoolUrl : MonoBehaviour
{
    // Public variables allow you to set them in the Unity editor, which is useful!
    // And also SerialisedFields

    [SerializeField]
    private string url = "https://CHANGE-THIS-URL.draggiegames.com";

    void Start()
    {
        // Might be useful to see what gameObject this script is attached to
        Debug.Log("[OpenACoolUrl] Script loaded on gameObject: " + gameObject.name);
    }

    public void OnButtonClickOpenTheURL()
    {
        Application.OpenURL(url);
    }
}