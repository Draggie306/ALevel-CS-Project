using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Simple script to display the number of times the game has been beaten in the main menu.
/// </summary>

public class TimesBeatenGame : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textToDisplay;
    // Start is called before the first frame update
    void Awake()
    {
        int TimesBeaten = PlayerPrefs.GetInt("TimesBeaten", 0);
        Debug.Log($"Times beaten: {TimesBeaten}");
        if (TimesBeaten == 0) {
            // Hide the text if the game has not been beaten yet
            textToDisplay.gameObject.SetActive(false);
        }
        else {
            // Display and read how many times the game has been beaten!
            textToDisplay.gameObject.SetActive(true);
            textToDisplay.text = $"Times beaten game: {TimesBeaten}";
    }}
}
