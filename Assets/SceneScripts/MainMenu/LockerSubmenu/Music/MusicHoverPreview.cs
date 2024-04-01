using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Script to play a sound when hovering over an object
/// </summary>

// References:
// Scene transition: https://www.youtube.com/watch?v=HBEStd96UzI
// Keep audio playing between scenes: https://www.youtube.com/watch?v=xswEpNpucZQ

public class MusicHoverPreview : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    public string hoverText;
    public OnMusicHover hoverInfo;
    public AudioClip hoverSound;
    private AudioSource audioSource;
    private bool audioPlaying = false;
    private AudioSource[] allAudioSources;

    public void StopAllAudio() {
        allAudioSources = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
        foreach( AudioSource audioS in allAudioSources) {
            Debug.Log($"[StopAllAudio] Stopping audio source {audioS.clip.name} on {audioS.gameObject.name}");
            audioS.Stop();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("No AudioSource found on " + gameObject.name);
        } else
        {
            audioSource.clip = hoverSound;
            Debug.Log($"Assigned audio clip '{hoverSound.name}' to {gameObject.name}");
        }
    }


    // Audio may break if hover is spammed
    // TO fix this, keep track of whether the audio is playing or not

    private Coroutine audioFadeInCoroutine;
    private Coroutine audioFadeOutCoroutine;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverInfo != null)
        {
            hoverInfo.ChangeInfoContent(hoverText);
        }
        Debug.Log($"Now hovering over {gameObject.name}");
        if (audioPlaying) { StopAllAudio(); }
        if (audioSource != null && hoverSound != null)
        {
            if (!audioSource.enabled) { audioSource.enabled = true; }
            if (audioSource.isPlaying) { audioSource.Stop(); }
            if (audioFadeOutCoroutine != null) { StopCoroutine(audioFadeOutCoroutine); Debug.Log($"determined fade out coroutine was not null, stopping playing sound {hoverSound.name}"); }
            audioFadeInCoroutine = StartCoroutine(AudioFadeIn.FadeIn(audioSource, 0.7f));
            Debug.Log($"Playing sound {hoverSound.name}");
            audioPlaying = true;
        } else
        {
            Debug.LogWarning($"AudioSource or hoverSound was null on {gameObject.name}");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverInfo != null)
        {
            hoverInfo.ChangeInfoContent(hoverInfo.TextToDisplay);
        }
        Debug.Log($"No longer hovering over {gameObject.name}");
        if (audioSource != null && hoverSound != null)
        {
            // TODO: if the audio button in unity is "selected", then don't stop the audio
            // Early return if the user has selected the audio, don't stop it
            if (EventSystem.current.currentSelectedGameObject != null) {
                //Debug.Log($"[OnPointerEnter] The current game object is {EventSystem.current.currentSelectedGameObject.name} and is selected. Returning early as we do not need to stop the audio");
                // return; // early return

                Debug.Log($"[OnPointerEnter] would early return but bug says no");
            }
            
            if (audioFadeInCoroutine != null) { StopCoroutine(audioFadeInCoroutine); Debug.Log($"determined fade in coroutine was not null, stopping playing sound {hoverSound.name}"); }
            audioFadeOutCoroutine = StartCoroutine(AudioFadeOut.FadeOut(audioSource, 0.3f));
            Debug.Log("Stopping sound");
            audioPlaying = false;
        }
    }
}