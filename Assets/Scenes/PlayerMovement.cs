using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [Header("Ground Check")]

    // Ground Checking Variables
    public float playerHeight;
    public float groundDrag;
    public LayerMask whatIsGround;
    bool grounded;


    // Movement Variables
    public float movement_speed;
    public Transform orientation;
    float horizontalInput;
    float verticalInput;

    // Jumping Variables
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


        string grounded_caps_string = grounded.ToString();
        grounded_caps_string = grounded_caps_string.ToUpper();

        // log the horizontal and vertical input
        Debug.Log("Grounded: " + grounded_caps_string + ". Rigidbody X velocity: " + rb.velocity.x + ". Rigidbody Y velocity: " + rb.velocity.y + ". Rigidbody Z velocity: " + rb.velocity.z);

        // if movement hasn't changed since last frame, set grounded to False

        if (rb.velocity.x == 0 && rb.velocity.y == 0 && rb.velocity.z == 0)
        {
            grounded = true;
            // this is to prevent the player from becoming stuck
        }
        MyInput();
        Speed_Control();

        // If we are grounded, we want to apply a drag force to the player
        if (grounded)
        {
            rb.drag = groundDrag;
            Debug.Log("Grounded");
        }
        else
        {
            rb.drag = 0.1f;
            Debug.Log("Not Grounded, set drag to 0.1f");
        }
    }

    private void print(string v)
    {
        Debug.Log("[Debug] " + v);
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // Remove all velocity from the y axis
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); // Add the jump force to the player
    }

    private void resetJump()
    {
        readyTOJump = true;
        Debug.Log("Reset Jump");
    }

    private void Speed_Control()
    {
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
            Invoke(nameof(resetJump), jumpCooldown);
            Jump();
            print("Jumped, added rigidbody velocity of " + rb.velocity + " and added force of " + Vector3.up * jumpForce + "");
        }
    }

    private void MovePlayer()
    {
        moveDirection = (orientation.forward * verticalInput) + (orientation.right * horizontalInput); // Get the direction of the player

        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * movement_speed * 10f, ForceMode.Force); // Add force to the player in the direction of the player
        }
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * movement_speed * 10f * air_multiplier, ForceMode.Force); // Add force to the player in the direction of the player
        }
    }
}
