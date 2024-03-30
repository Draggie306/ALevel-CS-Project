using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using JetBrains.Annotations;

/// <summary>
/// https://www.youtube.com/watch?v=YUIohCXt_pc 
/// </summary>
public class TooltipScreenSpaceUI : MonoBehaviour
{
    public static TooltipScreenSpaceUI Instance { get; private set; }

    [SerializeField]
    private RectTransform canvasRectTransform;
    private RectTransform backgoundRectTransform;
    private TextMeshProUGUI textMeshPro;
    private RectTransform rectTransform;
    public bool IsMouseOverTooltip;
    public Vector3 Offset = new(4, 8);
    private System.Func<string> getTooltipTextFunc;
    private void Awake()
    {
        Instance = this;

        backgoundRectTransform = transform.Find("background").GetComponent<RectTransform>();
        textMeshPro = transform.Find("text").GetComponent<TextMeshProUGUI>(); 
        Debug.Log("TooltipScreenSpaceUI Awake");

        rectTransform = transform.GetComponent<RectTransform>();

        // SetText("Hello World!");
        HideTooltip();
    }

    private void SetText(string tooltipText)
    {
        textMeshPro.SetText(tooltipText);
        textMeshPro.ForceMeshUpdate();

        Vector2 textSize = textMeshPro.GetRenderedValues(false);
        Vector2 padding = new(10, 8);
        backgoundRectTransform.sizeDelta = textSize + padding;

        // Comment out else log gets spammed    
        // Debug.Log($"[TooltipScreenSpaceUI] Set tooltip text to '{tooltipText}'");
    }


    void Update()
    {
        var CheckIfMouseIsOverTooltip = RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition);
        // Debug.Log($"[TooltipScreenSpaceUI] CheckIfMouseIsOverTooltip: {CheckIfMouseIsOverTooltip}");
        SetText(getTooltipTextFunc());

        Vector2 anchoredPosition = Input.mousePosition / canvasRectTransform.localScale.x + Offset;// add offset to make it appear above the mouse

        if (anchoredPosition.x + backgoundRectTransform.rect.width > canvasRectTransform.rect.width) 
        {
            anchoredPosition.x = canvasRectTransform.rect.width - backgoundRectTransform.rect.width;
        }

        if (anchoredPosition.y + backgoundRectTransform.rect.height > canvasRectTransform.rect.height)
        {
            anchoredPosition.y = canvasRectTransform.rect.height - backgoundRectTransform.rect.height;
        }
        rectTransform.anchoredPosition = anchoredPosition;
    }


    private void ShowTooltip(string tooltipText)
    {
        gameObject.SetActive(true);
        SetText(tooltipText);
    }

    private void ShowTooltip(System.Func<string> getTooltipStringFunc)
    {
        this.getTooltipTextFunc = getTooltipStringFunc;
        gameObject.SetActive(true);
        SetText(getTooltipStringFunc());
    }

    private void HideTooltip()
    {
        // Todo: Fade out animation: https://stackoverflow.com/questions/44933517/fading-in-out-gameobject
        // todo: And check if the mouse is over the tooltip. 

        gameObject.SetActive(false);
    }
    public static void ShowTooltip_Static(string tooltipText)
    {
        Instance.SetText(tooltipText);
    }

    public static void ShowTooltip_Static(System.Func<string> getTooltipStringFunc)
    {
        Instance.ShowTooltip(getTooltipStringFunc);
    }

    public static void HideTooltip_Static()
    {
        Instance.HideTooltip();
    }

    public bool FuncIsMouseOverTooltip()
    {
        //https://docs.unity3d.com/ScriptReference/RectTransformUtility.RectangleContainsScreenPoint.html
        // Looks good to me
        
        // Grab the value from the update function
        return IsMouseOverTooltip;
    }

    public Rect GetRect()
    {
        return new Rect(rectTransform.position, backgoundRectTransform.sizeDelta);
    }
}
