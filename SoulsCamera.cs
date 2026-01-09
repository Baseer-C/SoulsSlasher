using UnityEngine;

public class SoulsCamera : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform playerTransform;       // Auto-finds if left empty
    public PlayerController playerScript;   // Auto-finds if left empty
    public Vector3 targetOffset = new Vector3(0, 1.5f, 0);

    [Header("Distance Settings")]
    public float defaultDistance = 5.0f;
    public float minDistance = 1.0f;
    public float maxDistance = 6.0f;
    public LayerMask collisionLayers;

    [Header("Input Settings")]
    public float xSpeed = 200.0f;
    public float ySpeed = 100.0f;
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    [Header("Lock On Smoothing")]
    public float lockOnDamp = 5.0f;

    // Internal state
    private float x = 0.0f;
    private float y = 0.0f;
    private float currentDistance;

    void Start()
    {
        // 1. Force Cursor Lock
        LockCursor();

        // 2. Auto-Find Player (Fixes camera not moving if variable is empty)
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                playerScript = player.GetComponent<PlayerController>();
            }
            else
            {
                Debug.LogError("CAMERA ERROR: Could not find object with tag 'Player'!");
            }
        }
        else if (playerScript == null)
        {
            playerScript = playerTransform.GetComponent<PlayerController>();
        }

        // Initialize angles
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        currentDistance = defaultDistance;
    }

    void Update()
    {
        // 3. Re-Lock Cursor if user clicks (Fixes mouse floating around in Editor)
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                LockCursor();
            }
        }
    }

    void LateUpdate()
    {
        if (playerTransform == null) return;

        // 4. Check Lock-On State
        bool isLocked = (playerScript != null && playerScript.isLockedOn && playerScript.currentTarget != null);

        if (isLocked)
        {
            // --- LOCKED ON LOGIC ---
            Vector3 dirToEnemy = playerScript.currentTarget.position - playerTransform.position;

            // Safety check for zero vector
            if (dirToEnemy != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(dirToEnemy);
                float targetX = targetRotation.eulerAngles.y;
                float targetY = 20f; // Fixed downward angle

                // Smoothly look at target
                x = Mathf.LerpAngle(x, targetX, Time.deltaTime * lockOnDamp);
                y = Mathf.Lerp(y, targetY, Time.deltaTime * lockOnDamp);
            }
        }
        else
        {
            // --- FREE LOOK LOGIC ---
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
        }

        y = ClampAngle(y, yMinLimit, yMaxLimit);

        // 5. Calculate Rotation & Position
        Quaternion rotation = Quaternion.Euler(y, x, 0);

        Vector3 focusPoint = playerTransform.position + targetOffset;
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -defaultDistance);
        Vector3 desiredPos = rotation * negDistance + focusPoint;

        // Wall Collision
        RaycastHit hit;
        float actualDistance = defaultDistance;

        if (Physics.Linecast(focusPoint, desiredPos, out hit, collisionLayers))
        {
            actualDistance = Vector3.Distance(focusPoint, hit.point) - 0.2f;
            if (actualDistance < minDistance) actualDistance = minDistance;
        }

        Vector3 position = rotation * new Vector3(0.0f, 0.0f, -actualDistance) + focusPoint;

        transform.rotation = rotation;
        transform.position = position;
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F) angle += 360F;
        if (angle > 360F) angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}