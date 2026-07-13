using UnityEngine;

/// <summary>
/// Spawns a neat grid of cargo items at game start.
/// Attach to an empty child of the vehicle placed where the pile should start
/// (its position = one corner of the grid). Grid extends along local +X / +Z.
/// Spacing must be larger than the item size or physics will pop them apart.
/// </summary>
public class CargoSpawner : MonoBehaviour
{
    [Header("What to spawn")]
    [SerializeField] private PickableItem itemPrefab;
    [Tooltip("Total items = columns x rows (extra layers stack on top if count exceeds one layer)")]
    [SerializeField] private int count = 12;

    [Header("Grid layout")]
    [SerializeField] private int columns = 4;
    [SerializeField] private int rowsPerLayer = 3;
    [Tooltip("Distance between item centers - keep it bigger than the item's collider")]
    [SerializeField] private float spacing = 0.6f;
    [Tooltip("Spawn height above this object (items drop and settle)")]
    [SerializeField] private float dropHeight = 0.3f;

    private void Start()
    {
        if (itemPrefab == null)
        {
            Debug.LogWarning("CargoSpawner: no item prefab assigned", this);
            return;
        }

        for (int i = 0; i < count; i++)
        {
            int layer = i / (columns * rowsPerLayer);
            int indexInLayer = i % (columns * rowsPerLayer);
            int row = indexInLayer / columns;
            int col = indexInLayer % columns;

            Vector3 localPos = new Vector3(
                col * spacing,
                dropHeight + layer * spacing,
                row * spacing);

            Instantiate(itemPrefab,
                transform.TransformPoint(localPos),
                transform.rotation);
        }
    }

    // Draw the grid in the Scene view so you can position it before pressing Play
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        for (int i = 0; i < count; i++)
        {
            int layer = i / (columns * rowsPerLayer);
            int indexInLayer = i % (columns * rowsPerLayer);
            int row = indexInLayer / columns;
            int col = indexInLayer % columns;

            Vector3 localPos = new Vector3(col * spacing, dropHeight + layer * spacing, row * spacing);
            Gizmos.DrawWireCube(transform.TransformPoint(localPos), Vector3.one * spacing * 0.8f);
        }
    }
}
