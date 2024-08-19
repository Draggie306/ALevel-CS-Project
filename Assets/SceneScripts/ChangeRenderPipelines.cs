using UnityEngine;
using UnityEngine.Rendering;
 
/// <summary>
/// https://docs.unity3d.com/2021.1/Documentation/Manual/srp-setting-render-pipeline-asset.html
/// </summary>
public class SwitchRenderPipelineAsset : MonoBehaviour
{
    public RenderPipelineAsset assetToSwitchTo = null;
 
    void Awake()
    {
        if (assetToSwitchTo == null)
        {
            Debug.Log($"[SwitchRenderPipelineAsset] not forcibly switching render pipeline asset on as switch is disabled on {gameObject.name}");
            return;
        }
        Debug.Log($"[SwitchRenderPipelineAsset] Initialised on asset {gameObject.name}");
        GraphicsSettings.renderPipelineAsset = assetToSwitchTo;
        Debug.Log($"[SwitchRenderPipelineAsset] Render Pipeline Asset switched to {assetToSwitchTo.name}");
    }
}