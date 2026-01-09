using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    [Header("UI References")]
    public Slider slider;
    public Image fillImage;
    public Canvas canvas;

    [Header("Visual Settings")]
    public Color healthyColor = new Color(0.8f, 0.1f, 0.1f); // Dark Red
    public bool hideAtFullHealth = true;
    public float verticalOffset = 2.0f; // Height above head

    [Header("Performance")]
    // Optimization: Don't find Camera.main every frame (it's slow)
    private Camera mainCamera;
    private Transform targetTransform;
    private CharacterStats stats;

    void Start()
    {
        mainCamera = Camera.main;

        // Find the stats script on the parent (The Minion)
        stats = GetComponentInParent<CharacterStats>();
        targetTransform = transform.parent;

        if (stats == null)
        {
            // Fallback if attached incorrectly
            Debug.LogWarning("FloatingHealthBar: Could not find CharacterStats on parent!");
            return;
        }

        // Initialize slider values
        slider.maxValue = stats.maxHealth;
        slider.value = stats.currentHealth;

        // Setup simple Red color
        if (fillImage != null) fillImage.color = healthyColor;
    }

    void LateUpdate()
    {
        // 1. BILLBOARD EFFECT (Look at Camera)
        // We invert the direction so the text/UI isn't mirrored
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                         mainCamera.transform.rotation * Vector3.up);

        // 2. Update Health Value
        if (stats != null)
        {
            slider.maxValue = stats.maxHealth;
            slider.value = stats.currentHealth;

            // 3. Elden Ring Style: Hide if full health or dead
            if (canvas != null && hideAtFullHealth)
            {
                if (stats.currentHealth >= stats.maxHealth || stats.currentHealth <= 0)
                {
                    canvas.enabled = false;
                }
                else
                {
                    canvas.enabled = true;
                }
            }
        }
    }
}