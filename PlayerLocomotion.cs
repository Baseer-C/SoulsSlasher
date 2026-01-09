using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(InputHandler))]
public class PlayerLocomotion : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraObject;
    private InputHandler _inputHandler;
    private Rigidbody _rb;
    private Transform _myTransform;

    [Header("Movement Stats")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer; // Set this to Default or Ground in Inspector
    [SerializeField] private float groundCheckRadius = 0.3f; // Size of the check
    [SerializeField] private float groundCheckOffset = 0.5f; // How high up to start the check

    // State Flags
    public bool isGrounded;

    // Physics Variables
    private Vector3 _moveDirection;
    private Vector3 _normalVector;
    private Vector3 _targetPosition;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _inputHandler = GetComponent<InputHandler>();
        _myTransform = transform;

        // Default the normal vector to Up so we don't divide by zero before touching ground
        _normalVector = Vector3.up;

        if (cameraObject == null)
        {
            if (Camera.main != null)
                cameraObject = Camera.main.transform;
            else
                Debug.LogError("No Main Camera found! Please tag your camera as MainCamera.");
        }
    }

    private void Update()
    {
        // 1. Read Input
        float delta = Time.deltaTime;
        _inputHandler.TickInput(delta);

        // 2. Rotate Character (Visuals)
        HandleRotation(delta);

        // Debug: Visualize the Ground Check in Scene View
        Debug.DrawRay(_myTransform.position + Vector3.up * groundCheckOffset, Vector3.down * 1f, Color.red);
    }

    private void FixedUpdate()
    {
        float delta = Time.fixedDeltaTime;

        // 3. Check for Ground
        HandleGroundCheck();

        // 4. Move Rigidbody (Physics)
        HandleMovement(delta);
    }

    private void HandleGroundCheck()
    {
        // Start the ray slightly up inside the player model
        Vector3 origin = _myTransform.position;
        origin.y += groundCheckOffset;

        RaycastHit hit;
        // Shoot a ray down to find the ground
        if (Physics.SphereCast(origin, groundCheckRadius, Vector3.down, out hit, 1.0f, groundLayer))
        {
            isGrounded = true;
            _normalVector = hit.normal;
        }
        else
        {
            isGrounded = false;
            _normalVector = Vector3.up;
        }
    }

    private void HandleMovement(float delta)
    {
        // If we have input
        if (_inputHandler.moveAmount > 0.1f)
        {
            // Calculate direction based on Camera view
            _moveDirection = cameraObject.forward * _inputHandler.vertical;
            _moveDirection += cameraObject.right * _inputHandler.horizontal;
            _moveDirection.Normalize();
            _moveDirection.y = 0;

            // Apply Speed
            float speed = _inputHandler.sprintFlag ? sprintSpeed : walkSpeed;
            _moveDirection *= speed;

            // Project movement on the ground slope so we don't fly off ramps
            Vector3 projectedVelocity = Vector3.ProjectOnPlane(_moveDirection, _normalVector);

            // Apply Velocity
            _rb.linearVelocity = new Vector3(projectedVelocity.x, _rb.linearVelocity.y, projectedVelocity.z);
        }
        else
        {
            // Stop strictly (Snappy movement)
            _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
        }
    }

    private void HandleRotation(float delta)
    {
        Vector3 targetDir = Vector3.zero;

        targetDir = cameraObject.forward * _inputHandler.vertical;
        targetDir += cameraObject.right * _inputHandler.horizontal;
        targetDir.Normalize();
        targetDir.y = 0;

        if (targetDir == Vector3.zero)
            targetDir = _myTransform.forward;

        Quaternion tr = Quaternion.LookRotation(targetDir);
        Quaternion targetRotation = Quaternion.Slerp(_myTransform.rotation, tr, rotationSpeed * delta);

        _myTransform.rotation = targetRotation;
    }
}