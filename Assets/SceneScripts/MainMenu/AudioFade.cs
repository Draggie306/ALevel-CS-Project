using UnityEngine;
using System.Collections;

/// <summary>
/// Fades the audio source. Used mainly in the main menu.
/// </summary>


// Based on: https://forum.unity.com/threads/fade-out-audio-source.335031/
public static class AudioFadeOut {
    public static IEnumerator FadeOut(AudioSource audioSource, float FadeTime, bool stopAfterFade = true) {
        float startVolume = audioSource.volume;
        //FadeTime = 0.1f; // can't seem to fix bug where rapid spam of hover causes audio to break, so just set fade time to 0.1f and hope for the best
        if (FadeTime <= 0) { FadeTime = 0.1f; } // Prevents divide by zero (or negative)
        Debug.Log($"[AudioFade/Out] Fading out {audioSource.name} over {FadeTime} seconds");

        while (audioSource.volume > 0) {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;
            if (stopAfterFade) { audioSource.Stop(); Debug.Log($"[AudioFade/Out] (stopAfterFade selection in while) Stopped playing {audioSource.name}"); yield break; }
            else {
                yield return null;
            }
        }

        if (stopAfterFade) { audioSource.Stop(); Debug.Log($"[AudioFade/Out] (stopAfterFade selection) Stopped playing {audioSource.name}"); yield break; }

        audioSource.volume = startVolume; // reset volume
        Debug.Log($"[AudioFade/Out] Reset volume for {audioSource.name} to {startVolume}");
    }
}

public static class AudioFadeIn {
    public static IEnumerator FadeIn(AudioSource audioSource, float FadeTime, bool NBackgroundControllerIsNotResuming = true) {
        if (!audioSource.enabled) { audioSource.enabled = true; }
        if (FadeTime <= 0) { FadeTime = 0.1f; } // Prevent divide by zero (or negative)
        Debug.Log($"[AudioFade/In] Fading in {audioSource.name} over {FadeTime} seconds");
        float targetVolume = 1; // THIS WAS THE ISSUE IT WAS targetVolume = audioSource.volume; WHICH MEANT THAT IT WOULD BE SO QUIET WHEN FADE OUT SPAMMED
        audioSource.volume = 0; // reset volume
        //if (NBackgroundControllerIsNotResuming) { audioSource.Play(); }

        audioSource.Play();



        while (audioSource.volume < targetVolume) {
            audioSource.volume += targetVolume * Time.deltaTime / FadeTime;
            yield return null;
        }
    }
}

// Can be used like this:

/*
StartCoroutine (AudioFadeIn.FadeIn (sound_open, 0.1f));

public AudioSource Sound1;
 
IEnumerator fadeSound1 = AudioFadeIn.FadeIn (Sound1, 0.5f);
StartCoroutine (fadeSound1);

*/