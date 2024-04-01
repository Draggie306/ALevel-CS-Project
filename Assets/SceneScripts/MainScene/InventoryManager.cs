using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [Header("Inventory")]
    public GameObject[] InventorySlots;
    public Sprite TokenImage;
    public TextMeshProUGUI AmountCollectedInfoText;

    [Header("Audio")]
    public AudioClip[] PickUpTokenVO;
    public AudioClip[] PickUpTokenSFX;
    public AudioClip[] FinalTokenVOs;
    public AudioClip TenSecondCountdownVO;
    public GameObject CanvasWithDialogueControllerOnIt;
    public static object Instance { get; internal set; }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"Inventory Manager Loaded. Inventory Slots: {InventorySlots.Length}");
    }

    IEnumerator SceneEndSequence()
    {
        StartCoroutine(CanvasWithDialogueControllerOnIt.GetComponent<DialogueController>().ShowImportantText("All tokens have been collected! We're sending the extraction team now, give us a minute...", FinalTokenVOs[0], 6, 1));
        yield return new WaitForSeconds(7);
        StartCoroutine(CanvasWithDialogueControllerOnIt.GetComponent<DialogueController>().ShowImportantText("Initiating countdown: Ten to one, let's reverse the run. Ready to return to where we begun?", FinalTokenVOs[1], 6, 1));
        yield return new WaitForSeconds(1);

        AudioSource audioSource = CanvasWithDialogueControllerOnIt.GetComponent<AudioSource>();
        audioSource.clip = TenSecondCountdownVO;
        audioSource.Play();

        yield return new WaitForSeconds(11);
        EndGame();
    }

    public void OnAllSixCollected()
    {
        // End sequence after the sixth toke, countdown, etc
        Debug.Log("All six tokens collected");
        StartCoroutine(SceneEndSequence());
    }


    private void EndGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("WinScene");
    }

    public void UpdateAmountCollected(int amount)
    {
        AmountCollectedInfoText.SetText($"Collected: {amount}/6");
    }

    public void PickupToken()
    {
        Debug.Log("Pickup token called");
        for (int i = 0; i < InventorySlots.Length; i++)
        {
            Debug.Log($"Iterating through slot {i}");
            // Changing image from: https://forum.unity.com/threads/solved-change-ui-source-image.367215/
            // Check if the image is the defaut image, if it is, change it to the token image then brea
            if (InventorySlots[i].GetComponent<UnityEngine.UI.Image>().sprite.name == "square")
            {
                InventorySlots[i].GetComponent<UnityEngine.UI.Image>().sprite = TokenImage;

                // random colour: https://docs.unity3d.com/ScriptReference/Random.ColorHSV.html
                InventorySlots[i].GetComponent<UnityEngine.UI.Image>().color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                Debug.Log($"Filled slot {i} with token image.");

                // Update the amount collected
                UpdateAmountCollected(i + 1);

                // Play the sound effet. Still want to play it for the 6th token, but not theVO as there is a custom one for that
                // Todo: Implement "You're halfway there" VO for i == 2!
                AudioSource.PlayClipAtPoint(PickUpTokenSFX[Random.Range(0, PickUpTokenSFX.Length)], transform.position, 0.6f);

                if (i == 5)
                {
                    OnAllSixCollected();
                    break;
                }

                // Finally, play the pickup token sound VO
                AudioSource.PlayClipAtPoint(PickUpTokenVO[Random.Range(0, PickUpTokenVO.Length)], transform.position, 3.0f);                

                // Break out of iteration as we have filled the slot and done everything needed
                break;
            } else {
                Debug.Log($"Slot {i} alredy prefilled with token image: {InventorySlots[i].GetComponent<UnityEngine.UI.Image>().sprite.name}");
            }
        }
        Debug.Log("Pickup token called");
    }
}
