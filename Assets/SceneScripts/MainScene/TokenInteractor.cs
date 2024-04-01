using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenInteractor : MonoBehaviour, IInteractable
{
    [SerializeField] private string _Prompt = "You're close to a token. Press E to pick it up!";

    public string InteractionPrompt => _Prompt;

    public bool Interact(Interactor interactor)
    {
        Debug.Log("Token picked up");
        return true;
    }

}
