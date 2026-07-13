using UnityEngine;

/// <summary>
/// Marks a pickable item as food. Press F while holding it to eat and restore hunger.
/// Attach to the food prefab (requires Rigidbody + PickableItem as well).
/// </summary>
[RequireComponent(typeof(PickableItem))]
public class FoodItem : MonoBehaviour
{
    [Tooltip("How much hunger this restores when eaten")]
    [SerializeField] private float hungerRestore = 25f;

    public float HungerRestore => hungerRestore;
}
