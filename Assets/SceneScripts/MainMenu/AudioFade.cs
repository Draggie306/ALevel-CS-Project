using UnityEngine;
using System.Collections;

/// <summary>
/// Fades the audio source. Used mainly in the main menu.
/// </summary>


// Based on: https://forum.unity.com/threads/fade-out-audio-source.335031/
public static class AudioFadeOut {
    public static IEnumerator FadeOut(AudioSource audioSource, float FadeTime) {
        float startVolume = audioSource.volume;
        //FadeTime = 0.1f; // can't seem to fix bug where rapid spam of hover causes audio to break, so just set fade time to 0.1f and hope for the best
        if (FadeTime <= 0) { FadeTime = 0.1f; } // Prevents divide by zero (or negative)
        Debug.Log($"Fading out {audioSource.name} over {FadeTime} seconds");

        while (audioSource.volume > 0) {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume; // reset volume
    }
}

public static class AudioFadeIn {
    public static IEnumerator FadeIn(AudioSource audioSource, float FadeTime) {
        if (!audioSource.enabled) { audioSource.enabled = true; }
        if (FadeTime <= 0) { FadeTime = 0.1f; } // Prevent divide by zero (or negative)
        Debug.Log($"Fading in {audioSource.name} over {FadeTime} seconds");
        float targetVolume = 1; // THIS WAS THE ISSUE IT WAS targetVolume = audioSource.volume; WHICH MEANT THAT IT WOULD BE SO QUIET WHEN FADE OUT SPAMMED
        audioSource.volume = 0; // reset volume
        audioSource.Play();

        while (audioSource.volume < targetVolume) {
            audioSource.volume += targetVolume * Time.deltaTime / FadeTime;
            yield return null; // Fancy way of saying "wait for the next frame"
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