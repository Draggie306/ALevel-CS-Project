using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class TooltipHoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler 
{
    public string TooltipText = ""; 
    
    void TooltipChanger(bool showOrHide)
    {
        if (showOrHide)
        {
            TooltipScreenSpaceUI.ShowTooltip_Static(() => TooltipText);
        }
        else
        {
            TooltipScreenSpaceUI.HideTooltip_Static();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("OnPointerEnter");
        TooltipChanger(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("OnPointerExit");
        TooltipChanger(false);
    }
}