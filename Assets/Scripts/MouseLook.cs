using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Mouse look. Attach to the Player and make Main Camera a child of the Player (about head height).
///   - Mouse left/right: rotates the whole Player (so W always moves where you face, E interacts toward the crosshair)
///   - Mouse up/down: only pitches the camera
///   - Esc unlocks the cursor, click to re-lock
/// </summary>
public class MouseLook : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform; // empty = auto-find the child Camera
    [SerializeField] private float sensitivity = 0.12f; // mouse sensitivity
    [SerializeField] private float minPitch = -70f;     // max look-down angle
    [SerializeField] private float maxPitch = 80f;      // max look-up angle

    private float pitch;

    private void Start()
    {
        if (cameraTransform == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
            {
                cameraTransform = cam.transform;
            }
        }

        LockCursor(true);
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        Keyboard keyboard = Keyboard.current;

        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            LockCursor(false);
        }
        if (mouse != null && mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
        {
            LockCursor(true);
        }

        if (mouse == null || Cursor.lockState != CursorLockMode.Locked)
        {
            return;
        }

        Vector2 delta = mouse.delta.ReadValue() * sensitivity;

        // Left/right: rotate the whole character
        transform.Rotate(0f, delta.x, 0f);

        // Up/down: camera pitch only
        if (cameraTransform != null)
        {
            pitch = Mathf.Clamp(pitch - delta.y, minPitch, maxPitch);
            cameraTransform.localEulerAngles = new Vector3(pitch, 0f, 0f);
        }
    }

    private void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
