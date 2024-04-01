using UnityEngine;
using TMPro;

/// <summary>
/// Displays name of sound being played when being hovered over by the mouse.
/// </summary>

public class OnMusicHover : MonoBehaviour
{
    public string TextToDisplay = "Not playing anything"; // default text
    public TextMeshProUGUI InformationText; // this is the text object to change

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"Initialised OnHoverButtonInfo on {gameObject.name}");
    }

    // change the contenet of the text on sthe screen
    public void ChangeInfoContent(string newContent)
    {
        InformationText.text = newContent;
    }
}