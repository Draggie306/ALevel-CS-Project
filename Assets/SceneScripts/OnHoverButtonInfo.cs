using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class HoverChangeTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler 
{

    [SerializeField]
    private string TooltipText = "Change me in the editor";

    // [SerializeField]

    
    void ChnageTooltip(bool showOrHide)
    {
        if (showOrHide)
        {
            TooltipScreenSpaceUI.ShowTooltip_Static(() => TooltipText);
        }
        else
        {
            //  fade out
            TooltipScreenSpaceUI.HideTooltip_Static();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("OnPointerEnter");
        ChnageTooltip(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // TODO: Check if the mouse is over the tooltip. If it is, don't hide it as it currently flashes a lot
        Debug.Log("OnPointerExit");
        ChnageTooltip(false);
    }
}