using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Central vehicle resource manager (singleton). Attach to an empty "GameManager" object.
/// Manages: hunger, battery energy, food storage, and total energy production/consumption.
/// Skeleton phase: values only rise/fall and display, no fail state yet.
/// Press F to eat food (held food first, then storage).
/// </summary>
public class VehicleResources : MonoBehaviour
{
    public static VehicleResources Instance { get; private set; }

    [Header("Hunger")]
    [SerializeField] private float maxHunger = 100f;
    [SerializeField] private float hungerDecayPerSecond = 1f;   // hunger lost per second
    [SerializeField] private float hungerPerFood = 25f;         // restored per food from storage
    [SerializeField] private Key eatKey = Key.F;

    [Header("Thirst")]
    [SerializeField] private float maxThirst = 100f;
    [SerializeField] private float thirstDecayPerSecond = 1.5f; // thirst drops faster than hunger

    [Header("Sanity")]
    [SerializeField] private float maxSanity = 100f;
    [SerializeField] private float sanityDecayPerSecond = 0.4f; // drops slowly; sleep to restore

    [Header("Energy")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float startEnergy = 60f;
    [Tooltip("Constant vehicle drain (autopilot/life support). Runs even when devices are off - this is what makes running out of power possible")]
    [SerializeField] private float baseDrainPerSecond = 0.5f;

    public float Hunger { get; private set; }
    public float MaxHunger => maxHunger;
    public float Thirst { get; private set; }
    public float MaxThirst => maxThirst;
    public float Sanity { get; private set; }
    public float MaxSanity => maxSanity;
    public float Energy { get; private set; }
    public float MaxEnergy => maxEnergy;
    public int FoodStored { get; private set; }

    /// <summary>Total production this second (for HUD).</summary>
    public float EnergyProductionPerSecond { get; private set; }
    /// <summary>Total device consumption this second (for HUD).</summary>
    public float EnergyConsumptionPerSecond { get; private set; }
    public float NetEnergyPerSecond => EnergyProductionPerSecond - EnergyConsumptionPerSecond;

    // Accumulated by devices each frame, settled at end of frame
    private float productionThisFrame;
    private float consumptionThisFrame;

    private PlayerCarryController carryController;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Hunger = maxHunger;
        Thirst = maxThirst;
        Sanity = maxSanity;
        Energy = startEnergy;
    }

    private void Start()
    {
        carryController = FindFirstObjectByType<PlayerCarryController>();
    }

    private void Update()
    {
        // Needs drain constantly; GameSession watches for zeros and triggers game over
        Hunger = Mathf.Max(0f, Hunger - hungerDecayPerSecond * Time.deltaTime);
        Thirst = Mathf.Max(0f, Thirst - thirstDecayPerSecond * Time.deltaTime);
        Sanity = Mathf.Max(0f, Sanity - sanityDecayPerSecond * Time.deltaTime);

        // Constant vehicle drain (bypasses TryConsume so the battery CAN reach zero)
        consumptionThisFrame += baseDrainPerSecond * Time.deltaTime;

        // Press F to eat: held food first, then storage
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard[eatKey].wasPressedThisFrame)
        {
            if (!TryEatHeldFood())
            {
                TryEatFood();
            }
        }
    }

    private void LateUpdate()
    {
        // Settle all production/consumption reported by devices this frame
        Energy = Mathf.Clamp(Energy + (productionThisFrame - consumptionThisFrame), 0f, maxEnergy);

        if (Time.deltaTime > 0f)
        {
            EnergyProductionPerSecond = productionThisFrame / Time.deltaTime;
            EnergyConsumptionPerSecond = consumptionThisFrame / Time.deltaTime;
        }

        productionThisFrame = 0f;
        consumptionThisFrame = 0f;
    }

    /// <summary>Called by solar panels each frame with energy produced (already multiplied by deltaTime).</summary>
    public void ReportProduction(float amount)
    {
        productionThisFrame += Mathf.Max(0f, amount);
    }

    /// <summary>Unavoidable drain (cargo weight, etc.). Bypasses the energy check so the battery can hit zero.</summary>
    public void ReportDrain(float amount)
    {
        consumptionThisFrame += Mathf.Max(0f, amount);
    }

    /// <summary>Called by consuming devices each frame. Returns true if there was enough energy.</summary>
    public bool TryConsume(float amount)
    {
        if (Energy < amount)
        {
            return false;
        }
        consumptionThisFrame += Mathf.Max(0f, amount);
        return true;
    }

    /// <summary>Called by the water purifier when the player drinks.</summary>
    public void Drink(float amount)
    {
        Thirst = Mathf.Min(maxThirst, Thirst + amount);
    }

    /// <summary>Called by the bed when the player sleeps.</summary>
    public void RestoreSanity(float amount)
    {
        Sanity = Mathf.Min(maxSanity, Sanity + amount);
    }

    public void AddFood(int count)
    {
        FoodStored += count;
    }

    /// <summary>Eats the FoodItem currently held by the player. Returns false if not holding food.</summary>
    public bool TryEatHeldFood()
    {
        if (carryController == null || carryController.HeldItem == null)
        {
            return false;
        }

        if (!carryController.HeldItem.TryGetComponent(out FoodItem food))
        {
            return false; // held item is not food
        }

        Hunger = Mathf.Min(maxHunger, Hunger + food.HungerRestore);
        carryController.DestroyHeldItem();
        return true;
    }

    public bool TryEatFood()
    {
        if (FoodStored <= 0)
        {
            return false;
        }
        FoodStored--;
        Hunger = Mathf.Min(maxHunger, Hunger + hungerPerFood);
        return true;
    }
}
