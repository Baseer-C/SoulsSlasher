using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

public class BossAI : MonoBehaviour
{
    [Header("UI Settings")]
    public Image healthBar;
    public float showHealthBarDistance = 50f; // Distance to reveal UI

    [Header("Targeting")]
    public Transform target;

    [Header("Poise System")]
    public float maxPoise = 100f;
    public float poiseRegenDelay = 5.0f;
    public float poiseRegenRate = 10f;
    public float staggerDuration = 2.0f;
    public string staggerAnimTrigger = "Hurt";

    [SerializeField] private float currentPoise;
    private float lastPoiseHitTime;

    [Header("Elden Ring Behavior")]
    [Range(0f, 1f)]
    public float aggression = 0.6f;
    public float minStrafeTime = 1.0f;
    public float maxStrafeTime = 3.0f;
    public float strafeDistance = 4.0f;

    [Header("AI Tools")]
    public float detectionRange = 15.0f;
    public float leashDistance = 40.0f;
    public float rotationSpeed = 5.0f;
    public float chaseSpeed = 3.5f;
    public bool showGizmos = true;

    [Header("Combat Settings")]
    public float attackRange = 3.0f;
    public float comboCooldown = 3.0f;
    public float timeBetweenHits = 1.0f;
    public WeaponHitbox bossWeapon;

    [Header("Tactical Retreat")]
    public float retreatDuration = 0.5f;
    public float retreatSpeed = 3.0f;

    [Header("Animation Settings")]
    public string baseAttackName = "Attack";
    public string moveAnimationName = "Blend Tree";
    public string idleAnimationName = "Idle";

    [Header("Timing Tuning")]
    public float attackDelay = 0.5f;
    public float hitboxDuration = 1.0f;
    public float recoveryTime = 1.0f;

    [Range(0.1f, 1f)]
    public float attackAnimSpeed = 0.5f;

    // Components
    private NavMeshAgent agent;
    private Animator animator;
    private CharacterStats stats;

    // State
    private Vector3 startPosition;
    private bool isAttacking = false;
    private bool isReturningHome = false;
    private bool isStrafing = false;
    private bool isStaggered = false;
    private float strafeTimer = 0f;
    private int strafeDirection = 1;
    private float lastAttackTime;
    private int comboStep = 0;

    // UI State
    private GameObject healthBarParent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        stats = GetComponent<CharacterStats>();

        startPosition = transform.position;
        currentPoise = maxPoise;

        if (agent != null) agent.speed = chaseSpeed;

        if (target == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }

        if (bossWeapon != null) bossWeapon.DisableHitbox();

        // --- NEW: UI Setup ---
        if (healthBar != null)
        {
            // Assuming the Image is inside a Panel/Canvas that needs hiding
            // If healthBar is the only thing, we hide it directly. 
            // Better to hide the parent if it has a background.
            if (healthBar.transform.parent != null)
            {
                healthBarParent = healthBar.transform.parent.gameObject;
                healthBarParent.SetActive(false); // Start hidden
            }
            else
            {
                healthBar.enabled = false;
            }
        }
    }

    void Update()
    {
        // 0. Update UI Visibility & Fill
        if (healthBar != null && stats != null && target != null)
        {
            float dist = Vector3.Distance(transform.position, target.position);

            // Check if we should show/hide the bar
            bool shouldShow = dist <= showHealthBarDistance;

            if (healthBarParent != null)
            {
                if (healthBarParent.activeSelf != shouldShow)
                    healthBarParent.SetActive(shouldShow);
            }
            else
            {
                healthBar.enabled = shouldShow;
            }

            // Only update fill if visible to save performance
            if (shouldShow)
            {
                float hpPercent = (float)stats.currentHealth / stats.maxHealth;
                healthBar.fillAmount = hpPercent;
            }
        }

        // 1. Safety Checks
        if (stats == null || stats.isDead || target == null)
        {
            if (agent.enabled && agent.isOnNavMesh) agent.isStopped = true;
            // Hide UI on death
            if (healthBarParent != null) healthBarParent.SetActive(false);
            else if (healthBar != null) healthBar.enabled = false;
            return;
        }
        if (!agent.isOnNavMesh) return;

        // 2. Poise Regeneration Logic
        if (!isStaggered && currentPoise < maxPoise)
        {
            if (Time.time > lastPoiseHitTime + poiseRegenDelay)
            {
                currentPoise += poiseRegenRate * Time.deltaTime;
                if (currentPoise > maxPoise) currentPoise = maxPoise;
            }
        }

        // 3. Stagger State (The "Stun" Lock)
        if (isStaggered)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            return; // Do nothing else while staggered
        }

        // 4. Lock Logic During Attack
        if (isAttacking)
        {
            agent.isStopped = true;
            return;
        }

        // 5. Distance Calculation
        float distToPlayer = Vector3.Distance(transform.position, target.position);
        float distToStart = Vector3.Distance(transform.position, startPosition);

        // --- RETURNING HOME ---
        if (isReturningHome)
        {
            if (distToStart < 1.0f)
            {
                isReturningHome = false;
                stats.currentHealth = stats.maxHealth;
                currentPoise = maxPoise; // Reset poise too
                animator.SetFloat("Speed", 0f);
            }
            else
            {
                agent.isStopped = false;
                agent.speed = chaseSpeed * 1.5f;
                agent.SetDestination(startPosition);
                animator.SetFloat("Speed", agent.speed);
            }
            return;
        }

        // --- LEASH ---
        if (distToStart > leashDistance)
        {
            isReturningHome = true;
            isStrafing = false;
            comboStep = 0;
            return;
        }

        // --- IDLE / DETECTION ---
        if (distToPlayer > detectionRange)
        {
            agent.isStopped = true;
            animator.SetFloat("Speed", 0f);
            return;
        }

        // --- COMBAT LOGIC ---

        if (Time.time - lastAttackTime > 5.0f) comboStep = 0;

        if (isStrafing)
        {
            HandleStrafing(distToPlayer);
            return;
        }

        if (distToPlayer > attackRange)
        {
            agent.isStopped = false;
            agent.speed = chaseSpeed;
            agent.SetDestination(target.position);
            RotateTowards(target.position);
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
        else
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            animator.SetFloat("Speed", 0f);

            bool canAttack = false;
            if (comboStep == 0) canAttack = (Time.time > lastAttackTime + comboCooldown);
            else canAttack = (Time.time > lastAttackTime + timeBetweenHits);

            if (canAttack)
            {
                float roll = Random.value;

                if (roll <= aggression || comboStep > 0)
                {
                    StartCoroutine(PerformBossAttack());
                }
                else
                {
                    StartStrafing();
                }
            }
            else
            {
                RotateTowards(target.position);
            }
        }
    }

    public void TakePoiseDamage(float amount)
    {
        if (isStaggered) return; // Already stunned

        currentPoise -= amount;
        lastPoiseHitTime = Time.time;

        if (currentPoise <= 0)
        {
            StartCoroutine(PerformStagger());
        }
    }

    IEnumerator PerformStagger()
    {
        isStaggered = true;
        isAttacking = false; // Cancel any current attack
        isStrafing = false;

        // Interrupt NavMesh
        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        // Turn off hitbox if mid-swing
        if (bossWeapon != null) bossWeapon.DisableHitbox();

        // Play Animation
        animator.SetTrigger(staggerAnimTrigger);

        // Wait
        yield return new WaitForSeconds(staggerDuration);

        // Reset
        isStaggered = false;
        currentPoise = maxPoise; // Reset meter logic
        comboStep = 0; // Reset combo
    }

    void StartStrafing()
    {
        isStrafing = true;
        strafeTimer = Random.Range(minStrafeTime, maxStrafeTime);
        strafeDirection = (Random.value > 0.5f) ? 1 : -1;
    }

    void HandleStrafing(float distToPlayer)
    {
        strafeTimer -= Time.deltaTime;
        RotateTowards(target.position);

        if (strafeTimer <= 0)
        {
            isStrafing = false;
            StartCoroutine(PerformBossAttack());
        }
        else
        {
            Vector3 offset = transform.right * strafeDirection * strafeDistance;
            agent.isStopped = false;
            agent.speed = chaseSpeed * 0.5f;

            if (agent.isOnNavMesh)
            {
                Vector3 strafeDir = transform.right * strafeDirection;
                if (distToPlayer < attackRange - 1f) strafeDir -= transform.forward;
                agent.SetDestination(transform.position + strafeDir);
                animator.SetFloat("Speed", agent.speed);
            }
        }
    }

    void RotateTowards(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }
    }

    IEnumerator PerformBossAttack()
    {
        isAttacking = true;
        isStrafing = false;

        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        string currentAnim = baseAttackName + (comboStep + 1);
        animator.speed = attackAnimSpeed;
        animator.Play(currentAnim, 0, 0f);

        yield return new WaitForSeconds(attackDelay / attackAnimSpeed);

        if (bossWeapon != null) bossWeapon.EnableHitbox();
        yield return new WaitForSeconds(hitboxDuration / attackAnimSpeed);
        if (bossWeapon != null) bossWeapon.DisableHitbox();

        // Tactical Retreat
        float retreatTimer = 0f;
        while (retreatTimer < retreatDuration)
        {
            if (agent.isOnNavMesh)
            {
                agent.Move(-transform.forward * retreatSpeed * Time.deltaTime);
                RotateTowards(target.position);
            }
            retreatTimer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(recoveryTime / attackAnimSpeed);

        animator.speed = 1f;
        animator.Play(moveAnimationName, 0, 0f);

        lastAttackTime = Time.time;
        comboStep++;
        if (comboStep > 2) comboStep = 0;

        isAttacking = false;
        if (agent.isOnNavMesh) agent.isStopped = false;
    }

    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Vector3 center = Application.isPlaying ? startPosition : transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, leashDistance);
        Gizmos.color = new Color(1, 0.5f, 0);
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Show Health Bar distance
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, showHealthBarDistance);
    }
}