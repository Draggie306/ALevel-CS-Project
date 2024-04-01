using UnityEngine;
using UnityEngine.Rendering;
 
/// <summary>
/// https://docs.unity3d.com/2021.1/Documentation/Manual/srp-setting-render-pipeline-asset.html
/// </summary>
public class SwitchRenderPipelineAsset : MonoBehaviour
{
    public RenderPipelineAsset assetToSwitchTo;
 
    void Awake()
    {
        GraphicsSettings.renderPipelineAsset = assetToSwitchTo;
        Debug.Log($"[SwitchRenderPipelineAsset] Render Pipeline Asset switched to {assetToSwitchTo.name}");
    }
}