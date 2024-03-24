using UnityEngine;

/// <summary>
/// Only have one per scene
/// </summary>
/// 

public class CapFPS : MonoBehaviour
{
    // The stakeholder requires 60, but as my device is more powerful (~2x more tflops), we will use 144fps as a target so I know it will work on their system
    // See section 

    public int target = 144;

    [SerializeField] // todo, writeup: this can make private fields visible in the editor
    private bool IsEnabled = true; // default: dont want to click an extra button if we drag the script onto an objec.t

    void Start()
    {
        Debug.Log($"[CapFPS] Initialised fpsCap.cs on object: \"{gameObject.name}\"");
        if (!IsEnabled) {Debug.Log("[CapFPS] FPS Capping isnot enabled in thus scene! Not doing it"); return;}
        QualitySettings.vSyncCount = 0;
        Debug.Log($"[CapFPS] Target vSync has been disabled to allow for frameratecapping of {target}, see below");
        Application.targetFrameRate = target;
        Debug.Log($"[CapFPS] Target frame rate has been set to {target}");
    }
}
