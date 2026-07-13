using UnityEngine;

/// <summary>
/// Solar panel: continuously charges the vehicle battery.
///   - Output drops drastically during sandstorms (WeatherSystem.SolarMultiplier)
///   - Can break during hazards; broken = zero output, repair with E (base class logic)
/// </summary>
public class SolarPanel : ShipDevice
{
    [Header("Solar")]
    [Tooltip("Energy produced per second in clear weather")]
    [SerializeField] private float energyPerSecond = 1.5f;

    /// <summary>Actual current output per second (for HUD/label).</summary>
    public float CurrentOutput { get; private set; }

    protected override void Tick()
    {
        if (VehicleResources.Instance == null || WeatherSystem.Instance == null)
        {
            return;
        }

        CurrentOutput = energyPerSecond * WeatherSystem.Instance.SolarMultiplier;
        VehicleResources.Instance.ReportProduction(CurrentOutput * Time.deltaTime);
    }

    protected override string GetStatusLine()
    {
        return $"+{CurrentOutput:F1}/s";
    }

    protected override void OnBroken()
    {
        CurrentOutput = 0f;
    }
}
