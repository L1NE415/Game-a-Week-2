using UnityEngine;

/// <summary>
/// Water purifier: consumes energy to fill a water tank; press E to drink.
///   - Filling requires energy (pauses when the battery is empty)
///   - Can break during hazards; broken = no filling, repair with E (base class logic)
/// </summary>
public class WaterPurifier : ShipDevice
{
    [Header("Water")]
    [Tooltip("Seconds to fill the tank from empty (with enough power)")]
    [SerializeField] private float secondsToFillTank = 30f;
    [Tooltip("Energy consumed per second while filling")]
    [SerializeField] private float energyPerSecond = 0.6f;
    [Tooltip("Fraction of the tank one drink uses (0.25 = 4 drinks per full tank)")]
    [Range(0.05f, 1f)][SerializeField] private float tankPerDrink = 0.25f;
    [Tooltip("Thirst restored per drink")]
    [SerializeField] private float thirstPerDrink = 40f;

    /// <summary>Tank level 0-1.</summary>
    public float TankLevel { get; private set; }
    public bool IsStarvedOfPower { get; private set; }

    protected override void Tick()
    {
        IsStarvedOfPower = false;

        if (TankLevel >= 1f)
        {
            return; // tank full
        }

        if (VehicleResources.Instance == null)
        {
            return;
        }

        float energyCost = energyPerSecond * Time.deltaTime;
        if (!VehicleResources.Instance.TryConsume(energyCost))
        {
            IsStarvedOfPower = true;
            return;
        }

        TankLevel = Mathf.Min(1f, TankLevel + Time.deltaTime / secondsToFillTank);
    }

    protected override string GetStatusLine()
    {
        if (IsStarvedOfPower)
        {
            return "NO POWER!";
        }
        return $"Tank {TankLevel:P0}";
    }

    protected override string GetWorkingPrompt()
    {
        if (TankLevel >= tankPerDrink)
        {
            return "[E] Drink";
        }
        return null; // not enough water yet
    }

    protected override void InteractWhenWorking()
    {
        if (TankLevel < tankPerDrink)
        {
            return;
        }
        TankLevel -= tankPerDrink;
        VehicleResources.Instance.Drink(thirstPerDrink);
    }
}
