using UnityEngine;
using UnityEngine.EventSystems;

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