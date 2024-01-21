using UnityEngine;

// taken from https://salusgames.com/2017-01-08-circle-loading-animation-in-unity/

public class LoadingCircle : MonoBehaviour
{
    private RectTransform rectComponent;
    private float rotateSpeed = -200f;

    private void Start()
    {
        Debug.Log($"[LoadingCircle] Starting on GameObject: {gameObject.name}");
        rectComponent = GetComponent<RectTransform>();
        rectComponent.pivot = new Vector2(0.5f, 0.5f); // MAKE SURE this is centred. Although, this shouyld be selected in the editor
        Debug.Log($"[LoadingCircle] Pivot set to {rectComponent.pivot}");
    }

    private void Update()
    {
        // Debug.Log($"[LoadingCircle] Updating on GameObject: {gameObject.name}");
        rectComponent.RotateAround(rectComponent.position, Vector3.forward, rotateSpeed * Time.deltaTime);
    }
}