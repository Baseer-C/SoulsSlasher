using UnityEngine;
using UnityEngine.UI; // Needed for UI elements

public class TargetIndicator : MonoBehaviour
{
    [Header("References")]
    public Image targetImage;           // Drag the UI Image here
    public PlayerController playerScript; // Drag your Player Object here

    [Header("Settings")]
    // How high above the enemy pivot point should the dot float?
    // Try 1.5 for humans, maybe 2.0 for taller monsters.
    public Vector3 verticalOffset = new Vector3(0, 1.5f, 0);

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        // Start with the image hidden
        if (targetImage != null)
            targetImage.enabled = false;
    }

    void LateUpdate()
    {
        if (playerScript == null || targetImage == null) return;

        // Check if the player is currently locked onto something
        if (playerScript.isLockedOn && playerScript.currentTarget != null)
        {
            // 1. Enable the image
            targetImage.enabled = true;

            // 2. Find the position above the enemy's head in 3D world space
            Vector3 worldPosition = playerScript.currentTarget.position + verticalOffset;

            // 3. Convert that 3D world position to a 2D screen position
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

            // 4. Check if the target is actually in front of the camera
            // (WorldToScreenPoint returns negative Z if it's behind you)
            if (screenPosition.z > 0)
            {
                // Move the UI image to that position
                targetImage.transform.position = screenPosition;
            }
            else
            {
                // Hide it if it's behind the camera view
                targetImage.enabled = false;
            }
        }
        else
        {
            // If not locked on, hide the image
            targetImage.enabled = false;
        }
    }
}