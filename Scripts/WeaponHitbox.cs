using UnityEngine;
using System.Collections.Generic;

public class WeaponHitbox : MonoBehaviour
{
    [Header("Damage Source")]
    [Tooltip("Assign the CharacterStats of the person holding this weapon!")]
    public CharacterStats ownerStats;
    public float damage = 10f; // Fallback damage if ownerStats is null

    [Header("Collider")]
    public Collider weaponCollider;

    [Header("Who to hit?")]
    [Tooltip("If this is the Player's sword, set to 'Enemy'. If this is the Boss's sword, set to 'Player'.")]
    public string targetTag = "Enemy";

    private List<GameObject> hitList = new List<GameObject>();

    void Start()
    {
        if (weaponCollider == null) weaponCollider = GetComponent<Collider>();

        if (weaponCollider != null)
        {
            weaponCollider.isTrigger = true;
            weaponCollider.enabled = false;
        }

        // Auto-find stats in parent if not assigned (Convenience)
        if (ownerStats == null)
        {
            ownerStats = GetComponentInParent<CharacterStats>();
        }
    }

    public void EnableHitbox()
    {
        hitList.Clear();
        if (weaponCollider != null)
        {
            weaponCollider.enabled = true;
            ForceCheckOverlap();
        }
    }

    void ForceCheckOverlap()
    {
        if (weaponCollider is BoxCollider box)
        {
            Collider[] hits = Physics.OverlapBox(box.bounds.center, box.bounds.extents, transform.rotation);
            foreach (Collider hit in hits) ProcessHit(hit);
        }
        else
        {
            Collider[] hits = Physics.OverlapSphere(weaponCollider.bounds.center, 1.0f);
            foreach (Collider hit in hits) ProcessHit(hit);
        }
    }

    public void DisableHitbox()
    {
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        ProcessHit(other);
    }

    void ProcessHit(Collider other)
    {
        // 1. Ignore Self
        if (other.gameObject == gameObject || other.transform.root == transform.root) return;

        // 2. Determine Damage Amount
        float finalDamage = damage;
        if (ownerStats != null)
        {
            finalDamage = ownerStats.damage;
        }

        // 3. Check Tag
        if (other.CompareTag(targetTag))
        {
            if (!hitList.Contains(other.gameObject))
            {
                // 4. HIT PLAYER LOGIC
                if (targetTag == "Player")
                {
                    CharacterStats playerStats = other.GetComponent<CharacterStats>();
                    if (playerStats != null)
                    {
                        playerStats.TakeDamage(finalDamage);
                        hitList.Add(other.gameObject);
                        Debug.Log("?? BOSS HIT PLAYER for " + finalDamage);
                        return;
                    }
                }

                // 5. HIT ENEMY LOGIC (Boss or Minion)
                if (targetTag == "Enemy")
                {
                    CharacterStats enemyStats = other.GetComponent<CharacterStats>();
                    if (enemyStats != null)
                    {
                        enemyStats.TakeDamage(finalDamage);
                        hitList.Add(other.gameObject);
                        Debug.Log("?? PLAYER HIT BOSS for " + finalDamage);
                        return;
                    }

                    MinionStats minionStats = other.GetComponent<MinionStats>();
                    if (minionStats != null)
                    {
                        minionStats.TakeDamage(finalDamage);
                        hitList.Add(other.gameObject);
                        Debug.Log("?? PLAYER HIT MINION for " + finalDamage);
                        return;
                    }
                }
            }
        }
    }
}