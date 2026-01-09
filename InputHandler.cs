using UnityEngine;

public class InputHandler : MonoBehaviour
{
    [Header("Input Readout")]
    public float horizontal;
    public float vertical;
    public float moveAmount;
    public float mouseX;
    public float mouseY;

    [Header("Flags")]
    public bool sprintFlag;

    // Call this from your main Manager or Update loop
    public void TickInput(float delta)
    {
        MoveInput(delta);
        HandleSprintInput();
    }

    private void MoveInput(float delta)
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");

        // Calculate absolute movement amount for animations (0 to 1)
        moveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));
    }

    private void HandleSprintInput()
    {
        // Holding Left Shift to sprint
        sprintFlag = Input.GetKey(KeyCode.LeftShift) && moveAmount > 0.5f;
    }
}