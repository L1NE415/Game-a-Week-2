using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Weight system (singleton). Attach to a child object of the vehicle with a
/// BoxCollider (Is Trigger ON) sized to cover the cargo deck.
/// Every PickableItem inside the box counts as cargo weight; items above the
/// free allowance add extra energy drain. Throw items off the vehicle (Q) to
/// shed weight. Items left far outside the zone are despawned (the car drove away).
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class WeightZone : MonoBehaviour
{
    public static WeightZone Instance { get; private set; }

    [Header("Weight")]
    [Tooltip("This many items ride for free (no extra drain)")]
    [SerializeField] private int freeItems = 3;
    [Tooltip("Extra energy drain per second for each item above the allowance")]
    [SerializeField] private float drainPerItem = 0.3f;

    [Header("Despawn")]
    [Tooltip("Off-board items are destroyed once they leave the camera view by this viewport margin (0.1 = 10% outside the screen edge). Set negative to disable despawning")]
    [SerializeField] private float despawnViewportMargin = 0.15f;

    /// <summary>Items currently on board (for HUD).</summary>
    public int ItemCount { get; private set; }
    /// <summary>Extra drain caused by weight right now (for HUD).</summary>
    public float CurrentDrain { get; private set; }
    public int FreeItems => freeItems;

    private BoxCollider box;
    private readonly HashSet<PickableItem> onBoard = new HashSet<PickableItem>();

    private void Awake()
    {
        Instance = this;
        box = GetComponent<BoxCollider>();
        box.isTrigger = true; // enforce: this volume must never physically collide
    }

    private void Update()
    {
        CountItemsOnBoard();

        // Weight drain bypasses the energy check - dead weight always costs power
        CurrentDrain = Mathf.Max(0, ItemCount - freeItems) * drainPerItem;
        if (VehicleResources.Instance != null && CurrentDrain > 0f)
        {
            VehicleResources.Instance.ReportDrain(CurrentDrain * Time.deltaTime);
        }

        DespawnFarItems();
    }

    private void CountItemsOnBoard()
    {
        onBoard.Clear();

        Collider[] hits = Physics.OverlapBox(
            box.transform.TransformPoint(box.center),
            Vector3.Scale(box.size * 0.5f, box.transform.lossyScale),
            box.transform.rotation,
            ~0,
            QueryTriggerInteraction.Ignore);

        foreach (Collider hit in hits)
        {
            PickableItem item = hit.GetComponentInParent<PickableItem>();
            if (item != null)
            {
                onBoard.Add(item);
            }
        }

        ItemCount = onBoard.Count;
    }

    /// <summary>Whether this item is currently inside the cargo zone (used by WorldScroller).</summary>
    public bool IsOnBoard(PickableItem item)
    {
        return onBoard.Contains(item);
    }

    private void DespawnFarItems()
    {
        if (despawnViewportMargin < 0f)
        {
            return;
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        foreach (PickableItem item in FindObjectsByType<PickableItem>(FindObjectsSortMode.None))
        {
            if (item.IsHeld || onBoard.Contains(item))
            {
                continue;
            }

            // Keep the item as long as the camera can (roughly) see it;
            // destroy once it drifts past the screen edge + margin
            Vector3 vp = cam.WorldToViewportPoint(item.transform.position);
            bool visible = vp.z > 0f
                && vp.x > -despawnViewportMargin && vp.x < 1f + despawnViewportMargin
                && vp.y > -despawnViewportMargin && vp.y < 1f + despawnViewportMargin;

            if (!visible)
            {
                Destroy(item.gameObject); // the car drove away without it
            }
        }
    }
}
