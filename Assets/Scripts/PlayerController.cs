using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using JetBrains.Annotations;
using System.Runtime.CompilerServices;
using System.ComponentModel.Design.Serialization;
using UnityEditor.Experimental.GraphView;

public class PlayerController : MonoBehaviour
{

    //Mana bar
    public float maxMana;
    public float currentMana;
    [SerializeField] AlertBar manabar;

    //Script for seeing Guard Through Wall
    public GameObject guardGameObject;
    private int guardChildren;

    //Public Static Event for collecting crystals
    public static event System.Action collectedAllCrystals;
    public int crystalsCollected;
    public int crystalsTotal;

    //KeyBinds
    public KeyCode jumpKey = KeyCode.Space;

    //Movement
    public float moveSpeed;

    public float groundDrag;

    //Jumping
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    //Ground check
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    //Animator
    public Animator playerAnimator;

    //Inputs
    float horizontalInput;
    float verticalInput;

    //GameOver
    public bool disabled;

    //Misc
    public Transform orientation;
    Vector3 moveDirection;
    Rigidbody rb;


    // Start is called before the first frame update
    void Start()
    {
        //Subscribing Disabled method to if player is spotted or all crystals collected
        GuardScript.OnGuardHasSpottedPlayer += Disabled;
        collectedAllCrystals += Disabled;

        resetJump();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        //Updating our Manabar
        manabar.UpdateAlertBar(currentMana, maxMana);


        //GroundCheck
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        //Drag on ground
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;

        MyInput();
        SpeedControl();
        
        //Static Event Checking for crystals collection
        if (crystalsCollected >= crystalsTotal)
        {
            if (collectedAllCrystals != null)
            {
                collectedAllCrystals();
            }
        }

    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);

        }
       
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    // Update is called once per frame
    private void MyInput()
    {
        

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //Change Guard Layers to visible / INVISIBLE THROUGH WALLS
        if (Input.GetKeyDown(KeyCode.Q))
        {
            var children = guardGameObject.GetComponentsInChildren<Transform>(includeInactive: true);
            foreach (var child in children)
            {
                child.gameObject.layer = LayerMask.NameToLayer("Guard");
            }
        }
        else if (Input.GetKeyUp(KeyCode.Q))
        {
            var children = guardGameObject.GetComponentsInChildren<Transform>(includeInactive: true);
            foreach (var child in children)
            {
                child.gameObject.layer = LayerMask.NameToLayer("GuardNotSeeThrough");
            }
        }
      

        //Jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            jump();

            Invoke(nameof(resetJump), jumpCooldown);
        }
    }

    private void Disabled()
    {
        disabled = true;
    }    

    private void MovePlayer()
    {
        //Checks if movement is disabled
        if (!disabled)
        {
            //Run Animation check
            if (verticalInput != 0f || horizontalInput != 0f)
            {
                playerAnimator.SetBool("IsRunning", true);
            }
            else
            {
                playerAnimator.SetBool("IsRunning", false);
            }

            //Calculate Move Direction
            moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;


            rb.AddForce(moveDirection * moveSpeed * 10f, ForceMode.Force);

            //Grounded

            if (grounded)
            {
                rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
                //Stops animation for jump
                playerAnimator.SetBool("IsJumping", false);
                playerAnimator.SetBool("IsGrounded", true);
            }
            //In air

            else if (!grounded)
                rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }

    public void jump()
    {

        //Checks if movement is disabled
        if (!disabled)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

            //Jump Animation
            playerAnimator.SetBool("IsJumping", true);
            playerAnimator.SetBool("IsGrounded", false);
        }
    }

    private void resetJump()
    {
        readyToJump = true;
    }

    //Unsubscribes disabled method for when scene restarts and destroys object
    private void OnDestroy()
    {
        collectedAllCrystals -= Disabled;
        GuardScript.OnGuardHasSpottedPlayer -= Disabled;
    }
}
