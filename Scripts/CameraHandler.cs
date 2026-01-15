using UnityEngine;
using System.Collections.Generic;

public class CameraHandler : MonoBehaviour
{
    [Header("References")]
    public Transform targetTransform;       // The Player
    public Transform cameraPivotTransform;  // The Pivot (Child)
    public Transform cameraTransform;       // The Actual Camera (Grandchild)

    private InputHandler inputHandler;
    private PlayerManager playerManager;
    private LayerMask ignoreLayers;
    private Vector3 cameraFollowVelocity = Vector3.zero;

    [Header("Settings")]
    public float lookSpeed = 0.1f;
    public float followSpeed = 0.1f;
    public float pivotSpeed = 0.03f;

    private float targetPosition;
    private float defaultPosition;
    private float lookAngle;
    private float pivotAngle;
    public float minimumPivot = -35;
    public float maximumPivot = 35;

    [Header("Collision")]
    public float cameraSphereRadius = 0.2f;
    public float cameraCollisionOffSet = 0.2f;
    public float minimumCollisionOffset = 0.2f;

    [Header("Lock On")]
    public float lockOnRadius = 30f;
    public float maximumLockOnDistance = 30f;
    public LayerMask enemyLayer;
    public Transform currentLockOnTarget;
    public Transform nearestLockOnTarget;
    List<CharacterStats> availableTargets = new List<CharacterStats>();

    private void Awake()
    {
        // Auto-find references if setup correctly in hierarchy
        if (FindObjectOfType<PlayerManager>() != null)
        {
            targetTransform = FindObjectOfType<PlayerManager>().transform;
            playerManager = FindObjectOfType<PlayerManager>();
        }

        if (FindObjectOfType<InputHandler>() != null)
        {
            inputHandler = FindObjectOfType<InputHandler>();
        }

        // Robust Hierarchy Check
        if (cameraPivotTransform == null)
        {
            if (transform.childCount > 0)
            {
                cameraPivotTransform = transform.GetChild(0);
                // FIX: Ensure Pivot isn't at feet (0,0,0) if user forgot to move it
                if (cameraPivotTransform.localPosition.y == 0)
                {
                    cameraPivotTransform.localPosition = new Vector3(0, 1.6f, 0); // Default head height
                }
            }
            else
            {
                Debug.LogError("CAMERA ERROR: CameraHandler needs a child GameObject (Pivot)!");
                return; // Stop here to prevent further errors
            }
        }

        if (cameraTransform == null)
        {
            if (cameraPivotTransform.childCount > 0)
            {
                cameraTransform = cameraPivotTransform.GetChild(0);
            }
            else
            {
                Debug.LogError("CAMERA ERROR: CameraPivot needs a child Camera!");
                return;
            }
        }

        defaultPosition = cameraTransform.localPosition.z;
        ignoreLayers = ~(1 << 8 | 1 << 9 | 1 << 10); // Example: Ignore Player/Small Objects layers
    }

    private void Start()
    {
        // Initialize rotations to match current transform to prevent snapping
        lookAngle = transform.eulerAngles.y;

        if (cameraPivotTransform != null)
            pivotAngle = cameraPivotTransform.localEulerAngles.x;
    }

    public void HandleAllCameraMovement()
    {
        if (playerManager == null) return;
        if (targetTransform == null) return; // Safety check

        FollowTarget();
        HandleCameraRotation();
        HandleCameraCollisions();
    }

    private void FollowTarget()
    {
        // Smoothly dampen movement to target
        Vector3 targetPos = Vector3.SmoothDamp(transform.position, targetTransform.position, ref cameraFollowVelocity, followSpeed);
        transform.position = targetPos;
    }

    private void HandleCameraRotation()
    {
        if (inputHandler == null) return;

        if (inputHandler.lockOnFlag && currentLockOnTarget != null)
        {
            // --- LOCKED ON MODE ---
            // Rotate entire Holder towards target
            Vector3 dir = currentLockOnTarget.position - transform.position;
            dir.Normalize();
            dir.y = 0;

            if (dir == Vector3.zero) dir = transform.forward; // Prevent zero vector error

            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookSpeed * Time.deltaTime * 50); // Faster smooth

            // Tilt Pivot slightly down
            dir = currentLockOnTarget.position - cameraPivotTransform.position;
            dir.Normalize();

            if (dir == Vector3.zero) dir = cameraPivotTransform.forward;

            Quaternion targetPivotRotation = Quaternion.LookRotation(dir);
            Vector3 euler = targetPivotRotation.eulerAngles;

            // Optional: Smoothly interpolate pivot angle
            // pivotAngle = Mathf.Lerp(pivotAngle, euler.x, pivotSpeed * Time.deltaTime); 
        }
        else
        {
            // --- FREE LOOK MODE ---
            lookAngle += (inputHandler.mouseX * lookSpeed) / Time.deltaTime;
            pivotAngle -= (inputHandler.mouseY * pivotSpeed) / Time.deltaTime;
            pivotAngle = Mathf.Clamp(pivotAngle, minimumPivot, maximumPivot);

            Vector3 rotation = Vector3.zero;
            rotation.y = lookAngle;
            Quaternion targetRotation = Quaternion.Euler(rotation);
            transform.rotation = targetRotation;

            rotation = Vector3.zero;
            rotation.x = pivotAngle;
            Quaternion targetPivotRotation = Quaternion.Euler(rotation);

            if (cameraPivotTransform != null)
                cameraPivotTransform.localRotation = targetPivotRotation;
        }
    }

    private void HandleCameraCollisions()
    {
        targetPosition = defaultPosition;
        RaycastHit hit;
        Vector3 direction = cameraTransform.position - cameraPivotTransform.position;
        direction.Normalize();

        if (Physics.SphereCast(cameraPivotTransform.position, cameraSphereRadius, direction, out hit, Mathf.Abs(targetPosition), ignoreLayers))
        {
            float dis = Vector3.Distance(cameraPivotTransform.position, hit.point);
            targetPosition = -(dis - cameraCollisionOffSet);
        }

        if (Mathf.Abs(targetPosition) < minimumCollisionOffset)
        {
            targetPosition = -minimumCollisionOffset;
        }

        Vector3 cameraPos = cameraTransform.localPosition;
        cameraPos.z = Mathf.Lerp(cameraTransform.localPosition.z, targetPosition, 0.2f);
        cameraTransform.localPosition = cameraPos;
    }

    public void HandleLockOn()
    {
        // 1. Find Targets
        float shortestDistance = Mathf.Infinity;
        Collider[] colliders = Physics.OverlapSphere(targetTransform.position, lockOnRadius);

        availableTargets.Clear();

        for (int i = 0; i < colliders.Length; i++)
        {
            CharacterStats character = colliders[i].GetComponent<CharacterStats>();

            if (character != null)
            {
                // Check if it's an enemy and not us
                if (character.transform.root != targetTransform.root && !character.isDead)
                {
                    // Check angle (in front of player)
                    Vector3 lockTargetDirection = character.transform.position - targetTransform.position;
                    float distanceFromTarget = Vector3.Distance(targetTransform.position, character.transform.position);
                    float viewableAngle = Vector3.Angle(lockTargetDirection, cameraTransform.forward);

                    if (character.transform.root != targetTransform.transform.root
                        && viewableAngle > -50 && viewableAngle < 50
                        && distanceFromTarget <= maximumLockOnDistance)
                    {
                        availableTargets.Add(character);
                    }
                }
            }
        }

        // 2. Select Nearest
        for (int k = 0; k < availableTargets.Count; k++)
        {
            float distanceFromTarget = Vector3.Distance(targetTransform.position, availableTargets[k].transform.position);

            if (distanceFromTarget < shortestDistance)
            {
                shortestDistance = distanceFromTarget;
                nearestLockOnTarget = availableTargets[k].transform;
            }
        }

        // 3. Toggle
        if (inputHandler.lockOn_Input && !inputHandler.lockOnFlag)
        {
            if (nearestLockOnTarget != null)
            {
                currentLockOnTarget = nearestLockOnTarget;
                inputHandler.lockOnFlag = true;
            }
        }
        else if (inputHandler.lockOn_Input && inputHandler.lockOnFlag)
        {
            inputHandler.lockOnFlag = false;
            currentLockOnTarget = null;
        }
    }
}