using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCarryController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private Key interactKey = Key.E;
    [SerializeField] private Key throwKey = Key.Q;

    [Header("Carry")]
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Vector3 fallbackHoldOffset = new Vector3(0f, 1.2f, 0.8f);
    [SerializeField] private float pickupDistance = 1.4f;
    [SerializeField] private float pickupRadius = 0.45f;
    [SerializeField] private LayerMask pickableLayers = ~0;

    [Header("Drop / Throw")]
    [SerializeField] private float dropDistance = 1f;
    [SerializeField] private float throwForce = 7f;
    [SerializeField] private float throwUpForce = 2f;

    private PickableItem heldItem;
    private Collider[] playerColliders;
    private Transform runtimeHoldPoint;

    private void Awake()
    {
        playerColliders = GetComponentsInChildren<Collider>();
        runtimeHoldPoint = holdPoint != null ? holdPoint : CreateFallbackHoldPoint();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        if (keyboard[interactKey].wasPressedThisFrame)
        {
            if (heldItem == null)
            {
                TryPickUp();
            }
            else
            {
                DropHeldItem();
            }
        }

        if (keyboard[throwKey].wasPressedThisFrame && heldItem != null)
        {
            ThrowHeldItem();
        }
    }

    private void TryPickUp()
    {
        if (!TryFindPickableItem(out PickableItem item))
        {
            return;
        }

        if (item.PickUp(runtimeHoldPoint, playerColliders))
        {
            heldItem = item;
        }
    }

    private bool TryFindPickableItem(out PickableItem item)
    {
        item = null;
        Vector3 origin = transform.position + Vector3.up * 0.75f;
        Vector3 direction = transform.forward;

        RaycastHit[] hits = Physics.SphereCastAll(
            origin,
            pickupRadius,
            direction,
            pickupDistance,
            pickableLayers,
            QueryTriggerInteraction.Ignore);

        float nearestDistance = float.MaxValue;
        PickableItem lastLoggedItem = null;

        foreach (RaycastHit hit in hits)
        {
            PickableItem candidate = hit.collider.GetComponentInParent<PickableItem>();

            if (candidate == null)
            {
                continue;
            }

            if (candidate != lastLoggedItem)
            {
                Debug.Log($"SphereCast detected collectable item: {candidate.name}", candidate);
                lastLoggedItem = candidate;
            }

            if (candidate.IsHeld || hit.distance >= nearestDistance)
            {
                continue;
            }

            nearestDistance = hit.distance;
            item = candidate;
        }

        return item != null;
    }

    private void DropHeldItem()
    {
        Vector3 dropPosition = GetReleasePosition();
        Quaternion dropRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);

        heldItem.Drop(dropPosition, dropRotation);
        heldItem = null;
    }

    private void ThrowHeldItem()
    {
        Vector3 throwPosition = GetReleasePosition();
        Quaternion throwRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        Vector3 impulse = transform.forward * throwForce + Vector3.up * throwUpForce;

        heldItem.Throw(throwPosition, throwRotation, impulse);
        heldItem = null;
    }

    private Vector3 GetReleasePosition()
    {
        return transform.position + transform.forward * dropDistance + Vector3.up * 0.5f;
    }

    private Transform CreateFallbackHoldPoint()
    {
        GameObject fallback = new GameObject("Runtime Hold Point");
        Transform fallbackTransform = fallback.transform;

        fallbackTransform.SetParent(transform);
        fallbackTransform.SetLocalPositionAndRotation(fallbackHoldOffset, Quaternion.identity);

        return fallbackTransform;
    }
}