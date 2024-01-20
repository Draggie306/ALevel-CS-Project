/*
using UnityEngine;
using System.Collections;

// Taken from https://forum.unity.com/threads/fade-out-audio-source.335031/
public static class AudioFadeOut {
 
    public static IEnumerator FadeOut (AudioSource audioSource, float FadeTime) {
        float startVolume = audioSource.volume;
        Debug.Log($"Fading out {audioSource.name} over {FadeTime} seconds");
 
        while (audioSource.volume > 0) {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;
 
            yield return null;
        }
 
        audioSource.Stop ();
        audioSource.volume = startVolume;
    }
 
}

// Can be used like this:

/*

StartCoroutine (AudioFadeOut.FadeOut (sound_open, 0.1f));
 
//or:
 
public AudioSource Sound1;
 
IEnumerator fadeSound1 = AudioFadeOut.FadeOut (Sound1, 0.5f);
StartCoroutine (fadeSound1);
StopCoroutine (fadeSound1);

*/


// Only one fade in/out coroutine should be running at a time, or the audio will bug out!! 
// I think this is because the coroutine will stop the audio when it's done, and if another coroutine is running, it will stop the audio again, causing it to not play at all.
// This has been commented out because it's not needed anymore, it has been replaced by everything in audiofade.cs