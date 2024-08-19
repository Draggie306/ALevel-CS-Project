using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    public List<AudioClip> MusicObjects = new();
    public Dictionary<string, AudioClip> MusicIDs;
    public AudioSource audioSource;
    public bool IsSilenced = true;

    private string musicID = null;
    void Start()
    {
        Debug.Log($"[MusicController] Starting music controller on {gameObject.name} (should be CanvasRoot)");
        // Read the prefs for the music ID to play.
        string musicID = PlayerPrefs.GetString("MusicID");

        if (musicID == "")
        {
            Debug.Log("[MusicController] No music ID found in PlayerPrefs.");
            musicID = null;
        } else
        {
            Debug.Log($"[MusicController] Found music ID in PlayerPrefs: {musicID}"); 
            TriggerPlay(musicID);
        }
    }

    // Update is called once per frame
    public void OnMusicSelected(string musicID)
    {
        Debug.Log($"[MusicController] Music selected: {musicID}");
        // stop the current music
        audioSource.Stop();

        PlayerPrefs.SetString("MusicID", musicID);
        PlayerPrefs.Save();

        TriggerPlay(musicID);
    }

    // Fades the music to 0, ready to be resumed after.
    public void TriggerPause()
    {
        Debug.Log($"[MusicController] A pause has been triggered for the background track on {gameObject.name}");
        IsSilenced = true;
        StartCoroutine(AudioFadeOut.FadeOut(audioSource, 0.7f, stopAfterFade: true));
        Debug.Log($"[MusicController] IsSilenced is now {IsSilenced}");
        audioSource.volume = 0; // just force it
    }

    public void TriggerPlay(string musicID = null)
    {
        if (musicID == null)
        {
            Debug.Log("[MusicController] No music ID provided, looking in PlayerPrefs");
            musicID = PlayerPrefs.GetString("MusicID");
        }
        Debug.Log($"[MusicController] Triggering play for {musicID}");

        MusicIDs = new() {
            { "001_Chemin",  MusicObjects[0] },
            { "002_EndFN1", MusicObjects[1] },
            { "003_EndFN2",  MusicObjects[2] },
            { "004_Chemin_Techno",  MusicObjects[3] },
            { "005_EndOrchestral",  MusicObjects[4] },
            { "006_Océan",  MusicObjects[5] },
            { "007_Mini_1",  MusicObjects[6] },
            { "008_Mini_2",  MusicObjects[7] },
            { "009_BrawlmasOrchestral",  MusicObjects[8] },
            { "010_athena",  MusicObjects[9] },
            { "011_TFR1", MusicObjects[10] },
            { "012_TFRstorm", MusicObjects[11] },
            { "013_djvix", MusicObjects[12] },
            { "014_waterflametm2", MusicObjects[13] },
            { "015_TFRwfl", MusicObjects[14] },
            { "016_TFRu", MusicObjects[15] },
            { "017_RFRfa", MusicObjects[16] },
            { "018_TFRm", MusicObjects[17] },
            { "019_MDKfb", MusicObjects[18] },
            { "020_brawlloveswamp1", MusicObjects[19] },
            { "021_TheOne", MusicObjects[20] },
            { "022_choristesconcert", MusicObjects[21] },
        };

        // Stop all audio sources
        StopAllAudio();

        audioSource.clip = MusicIDs[musicID];
        Debug.Log($"[MusicController] Assigned audio clip '{musicID}' to {gameObject.name}");
        IsSilenced = false;
        audioSource.loop = true;
        StartCoroutine(AudioFadeIn.FadeIn(audioSource, 0.7f, NBackgroundControllerIsNotResuming: false));
        Debug.Log($"[MusicController] StartCoroutine for {musicID} has finished");
    }

    // Stop all sources function from originally MusicHoverPreview.cs
    private AudioSource[] allAudioSources;
    public GameObject CanvasRoot;

    public void StopAllAudio() { 
        allAudioSources = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
        foreach( AudioSource audioS in allAudioSources) {
            // Do not stop the audio source on the canvas root (this plays background music)
            if (audioS.gameObject == CanvasRoot) { 
                Debug.Log($"[Musiccontroller/StopAllAudio] Skipping audio source {audioS.clip.name} on {audioS.gameObject.name}");
                CanvasRoot.GetComponent<MusicController>().TriggerPause();
                continue; 
                }

            // Else, stop the audio source
            // Debug.Log($"[Musiccontroller/StopAllAudio] Stopping audio source on {audioS.gameObject.name}"); // commented out due to spamming the log
            audioS.Stop();
        }
    }
}
