using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterController : MonoBehaviour
{
    [Header("Water References")]
    public GameObject water; // The parent water block

    [Header("Water Settings")]
    public float waterLevel; // The level of the water
    public float waterRiseSpeed; // The speed at which the water rises
    
    public AudioClip[] waterAudioClips;

    private DialogueController dialogueController;

    void Awake()
    {
        DialogueController dialogueController = FindObjectOfType<DialogueController>();
        
        // Water level should start fron below ground i.e. invisible to the user
        water.transform.position = new Vector3(water.transform.position.x, -2, water.transform.position.z);
    }
    void Start()
    {
        Debug.Log($"[WaterController] WaterController script loaded");
    }

    // Corouting to make the water rise at a given time.
    IEnumerator RiseWater()
    {
        while (water.transform.position.y < waterLevel)
        {
            water.transform.position += new Vector3(0, waterRiseSpeed * Time.deltaTime, 0);
            yield return null;
        }
    }

    // Coroutine to make the water fall.
    IEnumerator FallWater()
    {
        while (water.transform.position.y > 0)
        {
            water.transform.position -= new Vector3(0, waterRiseSpeed * Time.deltaTime, 0);
            yield return null;
        }
    }


    public void ChangeSimulationState(int state)
    {
    
    // There are 6 tokens in the game. Each changes the simulation state of the water.

    // Token 1: Water rises
    // Token 2: Water falls
    // Token 3: Water rises faster
    // Token 4: Water comes in at an angle
    // Token 5: Water freezes
    // Token 6: Water heals the player

    switch (state)
    {
        case 1:
            // Water rises
            dialogueController.ShowImportantText("The water is rising!", null, 4.23f, 2.0f);
            StartCoroutine(RiseWater());
            break;
        case 2:
            // Water falls
            StartCoroutine(FallWater());
            break;
        case 3:
            // Water rises faster
            //StartCoroutine(RiseWaterFaster());
            break;
        case 4:
            // Water comes in at an angle
            //StartCoroutine(RiseWaterAtAngle());
            break;
        case 5:
            // Water freezes
            //StartCoroutine(FreezeWater());
            break;
        case 6:
            // Water heals the player
            //StartCoroutine(HealPlayer());
            break;
        default:
            Debug.Log("Invalid token");
            break;
    }
    }
}
