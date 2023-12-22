using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OnMusicHover : MonoBehaviour
{
    public string TextToDisplay = "Not playing anything"; // default text
    public TextMeshProUGUI InformationText; // the text object to change

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"Initialised OnHoverButtonInfo on {gameObject.name}");
    }

    // method to change the text of the InfoContent game object
    public void ChangeInfoContent(string newContent)
    {
        InformationText.text = newContent; // change the text
    }
}