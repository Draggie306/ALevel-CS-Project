using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// When attached to a GameObject with a button, will change the text - useful for tooltips, etc.
/// </summary>

public class HoverableButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string hoverText;
    public OnHoverButtonInfo hoverInfo;

    public void OnPointerEnter(PointerEventData eventData)
    {
        hoverInfo.ChangeInfoContent(hoverText);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hoverInfo.ChangeInfoContent(hoverInfo.TextToDisplay);
    }
}