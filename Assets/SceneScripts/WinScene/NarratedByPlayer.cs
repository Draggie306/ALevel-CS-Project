using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarratedByPlayer : MonoBehaviour
{
    public AudioClip NarratedByPlayerAudioClip;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        int TimesBeaten = PlayerPrefs.GetInt("TimesBeaten", 0);
        TimesBeaten++;
        PlayerPrefs.SetInt("TimesBeaten", TimesBeaten);
        StartCoroutine(playAudio());
    }

    IEnumerator playAudio() {
        yield return new WaitForSeconds(1);
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.clip = NarratedByPlayerAudioClip;
        audioSource.Play();
    }
}
