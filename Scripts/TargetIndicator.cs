using UnityEngine;
using UnityEngine.UI;

public class TargetIndicator : MonoBehaviour
{
    [Header("References")]
    public Image targetImage;
    public CameraHandler cameraHandler; // REPLACED PlayerController with CameraHandler

    [Header("Settings")]
    public Vector3 verticalOffset = new Vector3(0, 1.5f, 0);

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        if (targetImage != null)
            targetImage.enabled = false;

        // Auto-find the new CameraHandler if not assigned
        if (cameraHandler == null)
        {
            cameraHandler = FindObjectOfType<CameraHandler>();
        }
    }

    void LateUpdate()
    {
        if (cameraHandler == null || targetImage == null) return;

        // NEW CHECK: Look at the CameraHandler's target variable
        if (cameraHandler.currentLockOnTarget != null)
        {
            // 1. Enable the image
            targetImage.enabled = true;

            // 2. Find position above enemy head
            Vector3 worldPosition = cameraHandler.currentLockOnTarget.position + verticalOffset;

            // 3. Convert to Screen Space
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

            // 4. Check if in front of camera
            if (screenPosition.z > 0)
            {
                targetImage.transform.position = screenPosition;
            }
            else
            {
                targetImage.enabled = false;
            }
        }
        else
        {
            // If no target in CameraHandler, hide image
            targetImage.enabled = false;
        }
    }
}