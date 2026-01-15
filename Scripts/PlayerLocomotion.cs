using UnityEngine;
using System.Collections;

public class PlayerLocomotion : MonoBehaviour
{
    PlayerManager playerManager;
    Transform cameraObject;
    Vector3 moveDirection;
    Vector3 yVelocity;

    [Header("Speeds")]
    public float walkingSpeed = 3f;
    public float runningSpeed = 6f;
    public float sprintingSpeed = 9f;
    public float rotationSpeed = 15f;
    public float gravity = -20f;
    public float jumpHeight = 2f;

    [Header("Roll Settings")]
    public float rollSpeed = 10f;
    public float rollDuration = 0.6f;
    public float invincibilityDuration = 0.4f;

    [Header("Stamina Costs")]
    public float sprintCost = 1f;
    public float rollCost = 20f;
    public float jumpCost = 20f;

    [Header("States")]
    public bool isRolling;
    public bool isJumping;
    public bool isInvincible;

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
        cameraObject = Camera.main.transform;
    }

    public void HandleGroundedMovement(float delta)
    {
        if (playerManager.isInteracting) return;

        // --- 1. MOVEMENT CALCULATION ---
        moveDirection = cameraObject.forward * playerManager.inputHandler.vertical;
        moveDirection += cameraObject.right * playerManager.inputHandler.horizontal;
        moveDirection.Normalize();
        moveDirection.y = 0;

        // --- 2. SPEED CALCULATION ---
        float speed = runningSpeed;

        if (playerManager.inputHandler.sprintFlag)
        {
            speed = sprintingSpeed;
            playerManager.isSprinting = true;
            playerManager.playerStats.TakeStaminaDamage(sprintCost * delta);
        }
        else
        {
            playerManager.isSprinting = false;
        }

        Vector3 velocity = moveDirection * speed;
        playerManager.characterController.Move(velocity * delta);
    }

    public void HandleRotation(float delta)
    {
        if (playerManager.isInteracting || !playerManager.canRotate) return;

        // --- LOCKED ON ROTATION (STRAFING) ---
        if (playerManager.inputHandler.lockOnFlag && playerManager.cameraHandler.currentLockOnTarget != null)
        {
            if (playerManager.isSprinting)
            {
                Vector3 targetDir = Vector3.zero;
                targetDir = cameraObject.forward * playerManager.inputHandler.vertical;
                targetDir += cameraObject.right * playerManager.inputHandler.horizontal;
                targetDir.Normalize();
                targetDir.y = 0;

                if (targetDir == Vector3.zero) targetDir = transform.forward;

                Quaternion tr = Quaternion.LookRotation(targetDir);
                Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, rotationSpeed * delta);
                transform.rotation = targetRotation;
            }
            else
            {
                Vector3 rotationDirection = moveDirection;
                rotationDirection = playerManager.cameraHandler.currentLockOnTarget.position - transform.position;
                rotationDirection.y = 0;
                rotationDirection.Normalize();
                Quaternion tr = Quaternion.LookRotation(rotationDirection);
                Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, rotationSpeed * delta);
                transform.rotation = targetRotation;
            }
        }
        // --- FREE LOOK ROTATION ---
        else
        {
            Vector3 targetDir = Vector3.zero;
            targetDir = cameraObject.forward * playerManager.inputHandler.vertical;
            targetDir += cameraObject.right * playerManager.inputHandler.horizontal;
            targetDir.Normalize();
            targetDir.y = 0;

            if (targetDir == Vector3.zero)
                targetDir = transform.forward;

            Quaternion tr = Quaternion.LookRotation(targetDir);
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, rotationSpeed * delta);

            transform.rotation = targetRotation;
        }
    }

    public void HandleRollingAndSprinting(float delta)
    {
        if (playerManager.isInteracting) return;

        if (playerManager.inputHandler.rollFlag)
        {
            if (playerManager.playerStats.currentStamina >= rollCost)
            {
                moveDirection = cameraObject.forward * playerManager.inputHandler.vertical;
                moveDirection += cameraObject.right * playerManager.inputHandler.horizontal;

                if (playerManager.inputHandler.moveAmount > 0)
                {
                    playerManager.animator.SetTrigger("Roll");
                    playerManager.playerStats.TakeStaminaDamage(rollCost);
                    StartCoroutine(PerformRoll(moveDirection));
                }
                else
                {
                    playerManager.animator.SetTrigger("Backstep");
                    playerManager.playerStats.TakeStaminaDamage(rollCost);
                    StartCoroutine(PerformRoll(-transform.forward));
                }
            }
        }
    }

    public void HandleJumping()
    {
        if (playerManager.isInteracting) return;

        // JUMP LOGIC
        if (playerManager.inputHandler.jumpInput)
        {
            // Debug check (uncomment if still broken)
            // Debug.Log("Jump Input Received. Grounded: " + playerManager.characterController.isGrounded);

            if (playerManager.characterController.isGrounded && playerManager.playerStats.currentStamina > 0)
            {
                playerManager.animator.SetTrigger("Jump");

                // Physics Formula: v = sqrt(h * -2 * g)
                // We use a positive target height, and gravity is negative, so -2 * g is positive.
                yVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

                playerManager.playerStats.TakeStaminaDamage(jumpCost);
                isJumping = true;
            }
        }
    }

    public void HandleGravity()
    {
        if (playerManager.characterController.isGrounded)
        {
            if (yVelocity.y < 0)
            {
                // Apply a small constant downward force when grounded to ensure isGrounded stays true
                // -2f is standard, but -5f can be stickier if slopes are an issue
                yVelocity.y = -2f;
            }

            // Only reset jumping flag if we are falling (y < 0) to avoid resetting it immediately on takeoff frame
            if (yVelocity.y < 0 && isJumping)
            {
                isJumping = false;
            }
        }

        // Apply gravity over time
        yVelocity.y += gravity * Time.deltaTime;

        // Move the controller
        playerManager.characterController.Move(yVelocity * Time.deltaTime);

        // Update Manager Grounded State for other scripts to see
        playerManager.isGrounded = playerManager.characterController.isGrounded;
    }

    System.Collections.IEnumerator PerformRoll(Vector3 direction)
    {
        isRolling = true;
        isInvincible = true;
        StartCoroutine(HandleInvincibility());

        float timer = 0;
        Quaternion rollRotation = Quaternion.LookRotation(direction);
        transform.rotation = rollRotation;

        while (timer < rollDuration)
        {
            playerManager.characterController.Move(transform.forward * rollSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        isRolling = false;
    }

    System.Collections.IEnumerator HandleInvincibility()
    {
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    // --- ANIMATION EVENT RECEIVERS ---
    public void FootR() { }
    public void FootL() { }
    public void Land() { }
    public void Hit() { }
}