using UnityEngine;
using UnityEngine.UI;

public class CharacterStats : MonoBehaviour
{
    [Header("Core Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float maxStamina = 100f;
    public float currentStamina;
    public float damage = 20f;

    [Header("Regen")]
    public float staminaRegenRate = 15f;
    public float staminaRegenDelay = 1f;
    private float lastStaminaUseTime;

    [Header("UI")]
    public Image healthBarImage;
    public Image staminaBarImage; // Add a yellow bar to your UI!
    public GameObject deathScreen;

    [Header("Status")]
    public bool isDead;

    private void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        if (deathScreen != null) deathScreen.SetActive(false);
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        // CHECK INVINCIBILITY (Assuming PlayerLocomotion is on the same object)
        PlayerLocomotion locomotion = GetComponent<PlayerLocomotion>();
        if (locomotion != null && locomotion.isInvincible)
        {
            return; // Ignore damage!
        }

        currentHealth -= amount;

        if (healthBarImage != null)
            healthBarImage.fillAmount = currentHealth / maxHealth;

        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            // Only play hurt anim if not a boss (BossAI handles poise)
            if (GetComponent<BossAI>() == null)
            {
                anim.Play("Damage");
            }
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    public void TakeStaminaDamage(float amount)
    {
        currentStamina -= amount;
        lastStaminaUseTime = Time.time;
        if (currentStamina < 0) currentStamina = 0;

        if (staminaBarImage != null)
            staminaBarImage.fillAmount = currentStamina / maxStamina;
    }

    public void RegenerateStamina()
    {
        if (isDead) return;

        if (Time.time > lastStaminaUseTime + staminaRegenDelay)
        {
            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                if (staminaBarImage != null)
                    staminaBarImage.fillAmount = currentStamina / maxStamina;
            }
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        if (healthBarImage != null) healthBarImage.fillAmount = currentHealth / maxHealth;
    }

    // --- ADDED THIS METHOD TO FIX STAT PICKUP ---
    public void IncreaseDamage(float amount)
    {
        damage += amount;
        Debug.Log("Damage Increased to: " + damage);
    }

    private void Die()
    {
        isDead = true;
        if (GetComponent<Animator>() != null) GetComponent<Animator>().SetTrigger("Die");
        if (deathScreen != null) deathScreen.SetActive(true);

        // Disable physics logic
        if (GetComponent<CharacterController>()) GetComponent<CharacterController>().enabled = false;
        if (GetComponent<PlayerManager>()) GetComponent<PlayerManager>().enabled = false;
    }
}