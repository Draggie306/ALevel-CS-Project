using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Test script to display the starter dialogue and text on screen.
/// </summary>
/// 

public class VoiceoversAndTextodisplayOnStart : MonoBehaviour
{
    // Todo: Future development - make various changes e.g. list of AudioClips, text array that is public for editr changing, etc
    public AudioClip WelcomeVO = null;

    // Should check if already player via PayerPrefs:
    public AudioClip HowToPlayVO = null;
    // The below ones are to be randomly played
    public AudioClip BeyondNextHorizon = null;
    public AudioClip EasyPeasy = null;
    public AudioClip YouCanCollectTokens = null;
    // The last clip
    public AudioClip TenMinutes = null;
    public AudioClip WaterIsRising = null;
    public AudioClip GoodLuckOutThere = null;

    private bool hasPlayedGame = false;

    void Awake()
    {
        // Settings and quality options from player prefs
        if (PlayerPrefs.GetInt("TargetFPS", 60) != 0)
        {
            Application.targetFrameRate = PlayerPrefs.GetInt("TargetFPS", 60);
        }
        QualitySettings.vSyncCount = PlayerPrefs.GetInt("VSyncEnabled", 0);
        Debug.Log($"[Awake] Target FPS set to {Application.targetFrameRate}, VSync set to {QualitySettings.vSyncCount}");

        // Has already played? check
        if (PlayerPrefs.GetInt("HasPlayedGame", 0) == 1)
        {
            // Skip longer intro dialogue if have already played.
            hasPlayedGame = true;
            Debug.Log("Has played game before, skipping intro dialogue.");
        }
        PlayerPrefs.SetInt("HasPlayedGame", 1);
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SceneStarter());

        // Call ShowImportantText from DialogueController
        
        // AudioSource audioSource = GetComponent<AudioSource>();
    }

    IEnumerator SceneStarter()
    {
        DialogueController dialogueController = FindObjectOfType<DialogueController>();
        StartCoroutine(dialogueController.ShowImportantText("Welcome to the simulation. Or, as we like to call it, the game.", WelcomeVO, 4.23f, 2.0f));
        yield return new WaitForSeconds(5);

        if (! hasPlayedGame)
        {
            StartCoroutine(dialogueController.ShowImportantText("You can use your typical WASD keys to move around, and the mouse to look around. Use the spacebar to jump, the left mouse button to grapple onto objects, and \"E\" to interact with objects.", HowToPlayVO, 12.0f, 2.0f));
            yield return new WaitForSeconds(14.5f);
        }

        // Randomly play a clip to make it less repetitive
        AudioClip[] randomClips = { BeyondNextHorizon, EasyPeasy, YouCanCollectTokens };
        AudioClip randomClip = randomClips[Random.Range(0, randomClips.Length)];
        if (randomClip == BeyondNextHorizon)
        {
            StartCoroutine(dialogueController.ShowImportantText("Beyond the next horizon lies a challenge untold. Will you rise as the tide, or like so many before, recede into the Dangerous Depths?", randomClip, 7.0f, 1.0f));
            yield return new WaitForSeconds(8);
        }
        else if (randomClip == EasyPeasy)
        {
            StartCoroutine(dialogueController.ShowImportantText("You're just here to collect some tokens, right? Easy peasy!", randomClip, 4.23f, 1.0f));
            yield return new WaitForSeconds(4);
        } else
        {
            StartCoroutine(dialogueController.ShowImportantText("You can collect powerful tokens scattered seldomly around the map to fight back against the water.", randomClip, 4.4f, 1.0f));
            yield return new WaitForSeconds(5.4f);
        }
        StartCoroutine(dialogueController.ShowImportantText("Oh, did I mention that water will be constantly rising? You'll need to keep moving around to dodge it.", WaterIsRising, 5.9f, 2.0f));
        yield return new WaitForSeconds(6.2f);
        StartCoroutine(dialogueController.ShowImportantText("The simulation will end and you will be extracted back to reality in ten minutes. That is, unless you become enveloped first...", TenMinutes, 7.0f, 2.0f));
        yield return new WaitForSeconds(7.6f);
        StartCoroutine(dialogueController.ShowImportantText("Good luck out there!", GoodLuckOutThere, 2.0f, 1.0f));
    }
}
