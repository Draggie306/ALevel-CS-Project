using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoggedInAs : MonoBehaviour
{
    public TMPro.TextMeshProUGUI LoggedInAsText;
    // Start is called before the first frame update
    void Start()
    {
        PlayerPrefs.SetString("SaturnianUsername", "Guest");
        var deviceUID = SystemInfo.deviceUniqueIdentifier;
        string uuid = System.Guid.NewGuid().ToString();

        Debug.Log($"LoggedInAs is attached to {gameObject.name}");
        Debug.Log("Attempting to update LoggedInAsText");
        LoggedInAsText.text = $"Logged in as: {PlayerPrefs.GetString("SaturnianUsername")} ({PlayerPrefs.GetString("DraggieGamesEmail")})\ndevice id: {deviceUID.ToString()}\nmatchmaker session id: {uuid}";
        Debug.Log($"Updated LoggedInAsText to '{LoggedInAsText.text}'");
    }
}
