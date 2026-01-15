using UnityEngine;

public class CameraDrift : MonoBehaviour
{
    public float speed = 0.5f;
    public float distance = 5f;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Moves the camera slowly left and right in a sine wave pattern
        transform.position = startPos + new Vector3(Mathf.Sin(Time.time * speed) * distance, 0, 0);
    }
}