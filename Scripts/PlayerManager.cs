using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Components")]
    public InputHandler inputHandler;
    public PlayerLocomotion playerLocomotion;
    public PlayerCombat playerCombat;
    public CharacterStats playerStats;
    public Animator animator;
    public CharacterController characterController;

    // Add reference at top
    public CameraHandler cameraHandler;

    [Header("Flags")]
    public bool isInteracting; // Is an animation playing that locks us?
    public bool isSprinting;
    public bool isGrounded;
    public bool canRotate = true;

    private void Awake()
    {
        inputHandler = GetComponent<InputHandler>();
        playerLocomotion = GetComponent<PlayerLocomotion>();
        playerCombat = GetComponent<PlayerCombat>();
        playerStats = GetComponent<CharacterStats>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        cameraHandler = FindObjectOfType<CameraHandler>();
    }

    private void Start()
    {
        // FORCE CURSOR LOCK ON START
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // 1. Check Interaction Status from Animator
        // (Assumes you setup a Bool "isInteracting" in Animator, usually via behavior scripts)
        // For now, we will approximate it using the Combat flag
        isInteracting = playerCombat.isAttacking || playerLocomotion.isRolling;
        canRotate = !playerCombat.isAttacking; // Lock rotation when attacking

        // 2. Handle Stats Regen
        playerStats.RegenerateStamina();

        // 3. Handle Input
        inputHandler.TickInput(Time.deltaTime);

        // 4. Handle Locomotion Actions
        playerLocomotion.HandleRollingAndSprinting(Time.deltaTime);
        playerLocomotion.HandleJumping();

        // 5. Handle Movement
        playerLocomotion.HandleRotation(Time.deltaTime);
        playerLocomotion.HandleGroundedMovement(Time.deltaTime);
        playerLocomotion.HandleGravity();

        // 6. Handle Combat
        playerCombat.HandleCombatInput(Time.deltaTime);

        // 7. Run Camera Logic (Added from previous step)
        if (cameraHandler != null)
        {
            cameraHandler.HandleAllCameraMovement();

            // Check for lock on toggle
            cameraHandler.HandleLockOn();
        }
    }

    private void LateUpdate()
    {
        // Reset One-Frame Inputs
        inputHandler.rollFlag = false;
        inputHandler.jumpInput = false;
        inputHandler.rb_Input = false; // Light Attack
        inputHandler.rt_Input = false; // Heavy Attack

        // RE-LOCK IF CLICKED (Fixes editor focus loss)
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}