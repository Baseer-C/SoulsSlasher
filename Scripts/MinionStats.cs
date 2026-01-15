using UnityEngine;

public class MinionStats : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 30f; // Lower default health for minions
    public float currentHealth;
    public float damage = 5f;     // Minion AI can read this value

    [Header("Status")]
    public bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;

        // Debug check
        // Debug.Log(gameObject.name + " took " + amount + " damage.");

        // --- Optional Hit Animation ---
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            // Plays "Damage" animation instantly if it exists
            anim.Play("Damage", 0, 0f);
        }
        // ------------------------------

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // 1. Disable Collider immediately so player can't hit it again
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 2. Play Death Animation
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        // 3. Destroy the body after a few seconds (cleanup)
        Destroy(gameObject, 3f);
    }
}