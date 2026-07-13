using UnityEngine;

/// <summary>
/// Greenhouse: consumes energy to grow food; press E to harvest when ready.
///   - Growing requires energy (pauses when the battery is empty)
///   - Heatwave weather slows growth drastically (WeatherSystem.GreenhouseMultiplier)
///   - Can break during hazards; broken = no growth, repair with E (base class logic)
/// Harvesting spawns physical food items that pop out of the greenhouse.
/// </summary>
public class Greenhouse : ShipDevice
{
    [Header("Greenhouse")]
    [Tooltip("Seconds to grow one food (clear weather, enough power)")]
    [SerializeField] private float secondsPerFood = 15f;
    [Tooltip("Energy consumed per second while growing")]
    [SerializeField] private float energyPerSecond = 0.8f;
    [Tooltip("Max food that can pile up waiting to be harvested")]
    [SerializeField] private int maxReadyFood = 3;

    [Header("Harvest drop")]
    [Tooltip("Food prefab spawned on harvest (Rigidbody + PickableItem + FoodItem). Empty = old behavior: straight to storage")]
    [SerializeField] private FoodItem foodPrefab;
    [Tooltip("Where food pops out; empty = above the greenhouse")]
    [SerializeField] private Transform foodSpawnPoint;
    [Tooltip("Upward pop impulse")]
    [SerializeField] private float popForce = 2.5f;

    /// <summary>Growth progress 0-1 of the current food.</summary>
    public float GrowthProgress { get; private set; }
    /// <summary>Food grown and waiting to be harvested.</summary>
    public int ReadyFood { get; private set; }
    /// <summary>True if stalled this frame due to lack of power (for HUD).</summary>
    public bool IsStarvedOfPower { get; private set; }

    protected override void Tick()
    {
        IsStarvedOfPower = false;

        if (ReadyFood >= maxReadyFood)
        {
            return; // full, waiting for the player to harvest
        }

        if (VehicleResources.Instance == null || WeatherSystem.Instance == null)
        {
            return;
        }

        // Consume energy first; stall if there is not enough
        float energyCost = energyPerSecond * Time.deltaTime;
        if (!VehicleResources.Instance.TryConsume(energyCost))
        {
            IsStarvedOfPower = true;
            return;
        }

        // Heatwave slows growth
        float growSpeed = WeatherSystem.Instance.GreenhouseMultiplier / secondsPerFood;
        GrowthProgress += growSpeed * Time.deltaTime;

        if (GrowthProgress >= 1f)
        {
            GrowthProgress = 0f;
            ReadyFood++;
        }
    }

    protected override string GetStatusLine()
    {
        if (IsStarvedOfPower)
        {
            return "NO POWER!";
        }
        string ready = ReadyFood > 0 ? $"  Ready x{ReadyFood}" : "";
        return $"Growing {GrowthProgress:P0}{ready}";
    }

    protected override string GetWorkingPrompt()
    {
        if (ReadyFood > 0)
        {
            return $"[E] Harvest food x{ReadyFood}";
        }
        return null; // no prompt until food is ready
    }

    protected override void InteractWhenWorking()
    {
        if (ReadyFood <= 0)
        {
            return;
        }

        if (foodPrefab == null)
        {
            // No prefab assigned: fall back to adding straight to storage
            VehicleResources.Instance.AddFood(ReadyFood);
            ReadyFood = 0;
            return;
        }

        // Pop food items out one by one
        Vector3 spawnPos = foodSpawnPoint != null
            ? foodSpawnPoint.position
            : transform.position + Vector3.up * 1.2f;

        for (int i = 0; i < ReadyFood; i++)
        {
            FoodItem food = Instantiate(foodPrefab, spawnPos, Random.rotation);

            if (food.TryGetComponent(out Rigidbody rb))
            {
                // Up plus a little random horizontal direction for a nice pop
                Vector3 impulse = Vector3.up * popForce
                    + new Vector3(Random.Range(-0.8f, 0.8f), 0f, Random.Range(-0.8f, 0.8f));
                rb.AddForce(impulse, ForceMode.Impulse);
            }
        }
        ReadyFood = 0;
    }
}
