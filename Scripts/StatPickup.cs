using UnityEngine;

public class StatPickup : MonoBehaviour
{
    public enum PickupType { Health, Damage }

    [Header("Pickup Settings")]
    public PickupType type;
    public float amount = 25f; // Heals 25 HP or adds 25 Damage
    public float rotationSpeed = 90f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.5f;

    private Vector3 startPos;

    void Start()
    {
        // Record where it spawned so we can bob up and down relative to that
        startPos = transform.position;
    }

    void Update()
    {
        // 1. Rotate
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

        // 2. Bob Up and Down
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterStats stats = other.GetComponent<CharacterStats>();
            if (stats != null)
            {
                if (type == PickupType.Health)
                {
                    stats.Heal(amount);
                    Debug.Log("?? Health Boosted by " + amount);
                }
                else if (type == PickupType.Damage)
                {
                    stats.IncreaseDamage(amount);
                    Debug.Log("?? Damage Boosted by " + amount);
                }

                // Optional: Spawn a particle effect here if you have one
                Destroy(gameObject);
            }
        }
    }
}