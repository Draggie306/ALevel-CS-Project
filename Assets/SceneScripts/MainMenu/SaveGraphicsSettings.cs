using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles when the user presses on the settings buttons in the graphics settings (sub)menu.
/// </summary>
public class GraphicsSettingsHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject FPSInputField;

    [SerializeField]
    private GameObject VSyncEnabledButton;
    [SerializeField]
    private GameObject VSyncEnabledText;

    [SerializeField]
    private GameObject RayTracingEnabledText;
    [SerializeField]
    private GameObject RayTracingEnabledButton;

    [SerializeField]
    private GameObject PostProcessingEnabledText;
    [SerializeField]
    private GameObject PostProcessingEnabledButton;

    [SerializeField]
    private GameObject ApplyButton;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"[SaveGraphicsSettings] GOt the following FPSInputField: {FPSInputField.name}, VSyncEnabledButton: {VSyncEnabledButton.name}, RayTracingEnabledButton: {RayTracingEnabledButton.name}, ApplyButton: {ApplyButton.name}");
        ApplyButton.SetActive(false);
        Debug.Log($"[SaveGraphicsSettings] Initialised on script {name}");

        // Read the current settings from PlayerPrefs
        int TargetFPS = PlayerPrefs.GetInt("TargetFPS", 60);
        int VSyncEnabled = PlayerPrefs.GetInt("VSyncEnabled", 0);
        int RayTracingEnabled = PlayerPrefs.GetInt("RayTracingEnabled", 0);
        int PostProcessingLevel = PlayerPrefs.GetInt("PostProcessingLevel", 2);

        // Se the ui elements to display the current settings
        FPSInputField.GetComponent<TMP_InputField>().text = TargetFPS.ToString();
        VSyncEnabledText.GetComponent<TMP_Text>().text = VSyncEnabled == 1 ? "Enabled" : "Disabled"; // Shorthand if statement
        RayTracingEnabledText.GetComponent<TMP_Text>().text = RayTracingEnabled == 1 ? "Enabled" : "Disabled";

        // Levels of graphics tier settings: 1 = low, 2 = medium, 3 = high
        PostProcessingEnabledText.GetComponent<TMP_Text>().text = PostProcessingLevel switch
        {
            0 => "Low",
            1 => "Medium",
            2 => "High",
            _ => "High",
        };

        // Extra checks using SystemInfo to see if the system supports ray tracing.
        if (!SystemInfo.supportsRayTracing) {
            foreach (Transform child in RayTracingEnabledButton.transform)
            {
                Debug.Log($"[SaveGraphicsSettings] RayTracingEnabledButton child: {child.name}");
            }
            RayTracingEnabledButton.GetComponent<Button>().interactable = false;
            RayTracingEnabledButton.GetComponentInChildren<TMP_Text>().text = "System unsupported";
        }
    }

    public void DisplaySaveButton()
    // In case we just need to display the button, e.g. for the fps input field
    {
        ApplyButton.SetActive(true);
    }

    // Method called when button is clicked
    public void OnSettingButtonClicked(GameObject TheButtonClicked)
    {
        if (TheButtonClicked == VSyncEnabledButton)
        {
            Debug.Log($"[SaveGraphicsSettings] VSyncEnabledButton clicked");
            VSyncEnabledText.GetComponent<TMP_Text>().text = VSyncEnabledText.GetComponent<TMP_Text>().text == "Enabled" ? "Disabled" : "Enabled";
            DisplaySaveButton();
        }

        if (TheButtonClicked == RayTracingEnabledButton)
        {
            Debug.Log($"[SaveGraphicsSettings] RayTracingEnabledButton clicked");
            RayTracingEnabledText.GetComponent<TMP_Text>().text = RayTracingEnabledText.GetComponent<TMP_Text>().text == "Enabled" ? "Disabled" : "Enabled";
            DisplaySaveButton();
        }

        if (TheButtonClicked == PostProcessingEnabledButton)
        {
            Debug.Log($"[SaveGraphicsSettings] PostProcessingEnabledButton clicked");
            PostProcessingEnabledText.GetComponent<TMP_Text>().text = PostProcessingEnabledText.GetComponent<TMP_Text>().text == "Enabled" ? "Disabled" : "Enabled";
            DisplaySaveButton();
        }

        if (TheButtonClicked == PostProcessingEnabledButton)
        {
            Debug.Log($"[SaveGraphicsSettings] PostProcessingEnabledButton clicked");
            
            // Curent level of post processing
            int PostProcessingLevel = PlayerPrefs.GetInt("PostProcessingLevel", 0);
            PostProcessingLevel = (PostProcessingLevel + 1) % 3; // 0, 1, 2, 0, 1, 2, ...
            PlayerPrefs.SetInt("PostProcessingLevel", PostProcessingLevel);

            // Updates the UI button text to show what the current setting is
            PostProcessingEnabledText.GetComponent<TMP_Text>().text = PostProcessingLevel switch
            {
                0 => "Low",
                1 => "Medium",
                2 => "High",
                _ => "Disabled",
            };
            DisplaySaveButton();
        }
    }

    // Final method called when specifically the Save Settings button is clicked
    public void OnSaveButtonClick() // public so it can be selected in the editor, yes, even for methods
    {
        int TargetFPS = -1; // Unlimited - default
        try {
            TargetFPS = int.Parse(FPSInputField.GetComponent<TMP_InputField>().text);
        } catch (System.Exception e)
        {
            Debug.LogWarning($"[SaveGraphicsSettings] Error parsing FPS input field - it must be a number: {e.Message}");
        }
        PlayerPrefs.SetInt("TargetFPS", TargetFPS);
        Application.targetFrameRate = TargetFPS;
        Debug.Log($"[SaveGraphicsSettings] Target FPS set to {TargetFPS}");

        if (VSyncEnabledText.GetComponent<TMP_Text>().text == "Enabled") {
            QualitySettings.vSyncCount = 1; // Enabled for every frame
        }
        else {
            QualitySettings.vSyncCount = 0; // Disable it, uncap fps.
        }
        PlayerPrefs.SetInt("VSyncEnabled", QualitySettings.vSyncCount);
        Debug.Log($"[SaveGraphicsSettings] VSync set to {QualitySettings.vSyncCount}");

        PlayerPrefs.SetInt("RayTracingEnabled", RayTracingEnabledText.GetComponent<TMP_Text>().text == "Enabled" ? 1 : 0);
        Debug.Log($"[SaveGraphicsSettings] RayTracing set to {RayTracingEnabledText.GetComponent<TMP_Text>().text}");

        Debug.Log($"[SaveGraphicsSettings] PostProcessingEnabledText: {PostProcessingEnabledText.GetComponent<TMP_Text>().text}");
        switch (PostProcessingEnabledText.GetComponent<TMP_Text>().text)
        {
            case "Low":
                QualitySettings.SetQualityLevel(1);
                break;
            case "Medium":
                QualitySettings.SetQualityLevel(2);
                break;
            case "High":
                QualitySettings.SetQualityLevel(3);
                break;
            default:
                QualitySettings.SetQualityLevel(1);
                break;
        }
        ApplyButton.SetActive(false);
    }
}
