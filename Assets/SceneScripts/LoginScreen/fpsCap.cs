using UnityEngine;

/// <summary>
/// Only have one per scene
/// </summary>
/// 

public class CapFPS : MonoBehaviour
{
    public int target = 60; // Can be changed in the Unity editor
    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Debug.Log($"[CapFPS] Target vSync has been disabled");
        Application.targetFrameRate = target;
        Debug.Log($"[CapFPS] Target frame rate has been set to {target}");
    }
}
