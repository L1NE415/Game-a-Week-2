using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Weather/crisis system (singleton). Attach to GameManager.
/// Randomly switches between Clear / Sandstorm / Heatwave:
///   - Sandstorm: solar output multiplied by solarInSandstorm (default 20%)
///   - Heatwave: greenhouse growth multiplied by greenhouseInHeatwave (default 30%)
///   - During any hazard: each second there is a chance a random device breaks,
///     requiring the player to repair it with E
/// </summary>
public class WeatherSystem : MonoBehaviour
{
    public enum WeatherType { Clear, Sandstorm, Heatwave }

    public static WeatherSystem Instance { get; private set; }

    [Header("Durations (seconds)")]
    [SerializeField] private Vector2 clearDuration = new Vector2(20f, 40f);   // clear weather range
    [SerializeField] private Vector2 hazardDuration = new Vector2(10f, 20f);  // hazard range

    [Header("Hazard effects")]
    [Range(0f, 1f)][SerializeField] private float solarInSandstorm = 0.2f;
    [Range(0f, 1f)][SerializeField] private float greenhouseInHeatwave = 0.3f;
    [Tooltip("Chance per second of breaking one device during a hazard")]
    [Range(0f, 1f)][SerializeField] private float breakChancePerSecond = 0.05f;

    public WeatherType Current { get; private set; } = WeatherType.Clear;
    /// <summary>Seconds remaining for the current weather (for HUD).</summary>
    public float TimeRemaining { get; private set; }

    /// <summary>Solar output multiplier.</summary>
    public float SolarMultiplier => Current == WeatherType.Sandstorm ? solarInSandstorm : 1f;
    /// <summary>Greenhouse growth multiplier.</summary>
    public float GreenhouseMultiplier => Current == WeatherType.Heatwave ? greenhouseInHeatwave : 1f;

    // All registered devices (used for random breakage)
    private readonly List<ShipDevice> devices = new List<ShipDevice>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        TimeRemaining = Random.Range(clearDuration.x, clearDuration.y);
    }

    private void Update()
    {
        TimeRemaining -= Time.deltaTime;
        if (TimeRemaining <= 0f)
        {
            SwitchWeather();
        }

        // Randomly break devices during a hazard
        if (Current != WeatherType.Clear && Random.value < breakChancePerSecond * Time.deltaTime)
        {
            BreakRandomDevice();
        }
    }

    private void SwitchWeather()
    {
        if (Current == WeatherType.Clear)
        {
            // Enter a random hazard
            Current = Random.value < 0.5f ? WeatherType.Sandstorm : WeatherType.Heatwave;
            TimeRemaining = Random.Range(hazardDuration.x, hazardDuration.y);
        }
        else
        {
            Current = WeatherType.Clear;
            TimeRemaining = Random.Range(clearDuration.x, clearDuration.y);
        }
    }

    private void BreakRandomDevice()
    {
        List<ShipDevice> working = devices.FindAll(d => d != null && !d.IsBroken);
        if (working.Count == 0)
        {
            return;
        }
        working[Random.Range(0, working.Count)].Break();
    }

    public void RegisterDevice(ShipDevice device)
    {
        if (!devices.Contains(device))
        {
            devices.Add(device);
        }
    }

    public void UnregisterDevice(ShipDevice device)
    {
        devices.Remove(device);
    }

    /// <summary>For HUD: names of all currently broken devices.</summary>
    public IEnumerable<string> GetBrokenDeviceNames()
    {
        foreach (ShipDevice device in devices)
        {
            if (device != null && device.IsBroken)
            {
                yield return device.DeviceName;
            }
        }
    }

    public string GetWeatherLabel()
    {
        switch (Current)
        {
            case WeatherType.Sandstorm: return "SANDSTORM (solar -80%)";
            case WeatherType.Heatwave: return "HEATWAVE (greenhouse -70%)";
            default: return "CLEAR";
        }
    }
}
