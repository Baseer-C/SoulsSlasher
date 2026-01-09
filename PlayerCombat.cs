using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public WeaponHitbox equippedWeapon; // Drag your Sword here

    // Duration needed for hitbox to stay active
    public float damageWindowDuration = 0.4f;
    public float attackDelay = 0.2f; // Windup time before damage starts

    [Header("Rolling Settings")]
    public float rollSpeed = 10f;
    public float rollDuration = 0.5f;
    public float invincibilityDuration = 0.3f;

    // State Flags
    public bool isAttacking = false;
    public bool isRolling = false;
    public bool isInvincible = false;

    private bool nextAttackQueued = false;
    private CharacterController controller;
    private Animator animator;
    private CharacterStats stats;
    private Transform cameraTransform;
    private Coroutine activeAttackRoutine;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        stats = GetComponent<CharacterStats>();
        if (Camera.main != null) cameraTransform = Camera.main.transform;

        // Sync damage from player stats to weapon
        if (equippedWeapon != null && stats != null)
        {
            equippedWeapon.damage = stats.damage;
        }
    }

    void Update()
    {
        if (stats != null && stats.isDead) return;

        // Note: Gravity and Walking are handled by PlayerController.cs
        // We only listen for Combat actions here.

        if (isRolling) return;

        HandleAttackInput();
        HandleRollInput();
    }

    void HandleRollInput()
    {
        // Use Left Shift to Roll
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isRolling && !isAttacking)
        {
            StartCoroutine(RollRoutine());
        }
    }

    void HandleAttackInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isAttacking)
            {
                nextAttackQueued = true;
            }
            else
            {
                activeAttackRoutine = StartCoroutine(AttackRoutine());
            }
        }
    }

    public void CancelAttack()
    {
        if (activeAttackRoutine != null) StopCoroutine(activeAttackRoutine);
        if (equippedWeapon != null) equippedWeapon.DisableHitbox();
        isAttacking = false;
        nextAttackQueued = false;
        animator.ResetTrigger("Attack");
    }

    public bool CanTakeDamage()
    {
        return !isInvincible;
    }

    IEnumerator RollRoutine()
    {
        isRolling = true;
        isInvincible = true;
        animator.SetTrigger("Roll");

        // Calculate roll direction based on input
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 rollDir;
        if (Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f)
        {
            // Roll in the direction we are holding
            float targetAngle = Mathf.Atan2(x, z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
            rollDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }
        else
        {
            // Roll backwards if touching nothing
            rollDir = transform.forward * -1f;
        }

        float timer = 0;
        while (timer < rollDuration)
        {
            // We only apply horizontal movement. 
            // PlayerController.cs will handle the gravity (vertical) automatically.
            controller.Move(rollDir * rollSpeed * Time.deltaTime);

            if (timer > invincibilityDuration) isInvincible = false;
            timer += Time.deltaTime;
            yield return null;
        }

        isInvincible = false;
        isRolling = false;
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        do
        {
            nextAttackQueued = false;
            animator.SetTrigger("Attack");

            yield return new WaitForSeconds(attackDelay);

            if (equippedWeapon != null) equippedWeapon.EnableHitbox();

            yield return new WaitForSeconds(damageWindowDuration);

            if (equippedWeapon != null) equippedWeapon.DisableHitbox();

            // Wait until the current attack animation is actually done
            yield return new WaitUntil(() => !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"));

            // If we queued another click during the animation, the loop repeats immediately
            // (Removed stamina check here)

        } while (nextAttackQueued);

        isAttacking = false;
        nextAttackQueued = false;
    }

    // --- Animation Event Receivers (Empty placeholders to prevent errors if Animation Events exist) ---
    public void FootR() { }
    public void FootL() { }
    public void Land() { }
}