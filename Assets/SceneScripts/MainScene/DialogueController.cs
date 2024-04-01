using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;
using System;

public class DialogueController : MonoBehaviour
{
    [SerializeField]
    private GameObject ImportantTextHud;
    private Coroutine ImportantTextCoroutine;
    public AudioClip WelcomeVO;
    public AudioClip HowToPlayVO;

    // Start is called before the first frame update
    void Start()
    {
        // When the scene starts, hide it, incase it was enabled in the editor.
        //ImportantTextHud.SetActive(false);

        // Hide ALL children: https://discussions.unity.com/t/get-all-children-gameobjects/89443/9

        Debug.Log("Ready to show the VO and text.");
    }

    internal IEnumerator ShowImportantText(string text, AudioClip VOToPlay = null, float DuratioToDisplayText = 5, float DurationToWaitBeforeDisplayingText = 0f)
    // internal so it can be called from StartingTheScene.cs
    // Must be IEndum as yield return requires it. https://forum.unity.com/threads/error-cannot-be-an-iterator-block-because-void-is-not-an-iterator-interface-type.144248/
    {
        // Initial check if the thing is active. This would mean that the coroutine is already running.
        if (ImportantTextCoroutine != null)
        {
            Debug.Log("ImportantTextHud is already active. Stopping the first one.");
            StopCoroutine(ImportantTextCoroutine);
        }

        // Need to keep a record of the coroutine active so we can stop it
        // This took a while: https://discussions.unity.com/t/how-to-stop-a-co-routine-in-c-instantly/49118/4
        ImportantTextCoroutine = StartCoroutine(SHowImportantText(text, VOToPlay, DuratioToDisplayText, DurationToWaitBeforeDisplayingText));
        yield return null;
    }

    public IEnumerator SHowImportantText(string text, AudioClip VOToPlay = null, float DuratioToDisplayText = 5, float DurationToWaitBeforeDisplayingText = 0f)
    {
        // Wait for the duration before showing the text
        yield return new WaitForSeconds(DurationToWaitBeforeDisplayingText);

        ImportantTextHud.GetComponent<TextMeshProUGUI>().text = text;
        ImportantTextHud.SetActive(true);

        // Enable the children of the parent gaemobject to show the text.
        // This took a while to figure out with some trial and error, but this solution works: https://discussions.unity.com/t/get-all-children-gameobjects/89443/3
        foreach (Transform child in transform) {
            // Now comppare if the tag is our target tag - in this case, "ImportantText".
            if (child.gameObject.CompareTag("ImportantText")) {
                child.gameObject.SetActive(true);
                Debug.Log($"Set child {child.name} to active.");
            } else {
                // This will catch other game objects that would otherwise be enabled.
                Debug.Log($"skipping child because it doesnt have the correct tag: {child.name}");
            }
        }
    
        // Play the voice over lines if it is set by the caller
        if (VOToPlay != null)
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            audioSource.clip = VOToPlay;
            audioSource.Play();
        }

        Debug.Log($"Set ImportantTextHud text to '{text}'");
        yield return new WaitForSeconds(DuratioToDisplayText);
        ImportantTextHud.SetActive(false);
        Debug.Log($"Hidden ImportantTextHud after {DuratioToDisplayText} seconds.");
    }
}
