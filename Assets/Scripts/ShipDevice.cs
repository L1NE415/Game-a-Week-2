using UnityEngine;

/// <summary>
/// Base class for vehicle devices (Greenhouse and SolarPanel inherit from it).
/// Handles the shared "broken / repair with E" logic:
///   - A broken device stops working and triggers a HUD alert (top right)
///   - The player walks up and presses E; each press repairs 25%, full = fixed
/// Subclasses implement Tick() (per-frame work while healthy) and GetWorkingPrompt().
/// </summary>
public abstract class ShipDevice : MonoBehaviour, IInteractable
{
    [Header("Device")]
    [SerializeField] private string deviceName = "Device";
    [Tooltip("Repair progress gained per E press")]
    [Range(0.05f, 1f)][SerializeField] private float repairPerPress = 0.25f;

    [Header("Overhead label")]
    [SerializeField] private bool showLabel = true;
    [Tooltip("Label height above the device origin")]
    [SerializeField] private float labelHeight = 1.6f;
    [Tooltip("World-space size of the label text")]
    [SerializeField] private float labelTextSize = 0.14f;

    public string DeviceName => deviceName;
    public bool IsBroken { get; private set; }
    /// <summary>Repair progress 0-1, only meaningful while broken.</summary>
    public float RepairProgress { get; private set; }

    protected virtual void Start()
    {
        if (WeatherSystem.Instance != null)
        {
            WeatherSystem.Instance.RegisterDevice(this);
        }

        if (showLabel)
        {
            DeviceLabel.Create(this, labelHeight, labelTextSize);
        }
    }

    protected virtual void OnDestroy()
    {
        if (WeatherSystem.Instance != null)
        {
            WeatherSystem.Instance.UnregisterDevice(this);
        }
    }

    private void Update()
    {
        if (!IsBroken)
        {
            Tick();
        }
    }

    /// <summary>Called by the weather system when this device randomly breaks.</summary>
    public void Break()
    {
        if (IsBroken)
        {
            return;
        }
        IsBroken = true;
        RepairProgress = 0f;
        OnBroken();
    }

    // ---- IInteractable ----

    public string GetPrompt()
    {
        if (IsBroken)
        {
            return $"[E] Repair {deviceName} ({RepairProgress:P0})";
        }
        return GetWorkingPrompt(); // may return null = no interaction available right now
    }

    public void Interact()
    {
        if (IsBroken)
        {
            RepairProgress += repairPerPress;
            if (RepairProgress >= 1f)
            {
                IsBroken = false;
                RepairProgress = 0f;
                OnRepaired();
            }
            return;
        }
        InteractWhenWorking();
    }

    /// <summary>Text shown on the overhead label. Subclasses provide the status line via GetStatusLine().</summary>
    public string GetStatusText()
    {
        if (IsBroken)
        {
            return $"{deviceName}\nBROKEN - repair {RepairProgress:P0}";
        }
        string status = GetStatusLine();
        return status == null ? deviceName : $"{deviceName}\n{status}";
    }

    // ---- Subclass hooks ----

    /// <summary>Second line of the overhead label (growth progress / output rate), null = name only.</summary>
    protected virtual string GetStatusLine() => null;

    /// <summary>Called every frame while the device works normally (produce power / grow food).</summary>
    protected abstract void Tick();

    /// <summary>Interaction prompt while working normally, null = no interaction.</summary>
    protected virtual string GetWorkingPrompt() => null;

    /// <summary>What pressing E does while working normally (e.g. harvest).</summary>
    protected virtual void InteractWhenWorking() { }

    protected virtual void OnBroken() { }
    protected virtual void OnRepaired() { }
}
