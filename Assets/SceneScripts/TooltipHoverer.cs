using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Net.Security;

public class TooltipHoverButtonUseMeInEditor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler 
{
    public string TooltipText = "";
    public float OffsetX = 4;
    public float OffsetY = 8;
    void TooltipChanger(bool showOrHide)
    {
        if (showOrHide)
        {
            TooltipScreenSpaceUI.ShowTooltip_Static(() => TooltipText);
        }
        else
        {
            /*
            while (!CheckIfMouseIsOverTooltip()) // This will cause an infinite loop - don't do it again!
            {
                TooltipScreenSpaceUI.HideTooltip_Static();
            }
            */
            TooltipScreenSpaceUI.HideTooltip_Static();
        }
    }

    // Store the coroutine so it can be stopped if the user enters the area again
    private Coroutine delayedTooltipHideCoroutine;
    // Now we need a test case to see if it is on a didfferent object, if it is it should not be hidden
    private static TooltipHoverButtonUseMeInEditor currentHoveredObject; // Omg static actually fixed it

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("OnPointerEnter");
        TooltipChanger(true);
        if (delayedTooltipHideCoroutine != null)
        {
            StopCoroutine(delayedTooltipHideCoroutine);
            //Debug.Log($"tooltip fixer: {delayedTooltipHideCoroutine}");
            delayedTooltipHideCoroutine = null;
        }
        currentHoveredObject = this;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("OnPointerExit");
        delayedTooltipHideCoroutine = StartCoroutine(DelayedTooltipHide());
    }

    private IEnumerator DelayedTooltipHide()
    {
        yield return new WaitForSeconds(0.2f);
        // dont hide if the mouse is over the  tooltip OR is not this object
        // this is so confusing
        if (currentHoveredObject != this || TooltipScreenSpaceUI.Instance.IsMouseOverTooltip)
        {
            //Debug.Log($"the mouse is over the tooltip or not this object (currentHoveredObject: {currentHoveredObject}, this: {this})");
            yield break; // exit coro early as tooltip sgouldn't be hidden.
        }
        TooltipChanger(false);
    }
}