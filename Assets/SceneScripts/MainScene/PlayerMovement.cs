using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tutorial from: https://www.youtube.com/watch?v=f473C43s8nE
/// </summary>

public class PlayerMovement : MonoBehaviour
{
    
    [Header("Ground Check")]

    // Ground Checking Variables
    public float playerHeight;
    public float groundDrag;
    public LayerMask whatIsGround;
    bool grounded;


    [Header("Movement")]
    public float movement_speed;
    public Transform orientation;
    float horizontalInput;
    float verticalInput;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float air_multiplier;
    bool readyTOJump;
    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;

    // initialise the move direction vector
    Vector3 moveDirection;

    // rigidbody to apply forces to the player
    Rigidbody rb; 

    public bool frozen = false;

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyTOJump = true;
        Debug.Log("Player Movement Script Loaded");
    }

    private void Update()
    {
        // Ground Check.
        // to perform the ground check, we need to cast a Raycast from the position of the player, down to the ground. if it hits the ground, we are grounded. the length of the raycast is the player height + 0.1f
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.1f, whatIsGround);

        /*
        string grounded_caps_string = grounded.ToString();
        grounded_caps_string = grounded_caps_string.ToUpper();

        // log the horizontal and vertical input
        // Debug.Log("Grounded: " + grounded_caps_string + ". Rigidbody X velocity: " + rb.velocity.x + ". Rigidbody Y velocity: " + rb.velocity.y + ". Rigidbody Z velocity: " + rb.velocity.z);
        */

        if (frozen)
        {
            rb.velocity = Vector3.zero;
        }

        // if movement hasn't changed since last frame, set grounded to False

        if (rb.velocity.x == 0 && rb.velocity.y == 0 && rb.velocity.z == 0)
        {
            grounded = true;
            // this is to prevent the player from becoming stuck
        }
        MyInput();
        Speed_Control();

        // If we are grounded, we want to apply a drag force to the player
        if (grounded && !activeGrapple) {
            rb.drag = groundDrag;
            // Debug.Log("Grounded");
        }
        else {
            rb.drag = 0;
            // Debug.Log("Not Grounded, set drag to 0.1f");
        }

        // Push the player up if they are below the terrain (from: https://discussions.unity.com/t/character-keeps-falling-through-terrain-with-colliders/233186/3)
        int naturalYDistance = 1;
        float playerPositionCalculatedY = this.transform.position.y - Terrain.activeTerrain.SampleHeight(this.transform.position);
        if (playerPositionCalculatedY < naturalYDistance)
        {
            float pushHeight = 1 - playerPositionCalculatedY;
            transform.position += new Vector3(0, pushHeight, 0);
        }
    }

    private bool enableMovementOnNextTouch;
    private bool activeGrapple;
    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        Debug.Log($"[FREEZE_DEBUG-playermovement] Jumping to position: {targetPosition}, {trajectoryHeight}");
        activeGrapple = true;

        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        Debug.Log($"[FREEZE_DEBUG-playermovement] Calculated velocity: {velocityToSet}");

        Invoke(nameof(SetVelocity), 0.1f);

        Invoke(nameof(ResetRestrictions), 3f); // Allow if grappling for more than 3 seconds
    }

    private Vector3 velocityToSet;
    private void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        rb.velocity = velocityToSet;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            ResetRestrictions();

            GetComponent<Grappling>().StopGrapple();
        }
    }

    private void ResetRestrictions()
    {
        activeGrapple = false;
    }

    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity) 
            + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }

    private void Print(string v)
    {
        Debug.Log("[Debug] " + v);
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // Remove all velocity from the y axis
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); // Add the jump force to the player
    }

    private void ResetJump()
    {
        readyTOJump = true;
        Debug.Log("Reset Jump");
    }

    private void Speed_Control()
    {
        if (activeGrapple) return; 
        Vector3 flatVelocity = new Vector3 (rb.velocity.x, 0f, rb.velocity.z);

        // If the player is moving faster than the movement speed, we want to limit the velocity to the movement speed
        if (flatVelocity.magnitude > movement_speed)
        {
            // Don't add a clause to check if the player is grounded, because we want to limit the velocity even if the player is in the air
            Vector3 limitedVelocity = flatVelocity.normalized * movement_speed;
            rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
        }
    }


    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    // Get the input from the player so we can work out where the player wants to move
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Jumping
        if (Input.GetKeyDown(jumpKey) && readyTOJump && grounded)
        {
            readyTOJump = false;
            Invoke(nameof(ResetJump), jumpCooldown);
            Jump();
            //Print("Jumped, added rigidbody velocity of " + rb.velocity + " and added force of " + Vector3.up * jumpForce + "");
        }
    }

    private void MovePlayer()
    {
        if (activeGrapple || frozen) return;
        moveDirection = (orientation.forward * verticalInput) + (orientation.right * horizontalInput); // Get the direction of the player

        if (grounded)
        {
            rb.AddForce(10f * movement_speed * moveDirection.normalized, ForceMode.Force); // Add force to the player in the direction of the player
        }
        else if (!grounded)
        {
            rb.AddForce(10f * air_multiplier * movement_speed * moveDirection.normalized, ForceMode.Force); // Add force to the player in the direction of the player
        }
    }
}
