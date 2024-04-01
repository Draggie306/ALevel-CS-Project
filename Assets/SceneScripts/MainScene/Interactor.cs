using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Script to interact with tokens. 
/// Tutorial used: https://www.youtube.com/watch?v=THmW4YolDok
/// </summary>

public class Interactor : MonoBehaviour
{

    [SerializeField] private Transform _interactionPoint;
    [SerializeField] private float _interactionPointRadius = 0.5f;
    [SerializeField] private LayerMask _interactableMask;

    [SerializeField] private TextMeshProUGUI _textAreaToDisplayPrompt; 
    private readonly Collider[] _colliders = new Collider[3];
    [SerializeField] private int _numFound;


    // Update is called once per frame
    void Update()
    {
        // position is the position of the interaction point, radius is the radius of the interaction point, colliders is the array of colliders to store the results, mask is the layer mask to filter the results
        // will find everything that is part of the interactable mask
        _numFound = Physics.OverlapSphereNonAlloc(_interactionPoint.position, _interactionPointRadius, _colliders, _interactableMask);

        if (_numFound > 0)
        {
            var interactable = _colliders[0].GetComponent<IInteractable>();

            if (interactable != null)
            {
                Debug.Log("A token is in the area! Press E to pick it up.");
                _textAreaToDisplayPrompt.text = interactable.InteractionPrompt;
                if (Input.GetKeyDown(KeyCode.E))
                {
                    // Now interact with the object, we are the interactor interacting with the interactable
                    interactable.Interact(this);
                    // Destroy the object
                    Destroy(_colliders[0].gameObject);
                    Debug.Log("Token destroyed as pickup successful");
                    _textAreaToDisplayPrompt.text = "You picked up a token!";
                    
                    // Now call the pickup token method from the inventory manager
                    this.GetComponent<InventoryManager>().PickupToken();
                    // display for 2 seconds
                    StartCoroutine(ClearText());
                }
            }
            StartCoroutine(ClearText());    
        }
    }

    // Coroutine to clear the text after 2 seconds
    private IEnumerator ClearText()
    {
        yield return new WaitForSeconds(2);
        _textAreaToDisplayPrompt.text = "";
    }

    // Gizmos show the debug info - here it shows the max radius of the object that can be interacted with
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_interactionPoint.position, _interactionPointRadius);
    }
}
