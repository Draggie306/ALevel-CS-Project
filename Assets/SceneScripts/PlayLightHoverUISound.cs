using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro; 


// Reference: https://discussions.unity.com/t/how-to-attach-hover-event-to-button/149728/5

public class PlayLightHoverUISound : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Doesn't have to be lighthoveruisound, just assign it in component
    public AudioClip hoverSound;
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("No AudioSource found on " + gameObject.name);
        }
    }

    // when the mouse enters the GameObject
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (audioSource != null && hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound);
            Debug.Log("Playing sound");
        }
    }

    // when the mouse exits the GameObject
    public void OnPointerExit(PointerEventData eventData)
    {
        // Nothing is needed yet
    }
}