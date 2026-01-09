using UnityEngine;
using UnityEngine.UI;

public class CharacterStats : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float damage = 10f; // BossAI reads this

    [Header("Status")]
    public bool isDead = false;
    public bool isAttacking = false;

    [Header("UI References")]
    public Image healthBarImage;

    [Header("Win/Lose Screen")]
    public GameObject deathScreen;

    void Start()
    {
        currentHealth = maxHealth;

        if (deathScreen != null)
            deathScreen.SetActive(false);

        UpdateUI();
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        UpdateUI();

        // --- HIT ANIMATION LOGIC (UPDATED) ---
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            // CHECK: Does this character have the BossAI script?
            BossAI boss = GetComponent<BossAI>();

            if (boss != null)
            {
                // IT IS A BOSS: Do NOT play animation here.
                // We want the BossAI script to handle "Stagger" only when Poise breaks.
            }
            else
            {
                // IT IS A MINION OR PLAYER: Play animation immediately on every hit.
                anim.Play("Damage", 0, 0f);
            }
        }
        // -------------------------------------

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void UpdateUI()
    {
        if (healthBarImage != null)
            healthBarImage.fillAmount = currentHealth / maxHealth;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.SetTrigger("Die");

        if (deathScreen != null)
        {
            deathScreen.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (gameObject.CompareTag("Enemy"))
        {
            // Boss/Enemy Death Logic
        }
        else if (gameObject.CompareTag("Player"))
        {
            Debug.Log("Player Died");
            // Disable controls so player stops moving
            // (Make sure your Player script is actually named PlayerController, 
            // otherwise change this line to match your script name!)
            var pc = GetComponent<MonoBehaviour>();
            if (pc != null) pc.enabled = false;
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateUI(); // Don't forget to update the bar when healing!
    }

    public void IncreaseDamage(float amount)
    {
        damage += amount;
    }
}