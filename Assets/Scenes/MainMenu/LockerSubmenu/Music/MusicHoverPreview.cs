using UnityEngine;
using UnityEngine.EventSystems;



public class MusicHoverPreview : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string hoverText;
    public OnMusicHover hoverInfo;
    public AudioClip hoverSound;
    private AudioSource audioSource;

    private bool audioPlaying = false;

    private AudioSource[] allAudioSources;

    public void StopAllAudio() {
        allAudioSources = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
        foreach( AudioSource audioS in allAudioSources) {
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
        hoverInfo.ChangeInfoContent(hoverText);
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
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hoverInfo.ChangeInfoContent(hoverInfo.TextToDisplay);
        Debug.Log($"No longer hovering over {gameObject.name}");
        if (audioSource != null && hoverSound != null)
        {
            if (audioFadeInCoroutine != null) { StopCoroutine(audioFadeInCoroutine); Debug.Log($"determined fade in coroutine was not null, stopping playing sound {hoverSound.name}"); }
            audioFadeOutCoroutine = StartCoroutine(AudioFadeOut.FadeOut(audioSource, 0.3f));
            Debug.Log("Stopping sound");
            audioPlaying = false;
        }
    }
}