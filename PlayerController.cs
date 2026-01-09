using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Rolling Settings")]
    public float rollSpeed = 10f;
    public float rollDuration = 0.5f;
    public float invincibilityDuration = 0.3f;

    [Header("Combat Settings")]
    public WeaponHitbox weaponScript;
    public float attackWindUp = 0.3f;   // Time before hit
    public float attackDuration = 0.3f; // Time hit is active
    public float attackRecovery = 0.2f; // Time after hit (lowered for combos)
    public float comboResetTime = 1.0f; // Reset combo if you wait this long

    [Header("Lock-On Settings")]
    public float lockOnRadius = 15f;
    public LayerMask enemyLayer;

    // State
    private CharacterController controller;
    private Animator animator;
    private Transform cameraTransform;
    private CharacterStats stats;

    private Vector3 velocity;
    private bool isGrounded;

    // Combat State
    public bool isAttacking = false;
    public bool isRolling = false;
    public bool isInvincible = false;

    // Combo Variables
    private int comboStep = 0;
    private float lastAttackTime = 0;

    // Lock On State
    public bool isLockedOn = false;
    public Transform currentTarget;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        stats = GetComponent<CharacterStats>();
        if (Camera.main != null) cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        if (controller == null || !controller.enabled) return;
        if (stats != null && stats.isDead) return;

        // 1. Lock On Input
        if (Input.GetKeyDown(KeyCode.Q)) HandleLockOnInput();

        // 2. Combat Inputs
        if (!isRolling && !isAttacking)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift)) StartCoroutine(RollRoutine());
            if (Input.GetMouseButtonDown(0)) StartCoroutine(PerformAttack());
        }

        // 3. Movement
        if (isRolling)
        {
            // Rolling handles movement internally
        }
        else if (isAttacking)
        {
            // Stop moving while attacking
            animator.SetFloat("Speed", 0f);
            velocity = Vector3.zero;
        }
        else
        {
            HandleMovement();
            HandleGravityAndJump();
        }

        // 4. Combo Reset Logic
        // If we haven't attacked in a while, reset combo to step 0
        if (Time.time - lastAttackTime > comboResetTime && !isAttacking)
        {
            comboStep = 0;
        }
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;
        velocity = Vector3.zero; // Stop sliding

        // 1. Determine which animation to play
        // This creates string "Attack1", "Attack2", or "Attack3"
        string attackName = "Attack" + (comboStep + 1);

        // 2. Force the Animation (Fixes "Too Close" bug)
        animator.Play(attackName, 0, 0f);

        // 3. Advance Combo Step
        comboStep++;
        if (comboStep > 2) comboStep = 0; // Reset after 3rd attack
        lastAttackTime = Time.time;

        // 4. Combat Timing
        yield return new WaitForSeconds(attackWindUp);

        if (weaponScript != null) weaponScript.EnableHitbox();

        yield return new WaitForSeconds(attackDuration);

        if (weaponScript != null) weaponScript.DisableHitbox();

        yield return new WaitForSeconds(attackRecovery);

        isAttacking = false;
    }

    IEnumerator RollRoutine()
    {
        isRolling = true;
        isInvincible = true;
        animator.SetTrigger("Roll");

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 rollDir;

        if (Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f)
        {
            float targetAngle = Mathf.Atan2(x, z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
            rollDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }
        else
        {
            rollDir = transform.forward * -1f;
        }

        float timer = 0;
        while (timer < rollDuration)
        {
            controller.Move(rollDir * rollSpeed * Time.deltaTime);
            if (!controller.isGrounded) controller.Move(Vector3.down * 9.81f * Time.deltaTime);

            if (timer > invincibilityDuration) isInvincible = false;
            timer += Time.deltaTime;
            yield return null;
        }

        isInvincible = false;
        isRolling = false;
    }

    void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        float inputMagnitude = Vector2.ClampMagnitude(new Vector2(x, z), 1f).magnitude;
        if (animator != null) animator.SetFloat("Speed", inputMagnitude);

        if (cameraTransform == null) return;
        Vector3 moveDir = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0) * new Vector3(x, 0, z);

        if (isLockedOn && currentTarget != null)
        {
            Vector3 dirToEnemy = (currentTarget.position - transform.position).normalized;
            dirToEnemy.y = 0;
            if (dirToEnemy.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dirToEnemy), rotationSpeed * Time.deltaTime);
            }

            if (Vector3.Distance(transform.position, currentTarget.position) > lockOnRadius * 1.5f || !currentTarget.gameObject.activeInHierarchy)
                Unlock();
        }
        else if (moveDir.magnitude >= 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDir), rotationSpeed * Time.deltaTime);
        }

        controller.Move(moveDir.normalized * moveSpeed * Time.deltaTime);
    }

    void HandleGravityAndJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleLockOnInput()
    {
        if (isLockedOn) Unlock(); else FindNearestTarget();
    }

    void FindNearestTarget()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, lockOnRadius, enemyLayer);
        Transform nearest = null;
        float minDistance = Mathf.Infinity;
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Enemy") || (1 << col.gameObject.layer & enemyLayer) != 0)
            {
                float dist = Vector3.Distance(transform.position, col.transform.position);
                if (dist < minDistance) { minDistance = dist; nearest = col.transform; }
            }
        }
        if (nearest != null) { currentTarget = nearest; isLockedOn = true; }
    }

    public void Unlock() { isLockedOn = false; currentTarget = null; }

    // Helper for CharacterStats
    public bool CanTakeDamage() { return !isInvincible; }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lockOnRadius);
    }
    // --- Animation Event Receivers (Empty placeholders to prevent errors if Animation Events exist) ---
    public void FootR() { }
    public void FootL() { }
    public void Land() { }
}