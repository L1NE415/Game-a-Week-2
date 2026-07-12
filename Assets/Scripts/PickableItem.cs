using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PickableItem : MonoBehaviour
{
    public bool IsHeld { get; private set; }

    private Rigidbody rb;
    private Collider[] itemColliders;
    private Collider[] ignoredCarrierColliders;
    private bool originalUseGravity;
    private bool originalIsKinematic;
    private Transform originalParent;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        itemColliders = GetComponentsInChildren<Collider>();
    }

    public bool PickUp(Transform holdPoint, Collider[] carrierColliders)
    {
        if (IsHeld || holdPoint == null)
        {
            return false;
        }

        IsHeld = true;
        originalParent = transform.parent;
        originalUseGravity = rb.useGravity;
        originalIsKinematic = rb.isKinematic;
        ignoredCarrierColliders = carrierColliders;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.isKinematic = true;

        SetCarrierCollisionIgnored(true);

        transform.SetParent(holdPoint);
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        return true;
    }

    public void Drop(Vector3 position, Quaternion rotation)
    {
        Release(position, rotation);
    }

    public void Throw(Vector3 position, Quaternion rotation, Vector3 impulse)
    {
        Release(position, rotation);
        rb.AddForce(impulse, ForceMode.Impulse);
    }

    private void Release(Vector3 position, Quaternion rotation)
    {
        if (!IsHeld)
        {
            return;
        }

        transform.SetParent(originalParent);
        transform.SetPositionAndRotation(position, rotation);

        rb.isKinematic = originalIsKinematic;
        rb.useGravity = originalUseGravity;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        SetCarrierCollisionIgnored(false);
        ignoredCarrierColliders = null;
        IsHeld = false;
    }

    private void SetCarrierCollisionIgnored(bool ignored)
    {
        if (ignoredCarrierColliders == null)
        {
            return;
        }

        foreach (Collider itemCollider in itemColliders)
        {
            if (itemCollider == null)
            {
                continue;
            }

            foreach (Collider carrierCollider in ignoredCarrierColliders)
            {
                if (carrierCollider != null)
                {
                    Physics.IgnoreCollision(itemCollider, carrierCollider, ignored);
                }
            }
        }
    }
}