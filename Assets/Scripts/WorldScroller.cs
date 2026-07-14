using UnityEngine;

/// <summary>
/// Central "fake driving" controller (singleton). Attach to GameManager.
/// The car and camera stay still; everything else moves backward at ScrollSpeed.
/// Other scripts read Speed/Direction from here so the whole world stays in sync.
/// Also drags thrown-off PickableItems backward so they recede behind the car
/// (WeightZone then despawns them once they are far enough away).
/// </summary>
public class WorldScroller : MonoBehaviour
{
    public static WorldScroller Instance { get; private set; }

    [Tooltip("How fast the car 'drives' (meters/second)")]
    [SerializeField] private float scrollSpeed = 6f;
    [Tooltip("Direction the WORLD moves (car faces +Z, so the world slides toward -Z)")]
    [SerializeField] private Vector3 direction = new Vector3(0f, 0f, -1f);

    public float Speed => scrollSpeed;
    public Vector3 Direction => direction.normalized;
    /// <summary>World displacement this physics step.</summary>
    public Vector3 StepOffset => Direction * (scrollSpeed * Time.fixedDeltaTime);

    private void Awake()
    {
        Instance = this;
    }

    private void FixedUpdate()
    {
        // Drag items that fell off the car along with the ground
        foreach (PickableItem item in FindObjectsByType<PickableItem>(FindObjectsSortMode.None))
        {
            if (item.IsHeld)
            {
                continue;
            }

            if (WeightZone.Instance != null && WeightZone.Instance.IsOnBoard(item))
            {
                continue; // items on the car deck stay with the car
            }

            if (item.TryGetComponent(out Rigidbody rb))
            {
                rb.position += StepOffset;
            }
        }
    }
}
