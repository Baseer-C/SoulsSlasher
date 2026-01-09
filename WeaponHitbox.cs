using UnityEngine;
using System.Collections.Generic;

public class WeaponHitbox : MonoBehaviour
{
    public float damage = 10f;
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
    }

    public void EnableHitbox()
    {
        hitList.Clear();
        if (weaponCollider != null)
        {
            weaponCollider.enabled = true;
            // Debug.Log(transform.name + " Hitbox ACTIVE looking for: " + targetTag);
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

        // 2. Check Tag
        if (other.CompareTag(targetTag))
        {
            if (!hitList.Contains(other.gameObject))
            {
                // 3. HIT PLAYER LOGIC
                if (targetTag == "Player")
                {
                    CharacterStats playerStats = other.GetComponent<CharacterStats>();
                    if (playerStats != null)
                    {
                        playerStats.TakeDamage(damage);
                        hitList.Add(other.gameObject);
                        Debug.Log("?? BOSS HIT PLAYER for " + damage);
                        return;
                    }
                }

                // 4. HIT ENEMY LOGIC (Boss or Minion)
                if (targetTag == "Enemy")
                {
                    CharacterStats enemyStats = other.GetComponent<CharacterStats>();
                    if (enemyStats != null)
                    {
                        enemyStats.TakeDamage(damage);
                        hitList.Add(other.gameObject);
                        Debug.Log("?? PLAYER HIT BOSS");
                        return;
                    }

                    MinionStats minionStats = other.GetComponent<MinionStats>();
                    if (minionStats != null)
                    {
                        minionStats.TakeDamage(damage);
                        hitList.Add(other.gameObject);
                        Debug.Log("?? PLAYER HIT MINION");
                        return;
                    }
                }
            }
        }
    }
}