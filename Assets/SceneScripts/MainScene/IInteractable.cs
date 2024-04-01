using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// An interface means that any class that implements this interface must have the methods and properties defined in it.
public interface IInteractable
{
    public string InteractionPrompt { get; }
    public bool Interact(Interactor interactor);
}
