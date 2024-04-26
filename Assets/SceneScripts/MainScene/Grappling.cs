using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Grappling gun script. Tutorial: https://www.youtube.com/watch?v=TYzZsBl3OI0
/// </summary>

public class Grappling : MonoBehaviour
{
    [Header("References")]
    private PlayerMovement pm;
    public Transform cameraTransform;
    public Transform gunTip;
    public LayerMask whatIsGrappleable;
    public LineRenderer lr;
    public GameObject reticle;

    [Header("Grapple")]
    public float maxGrappleDistance;
    public float grappleDelayTime;
    public float overshootYAxis;

    private Vector3 grapplePoint;

    [Header("Cooldown")]
    public float grappleCooldown;
    private float grappleCooldownTimer;

    [Header("Input")] // Start the ability to grapple
    public KeyCode grappleKey = KeyCode.Mouse0; // The left mouse button
    private bool isGrappling;



    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Grappling script loaded");
        pm = GetComponent<PlayerMovement>();
    }

    private void LateUpdate()
    {
        // Continually update the start position to where teh gun tip is
        Debug.Log($"late update isGrappling: {isGrappling}");
        if(isGrappling)
        {
            lr.SetPosition(0, gunTip.position);
        }
        else
        {
            lr.SetPosition(0, Vector3.zero);
        }
    }

    
    // Three main functions: StartGrapple, ExecuteGrapple, StopGrapple
    // The idea is that there is a small delay between start and execute, which is when the animation ca be played.
    // Then the player is pulled to the target point.
    // Then the grappling stops and the cooldown is activated.
    private void StartGrapple()
    {
        if (grappleCooldownTimer > 0) { 
            Debug.Log("cooldown timer graeter than 0, returning");
            return;
        }

        isGrappling = true;
        pm.frozen = true;

        // Start from the position, extend forward. Store the info in hit, apply the max distance, and only look for grappleable objects
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, maxGrappleDistance, whatIsGrappleable))
        {
            // f the raycast hits, store the game location point and call execute with a bit of delay
            grapplePoint = hit.point;
            Debug.Log("Grapple point: " + grapplePoint);
            isGrappling = true;
            Invoke("ResetGrappleCooldown", grappleDelayTime);
            lr.enabled = true;
            lr.SetPosition(1, grapplePoint);
            Debug.Log($"Set position 1 to grapple point: {grapplePoint}");
        }
        else
        {
            //
            // if it didn't hit anything, stop the grapple and change the crosshair to match
            Debug.Log("Grapple point not found, stopping grapple.");
            reticle.GetComponent<Image>().color = new Color32(255,255,225,100); //https://discussions.unity.com/t/how-to-change-the-color-of-an-image-with-script/142259
            Invoke(nameof(StopGrapple), 0f);
        }

    }

    private void ExecuteGrapple()
    {
        float distanceFromPoint = Vector3.Distance(transform.position, grapplePoint);
        Debug.Log($"[FREEZE_DEBUG] Distance from point: {distanceFromPoint}");
        pm.frozen = false;

        Vector3 lowestPoint = new Vector3(grapplePoint.x, grapplePoint.y - 1f, grapplePoint.z);
        Debug.Log($"[FREEZE_DEBUG] Lowest point: {lowestPoint}");

        float grapplePointRelativYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativYPos + overshootYAxis;

        Debug.Log($"[FREEZE_DEBUG] Grapple point relative Y position: {grapplePointRelativYPos}, highest point on arc: {highestPointOnArc}");

        if (grapplePointRelativYPos < 0) highestPointOnArc = overshootYAxis;

        Debug.Log($"[FREEZE_DEBUG] Jumping to position: {grapplePoint}, {highestPointOnArc}");
        pm.JumpToPosition(grapplePoint, highestPointOnArc);
        Invoke(nameof(StopGrapple), 1f);
    }

    public void StopGrapple()
    {
        Debug.Log("Called to stop grapple");
        isGrappling = false;
        pm.frozen = false;
        
        grappleCooldownTimer = grappleCooldown;

        // deactivate the line renderer
        lr.enabled = false;
    }




    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(grappleKey)) // && !isGrappling
        {
            Debug.Log("Grapple key pressed");
            //reticle.GetComponent<Image>().color = Color.white;
            StartGrapple();
        }
        else if (isGrappling)
        {
            // immediately execute the if in air, no delay
            Debug.Log("Is grappling");
            ExecuteGrapple();
        }

        if (grappleCooldownTimer > 0)
        {
            // Count down the cooldown timer
            Debug.Log("Cooldown timer greater than 0");
            grappleCooldownTimer -= Time.deltaTime;
            reticle.GetComponent<Image>().color = new Color32(255, 0, 0, 100);
        } else
        {
            // When the cooldown timer is 0, change the crosshair back to green
            reticle.GetComponent<Image>().color = new Color32(0, 255, 0, 100);
        }
    }
}
