using UnityEngine;

public class FireFlicker : MonoBehaviour
{
    [Header("Settings")]
    public float minIntensity = 2.5f;
    public float maxIntensity = 3.5f;
    [Range(1, 50)]
    public int smoothing = 5;

    // Internal variables
    private Light fireLight;
    private System.Collections.Generic.Queue<float> smoothQueue;
    private float lastSum = 0;

    void Start()
    {
        fireLight = GetComponent<Light>();
        if (fireLight == null)
        {
            // If script is not on the light, check children
            fireLight = GetComponentInChildren<Light>();
        }

        smoothQueue = new System.Collections.Generic.Queue<float>(smoothing);
    }

    void Update()
    {
        if (fireLight == null) return;

        // Pop off an item if too big
        while (smoothQueue.Count >= smoothing)
        {
            lastSum -= smoothQueue.Dequeue();
        }

        // Generate random new item, calculate new average
        float newVal = Random.Range(minIntensity, maxIntensity);
        smoothQueue.Enqueue(newVal);
        lastSum += newVal;

        // Calculate smoothed average
        fireLight.intensity = lastSum / (float)smoothQueue.Count;
    }
}