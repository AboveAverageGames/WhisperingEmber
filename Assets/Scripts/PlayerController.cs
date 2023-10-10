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
using Unity.VisualScripting;

public class PlayerController : MonoBehaviour
{

    //Mana bar
    public bool canCastSpell;
    public bool usingASpell;
    public float maxMana = 100;
    public float currentMana;
    public float manaUseSpeed = 10.0f;
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


        //IF Using a spell reduce the mana
        if (usingASpell)
        {
                currentMana -= Time.deltaTime * manaUseSpeed;
            currentMana = Mathf.Clamp(currentMana,0,maxMana);
            }
        // Checks if you can cast a spell
        if (currentMana > 0)
        {
            canCastSpell = true;
        }


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


        //Checking If can cast
        if (currentMana == 0)
        {
            canCastSpell = false;
        }
        else if (currentMana > 0)
        {
            canCastSpell = true;
        }

        //Change Player Layer and children to INVISIBLE
        if (Input.GetKeyDown(KeyCode.LeftControl) && (currentMana > 0) && (canCastSpell))
        {
            var children = GetComponentsInChildren<Transform>(includeInactive: true);
            foreach (var child in children)
            {
                child.gameObject.layer = LayerMask.NameToLayer("InvisiblePlayer");
            }
            usingASpell = true;
            if (currentMana == 0)
            {
                canCastSpell = false;
            }
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl) || !canCastSpell)
        {
            var children = GetComponentsInChildren<Transform>(includeInactive: true);
            foreach (var child in children)
            {
                child.gameObject.layer = LayerMask.NameToLayer("Player");
            }
            usingASpell = false;
        }



        /*
            //Change Guard Layers to visible / INVISIBLE THROUGH WALLS.. Can cast spell is to stop player holding Q forever
            if (Input.GetKeyDown(KeyCode.Q) && (currentMana > 0) && (canCastSpell))
        {
            var children = guardGameObject.GetComponentsInChildren<Transform>(includeInactive: true);
            foreach (var child in children)
            {
                child.gameObject.layer = LayerMask.NameToLayer("Guard");
            }
            usingASpell = true;
            if (currentMana == 0)
            {
                canCastSpell = false;
            }
        }
        else if (Input.GetKeyUp(KeyCode.Q) || !canCastSpell)
        {
            var children = guardGameObject.GetComponentsInChildren<Transform>(includeInactive: true);
            foreach (var child in children)
            {
                child.gameObject.layer = LayerMask.NameToLayer("GuardNotSeeThrough");
            }
            usingASpell = false;
        }

        THIS HAS BEEN MOVED TO GUARD SCRIPT SO IT APPLIES TO ALL PREFABS
        */

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
