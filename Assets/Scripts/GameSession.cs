using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Run timer + game over flow. Attach to GameManager.
///   - Counts survival time (GameHUD shows it top-center)
///   - Hunger reaching 0 = game over: freezes the game, shows an end screen
///     with this run's time and the history of previous runs
///   - Press R on the end screen to restart (reloads the scene)
/// Run history is kept in a static list, so it survives scene reloads
/// (cleared only when the application quits).
/// </summary>
public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    // Static = survives scene reloads within one play session
    private static readonly List<float> runHistory = new List<float>();

    [SerializeField] private Key restartKey = Key.R;
    [Tooltip("How many past runs to list on the end screen")]
    [SerializeField] private int maxHistoryShown = 5;

    public float Elapsed { get; private set; }
    public bool IsGameOver { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Time.timeScale = 1f; // safety: in case we reloaded from a frozen game over
    }

    private void Update()
    {
        if (!IsGameOver)
        {
            Elapsed += Time.deltaTime;

            VehicleResources res = VehicleResources.Instance;
            if (res != null)
            {
                if (res.Thirst <= 0f)
                {
                    TriggerGameOver("YOU DIED OF THIRST");
                }
                else if (res.Hunger <= 0f)
                {
                    TriggerGameOver("YOU STARVED");
                }
                else if (res.Energy <= 0f)
                {
                    TriggerGameOver("POWER FAILURE - LIFE SUPPORT DOWN");
                }
            }
            return;
        }

        // Game over: wait for restart key
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard[restartKey].wasPressedThisFrame)
        {
            Restart();
        }
    }

    public static string FormatTime(float seconds)
    {
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        return $"{m:00}:{s:00}";
    }

    private void TriggerGameOver(string cause)
    {
        IsGameOver = true;
        runHistory.Add(Elapsed);
        Time.timeScale = 0f; // freeze gameplay
        BuildGameOverScreen(cause);
    }

    private void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ---- Runtime end screen ----

    private void BuildGameOverScreen(string cause)
    {
        GameObject canvasGo = new GameObject("GameOver Canvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // draw above the HUD
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        // Dark full-screen backdrop
        GameObject dim = new GameObject("Dim");
        dim.transform.SetParent(canvasGo.transform);
        Image dimImage = dim.AddComponent<Image>();
        dimImage.color = new Color(0f, 0f, 0f, 0.75f);
        RectTransform dimRect = dimImage.rectTransform;
        dimRect.anchorMin = Vector2.zero;
        dimRect.anchorMax = Vector2.one;
        dimRect.offsetMin = Vector2.zero;
        dimRect.offsetMax = Vector2.zero;
        dimRect.localScale = Vector3.one;

        // Text block
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"GAME OVER - {cause}");
        sb.AppendLine();
        sb.AppendLine($"Survived  {FormatTime(Elapsed)}");

        float best = float.MinValue;
        foreach (float t in runHistory)
        {
            best = Mathf.Max(best, t);
        }
        sb.AppendLine($"Best      {FormatTime(best)}{(Mathf.Approximately(best, Elapsed) ? "  (new best!)" : "")}");
        sb.AppendLine();
        sb.AppendLine("Previous runs:");

        int shown = 0;
        for (int i = runHistory.Count - 1; i >= 0 && shown < maxHistoryShown; i--, shown++)
        {
            sb.AppendLine($"  Run {i + 1}   {FormatTime(runHistory[i])}");
        }
        sb.AppendLine();
        sb.AppendLine($"Press [{restartKey}] to restart");

        GameObject textGo = new GameObject("GameOverText");
        textGo.transform.SetParent(canvasGo.transform);
        Text text = textGo.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 40;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.color = Color.white;
        text.text = sb.ToString();

        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(800f, 600f);
        rect.localScale = Vector3.one;
    }
}
