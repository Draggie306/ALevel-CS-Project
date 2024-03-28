using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueController : MonoBehaviour
{
    [SerializeField]
    private GameObject ImportantTextHud;

    public AudioClip WelcomeVO;
    public AudioClip HowToPlayVO;

    // Start is called before the first frame update
    void Start()
    {
        // When the scene starts, hide it, incase it was enabled in the editor.
        ImportantTextHud.SetActive(false);

        Debug.Log("Ready to show the VO and text.");
    }


    IEnumerator ShowImportantText(string text)
    // Must be IEndum as yield return requires it. https://forum.unity.com/threads/error-cannot-be-an-iterator-block-because-void-is-not-an-iterator-interface-type.144248/
    {
        ImportantTextHud.SetActive(true);
        ImportantTextHud.GetComponent<TextMeshProUGUI>().text = text;

        Debug.Log($"Set ImportantTextHud text to '{text}'");
        yield return new WaitForSeconds(5);
        ImportantTextHud.SetActive(false);
        Debug.Log("Hidden ImportantTextHud after 5 seconds.");
    }
}
