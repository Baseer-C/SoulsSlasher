using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public float horizontal;
    public float vertical;
    public float moveAmount;
    public float mouseX;
    public float mouseY;

    [Header("Input Flags")]
    public bool b_Input; // Roll (Tap) / Sprint (Hold) - SHIFT
    public bool jumpInput; // Jump - SPACE (Fixed Name)
    public bool rb_Input; // Light Attack - Left Click
    public bool rt_Input; // Heavy Attack - Right Click

    [Header("Lock On")]
    public bool lockOn_Input; // F Key
    public bool lockOnFlag;

    [Header("State Flags")]
    public bool rollFlag;
    public bool sprintFlag;
    public float b_Input_Timer;

    PlayerManager playerManager;

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
    }

    public void TickInput(float delta)
    {
        MoveInput(delta);
        HandleRollSprintInput(delta);
        HandleJumpInput();
        HandleAttackInput(delta);
        HandleLockOnInput();
    }

    private void MoveInput(float delta)
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        moveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));

        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");

        // Update Animator for movement blending
        if (playerManager.animator != null)
        {
            float snappedValue = 0;
            // 0.5 is Walk, 1 is Run, 2 is Sprint
            if (moveAmount > 0 && moveAmount < 0.55f) snappedValue = 0.5f;
            else if (moveAmount > 0.55f) snappedValue = 1;

            if (sprintFlag) snappedValue = 2;

            // Pass Horizontal/Vertical for Strafing if Locked On
            // Also pass the isLockedOn bool to switch blend trees
            if (playerManager.animator != null)
            {
                playerManager.animator.SetBool("isLockedOn", lockOnFlag);

                if (lockOnFlag)
                {
                    playerManager.animator.SetFloat("Vertical", vertical, 0.1f, delta);
                    playerManager.animator.SetFloat("Horizontal", horizontal, 0.1f, delta);
                }
                else
                {
                    // Pass Speed for Free Movement
                    playerManager.animator.SetFloat("Speed", snappedValue, 0.1f, delta);
                }
            }
        }
    }

    private void HandleRollSprintInput(float delta)
    {
        // SHIFT KEY = Roll / Sprint
        b_Input = Input.GetKey(KeyCode.LeftShift);

        if (b_Input)
        {
            b_Input_Timer += delta;

            // If moving and holding shift -> Sprint
            if (moveAmount > 0.5f && playerManager.playerStats.currentStamina > 0)
            {
                sprintFlag = true;
                playerManager.isSprinting = true;
            }
        }
        else
        {
            if (b_Input_Timer > 0 && b_Input_Timer < 0.5f)
            {
                sprintFlag = false;
                rollFlag = true; // Tap detected -> Roll
            }
            b_Input_Timer = 0;
            playerManager.isSprinting = false;
            sprintFlag = false;
        }
    }

    private void HandleJumpInput()
    {
        // SPACE KEY = Jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpInput = true; // Fixed: consistent variable name
        }
    }

    private void HandleAttackInput(float delta)
    {
        if (Input.GetMouseButtonDown(0)) rb_Input = true;
        if (Input.GetMouseButtonDown(1)) rt_Input = true;
    }

    private void HandleLockOnInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            lockOn_Input = true;
        }
        else
        {
            lockOn_Input = false;
        }
    }
}