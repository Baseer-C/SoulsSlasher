using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    PlayerManager playerManager;

    [Header("Combat Config")]
    public WeaponHitbox weaponScript;
    public string lastAttack;
    public bool isAttacking;

    [Header("Combo Settings")]
    public int comboStep = 0;
    public float comboResetTimer = 2.0f;
    private float currentComboTimer;

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
    }

    public void HandleCombatInput(float delta)
    {
        if (currentComboTimer > 0)
        {
            currentComboTimer -= delta;
            if (currentComboTimer <= 0) comboStep = 0;
        }

        if (playerManager.inputHandler.rb_Input)
        {
            PerformLightAttack();
        }
    }

    private void PerformLightAttack()
    {
        if (playerManager.isInteracting) return;
        if (playerManager.playerStats.currentStamina < 10) return;

        playerManager.isInteracting = true;
        isAttacking = true;

        comboStep++;
        if (comboStep > 3) comboStep = 1; // 1, 2, 1, 2 loop or 1,2,3

        string attackAnim = "Attack" + comboStep; // Ensure "Attack1", "Attack2" exist in Animator
        playerManager.animator.Play(attackAnim);

        // Cost
        playerManager.playerStats.TakeStaminaDamage(20);

        // Reset Timer
        currentComboTimer = comboResetTimer;

        StartCoroutine(AttackCoroutine());
    }

    IEnumerator AttackCoroutine()
    {
        // Wind up
        yield return new WaitForSeconds(0.2f);
        if (weaponScript != null) weaponScript.EnableHitbox();

        // Swing active
        yield return new WaitForSeconds(0.4f);
        if (weaponScript != null) weaponScript.DisableHitbox();

        // Recovery
        yield return new WaitForSeconds(0.2f);
        playerManager.isInteracting = false;
        isAttacking = false;
    }
}