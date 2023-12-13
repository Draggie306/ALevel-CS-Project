using UnityEngine;

public class CapFPS : MonoBehaviour
{
    public int target = 60;
    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Debug.Log($"[CapFPS] Target vSync has been disabled");
        Application.targetFrameRate = target;
        Debug.Log($"[CapFPS] Target frame rate has been set to {target}");
    }
}
