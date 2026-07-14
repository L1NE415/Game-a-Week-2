using UnityEngine;
using UnityEngine.UI;

public class RadarWeatherVisuals : MonoBehaviour
{
    private enum WeatherSource
    {
        FollowWeatherSystem,
        ManualClear,
        ManualSandstorm,
        ManualHeatwave
    }

    [Header("Tint Targets")]
    [SerializeField] private Image radarGrid;
    [SerializeField] private Image sweeper;
    [SerializeField] private Image warningRing;

    [Header("Weather Colors")]
    [SerializeField] private Color clearColor = new Color(0.1f, 1f, 0.35f, 1f);
    [SerializeField] private Color sandstormColor = new Color(1f, 0.85f, 0.15f, 1f);
    [SerializeField] private Color heatwaveColor = new Color(1f, 0.15f, 0.05f, 1f);

    [Header("Testing")]
    [Tooltip("Use the manual options to test radar colors in scenes that do not have a WeatherSystem.")]
    [SerializeField] private WeatherSource weatherSource = WeatherSource.FollowWeatherSystem;

    private void Update()
    {
        Color weatherColor = GetCurrentWeatherColor();

        SetColor(radarGrid, weatherColor);
        SetColor(sweeper, weatherColor);
        SetColor(warningRing, weatherColor);
    }

    private Color GetCurrentWeatherColor()
    {
        switch (weatherSource)
        {
            case WeatherSource.ManualSandstorm:
                return sandstormColor;
            case WeatherSource.ManualHeatwave:
                return heatwaveColor;
            case WeatherSource.ManualClear:
                return clearColor;
        }

        WeatherSystem weather = WeatherSystem.Instance;
        if (weather == null)
        {
            return clearColor;
        }

        switch (weather.Current)
        {
            case WeatherSystem.WeatherType.Sandstorm:
                return sandstormColor;
            case WeatherSystem.WeatherType.Heatwave:
                return heatwaveColor;
            default:
                return clearColor;
        }
    }

    private void SetColor(Image image, Color color)
    {
        if (image != null)
        {
            image.color = color;
        }
    }
}
