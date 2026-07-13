using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Runtime HUD. Attach to GameManager; the Canvas is built in code, no manual UI setup.
///   - Top left: hunger, food storage, energy, production/consumption
///   - Top right: weather state + countdown + broken device alerts
///   - Bottom center: [E] prompt for the current interactable
/// Uses Legacy Text (built-in font). Swap to TextMeshPro + a CJK font for localized UI later.
/// </summary>
public class GameHUD : MonoBehaviour
{
    [SerializeField] private PlayerInteractor playerInteractor; // drag the Player here (auto-found if empty)
    [SerializeField] private int fontSize = 32; // tweak in the Inspector for bigger/smaller text

    private Text topLeftText;
    private Text topRightText;
    private Text topCenterText;
    private Text promptText;
    private readonly StringBuilder sb = new StringBuilder();

    private void Start()
    {
        if (playerInteractor == null)
        {
            playerInteractor = FindFirstObjectByType<PlayerInteractor>();
        }
        BuildCanvas();
    }

    private void Update()
    {
        UpdateTopLeft();
        UpdateTopRight();
        UpdateTimer();
        UpdatePrompt();
    }

    private void UpdateTimer()
    {
        float elapsed = GameSession.Instance != null
            ? GameSession.Instance.Elapsed
            : Time.timeSinceLevelLoad;
        topCenterText.text = GameSession.FormatTime(elapsed);
    }

    private void UpdateTopLeft()
    {
        VehicleResources res = VehicleResources.Instance;
        if (res == null)
        {
            topLeftText.text = "No VehicleResources in scene";
            return;
        }

        sb.Clear();
        sb.AppendLine($"HUNGER  {res.Hunger:F0}/{res.MaxHunger:F0}   [F] Eat");
        sb.AppendLine($"THIRST  {res.Thirst:F0}/{res.MaxThirst:F0}");
        sb.AppendLine($"SANITY  {res.Sanity:F0}/{res.MaxSanity:F0}");
        sb.AppendLine($"FOOD    x{res.FoodStored}");
        sb.AppendLine($"ENERGY  {res.Energy:F0}/{res.MaxEnergy:F0}");

        WeightZone zone = WeightZone.Instance;
        if (zone != null)
        {
            string overload = zone.CurrentDrain > 0f
                ? $"  OVERLOADED +{zone.CurrentDrain:F1}/s drain - throw stuff off! [Q]"
                : $"  (free up to {zone.FreeItems})";
            sb.AppendLine($"WEIGHT  {zone.ItemCount} items{overload}");
        }
        topLeftText.text = sb.ToString();
    }

    private void UpdateTopRight()
    {
        WeatherSystem weather = WeatherSystem.Instance;
        if (weather == null)
        {
            topRightText.text = "No WeatherSystem in scene";
            return;
        }

        sb.Clear();
        sb.AppendLine($"{weather.GetWeatherLabel()}  {weather.TimeRemaining:F0}s");

        foreach (string name in weather.GetBrokenDeviceNames())
        {
            sb.AppendLine($"! {name} BROKEN - repair with [E]");
        }
        topRightText.text = sb.ToString();

        // Tint red during hazards
        topRightText.color = weather.Current == WeatherSystem.WeatherType.Clear
            ? Color.white
            : new Color(1f, 0.45f, 0.3f);
    }

    private void UpdatePrompt()
    {
        string prompt = null;
        if (playerInteractor != null && playerInteractor.HasTarget)
        {
            prompt = playerInteractor.CurrentTarget.GetPrompt();
        }
        promptText.text = prompt ?? string.Empty;
    }

    // ---- Runtime UI construction ----

    private void BuildCanvas()
    {
        GameObject canvasGo = new GameObject("HUD Canvas");
        canvasGo.transform.SetParent(transform);
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        topLeftText = CreateText(canvasGo.transform, "TopLeft",
            anchor: new Vector2(0f, 1f), pivot: new Vector2(0f, 1f),
            position: new Vector2(20f, -20f), alignment: TextAnchor.UpperLeft);

        topCenterText = CreateText(canvasGo.transform, "TopCenter",
            anchor: new Vector2(0.5f, 1f), pivot: new Vector2(0.5f, 1f),
            position: new Vector2(0f, -20f), alignment: TextAnchor.UpperCenter);
        topCenterText.fontSize = fontSize + 10;

        topRightText = CreateText(canvasGo.transform, "TopRight",
            anchor: new Vector2(1f, 1f), pivot: new Vector2(1f, 1f),
            position: new Vector2(-20f, -20f), alignment: TextAnchor.UpperRight);

        promptText = CreateText(canvasGo.transform, "Prompt",
            anchor: new Vector2(0.5f, 0f), pivot: new Vector2(0.5f, 0f),
            position: new Vector2(0f, 80f), alignment: TextAnchor.LowerCenter);
        promptText.fontSize = fontSize + 6;
        promptText.color = new Color(1f, 0.9f, 0.4f);
    }

    private Text CreateText(Transform parent, string name,
        Vector2 anchor, Vector2 pivot, Vector2 position, TextAnchor alignment)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);

        Text text = go.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.color = Color.white;

        // Outline keeps text readable on bright backgrounds
        Outline outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        RectTransform rect = text.rectTransform;
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(600f, 200f);
        rect.localScale = Vector3.one;

        return text;
    }
}
