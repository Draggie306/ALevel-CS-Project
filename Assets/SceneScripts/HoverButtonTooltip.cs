using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HoverButtonTooltip : MonoBehaviour
{
    public string TextToDisplay = ""; // default text // Hover over a button to see its description
    public TextMeshProUGUI InformationText; // the text object to change

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Initialised OnHoverButtonInfo");
    }

    // method to change the text of the InfoContent game object
    public void ChangeInfoContent(string newContent)
    {
        // check if information text is null
        if (InformationText == null)
        {
            Debug.Log("InformationText is null, cannot change text");
            return;
        }
        InformationText.text = newContent; // change the text
    }
}