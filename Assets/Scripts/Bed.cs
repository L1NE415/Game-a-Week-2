using UnityEngine;

/// <summary>
/// Bed: press E to take a nap and restore sanity.
///   - Only usable when sanity is below a threshold (no sleep-spamming)
///   - Short cooldown between naps
///   - Can break during hazards like any device; repair with E (base class logic)
/// </summary>
public class Bed : ShipDevice
{
    [Header("Bed")]
    [Tooltip("Sanity restored per nap")]
    [SerializeField] private float sanityPerNap = 50f;
    [Tooltip("Can only sleep when sanity is below this value")]
    [SerializeField] private float sleepThreshold = 70f;
    [Tooltip("Seconds between naps")]
    [SerializeField] private float cooldownSeconds = 10f;

    private float cooldownRemaining;

    protected override void Tick()
    {
        if (cooldownRemaining > 0f)
        {
            cooldownRemaining -= Time.deltaTime;
        }
    }

    protected override string GetStatusLine()
    {
        if (cooldownRemaining > 0f)
        {
            return $"Resting... {cooldownRemaining:F0}s";
        }
        return "Sleep here";
    }

    protected override string GetWorkingPrompt()
    {
        if (cooldownRemaining > 0f)
        {
            return null;
        }

        VehicleResources res = VehicleResources.Instance;
        if (res == null || res.Sanity >= sleepThreshold)
        {
            return null; // not tired enough
        }
        return "[E] Sleep";
    }

    protected override void InteractWhenWorking()
    {
        VehicleResources res = VehicleResources.Instance;
        if (res == null || res.Sanity >= sleepThreshold || cooldownRemaining > 0f)
        {
            return;
        }

        res.RestoreSanity(sanityPerNap);
        cooldownRemaining = cooldownSeconds;
    }
}
