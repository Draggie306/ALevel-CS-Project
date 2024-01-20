using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoggedInAs : MonoBehaviour
{
    public TMPro.TextMeshProUGUI LoggedInAsText;
    // Start is called before the first frame update
    void Start()
    {
        var uuid = new System.Guid(SystemInfo.deviceUniqueIdentifier);

        Debug.Log($"LoggedInAs is attached to {gameObject.name}");
        Debug.Log("Attempting to update LoggedInAsText");
        LoggedInAsText.text = $"Logged in as: {PlayerPrefs.GetString("SaturnianUsername")} ({PlayerPrefs.GetString("DraggieGamesEmail")} - {uuid.ToString()})";
        Debug.Log($"Updated LoggedInAsText to {LoggedInAsText.text}");
    }
}
