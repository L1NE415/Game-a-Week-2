using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [Tooltip("How fast the character turns to face the move direction (degrees/second)")]
    [SerializeField] private float turnSpeed = 720f;
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float groundCheckDistance = 0.08f;
    [SerializeField] private LayerMask groundLayers = ~0;

    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private bool jumpQueued;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpQueued = true;
        }
    }

    private void FixedUpdate()
    {
        Vector2 moveInput = ReadMoveInput();
        // Fixed top-down camera: move along world axes (W = up on screen),
        // and turn the character to face wherever it is moving (Overcooked style)
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        Vector3 velocity = rb.linearVelocity;
        velocity.x = moveDirection.x * moveSpeed;
        velocity.z = moveDirection.z * moveSpeed;
        rb.linearVelocity = velocity;

        // Rotate toward the move direction (interaction/pickup raycasts use transform.forward)
        if (moveDirection.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
        }

        if (jumpQueued && IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        jumpQueued = false;
    }

    private Vector2 ReadMoveInput()
    {
        if (Keyboard.current == null)
        {
            return Vector2.zero;
        }

        Vector2 input = Vector2.zero;

        if (Keyboard.current.aKey.isPressed)
        {
            input.x -= 1f;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            input.x += 1f;
        }

        if (Keyboard.current.sKey.isPressed)
        {
            input.y -= 1f;
        }

        if (Keyboard.current.wKey.isPressed)
        {
            input.y += 1f;
        }

        return input;
    }

    private bool IsGrounded()
    {
        Bounds bounds = capsuleCollider.bounds;
        float radius = Mathf.Max(0.01f, Mathf.Min(bounds.extents.x, bounds.extents.z) * 0.95f);
        Vector3 origin = new Vector3(bounds.center.x, bounds.min.y + radius + 0.01f, bounds.center.z);

        RaycastHit[] hits = Physics.SphereCastAll(
            origin,
            radius,
            Vector3.down,
            groundCheckDistance + 0.02f,
            groundLayers,
            QueryTriggerInteraction.Ignore);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider != capsuleCollider && !hit.collider.transform.IsChildOf(transform))
            {
                return true;
            }
        }

        return false;
    }
}