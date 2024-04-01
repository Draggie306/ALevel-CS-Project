using UnityEngine;

/// <summary>
/// Caps the FPS based on either the default value or the value set in the player prefs.
/// </summary>
/// 

public class CapFPS : MonoBehaviour
{
    // The stakeholder requires 60, but as my device is more powerful (~2x more tflops), we will use 144fps as a target so I know it will work on their system
    // See section 

    public int target = 144;

    [SerializeField]
    private bool IsEnabled = true; // default: dont want to click an extra button if we drag the script onto an objec.t

    void Start()
    {
        Debug.Log($"[CapFPS] Initialised fpsCap.cs on object: \"{gameObject.name}\"");
        if (!IsEnabled) {Debug.Log("[CapFPS] FPS Capping isnot enabled in thus scene! Not doing it"); return;}
        int TargetFPS = PlayerPrefs.GetInt("TargetFPS", target);
        Application.targetFrameRate = TargetFPS;
        Debug.Log($"[CapFPS] Target FPS set to {Application.targetFrameRate}");
    }
}
