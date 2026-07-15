using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives a circular UI fill image from VehicleResources.
/// Attach this to the meter root, then assign the Fill child Image or leave it
/// empty to auto-find a child named "Fill".
/// </summary>
public class ResourceFillMeter : MonoBehaviour
{
    public enum ResourceType
    {
        Hunger,
        Thirst,
        Sanity,
        Energy
    }

    [SerializeField] private ResourceType resourceType = ResourceType.Hunger;
    [SerializeField] private Image fillImage;
    [SerializeField] private bool smoothFill = true;
    [SerializeField] private float smoothSpeed = 10f;

    private float targetFill = 1f;

    private void Reset()
    {
        AutoAssignFillImage();
        ConfigureFillImage();
    }

    private void Awake()
    {
        if (fillImage == null)
        {
            AutoAssignFillImage();
        }

        ConfigureFillImage();
    }

    private void OnValidate()
    {
        if (fillImage == null)
        {
            AutoAssignFillImage();
        }

        ConfigureFillImage();
    }

    private void Update()
    {
        VehicleResources resources = VehicleResources.Instance;
        if (resources == null || fillImage == null)
        {
            return;
        }

        targetFill = GetNormalizedValue(resources);

        if (smoothFill)
        {
            fillImage.fillAmount = Mathf.MoveTowards(
                fillImage.fillAmount,
                targetFill,
                smoothSpeed * Time.deltaTime);
        }
        else
        {
            fillImage.fillAmount = targetFill;
        }
    }

    private float GetNormalizedValue(VehicleResources resources)
    {
        switch (resourceType)
        {
            case ResourceType.Hunger:
                return SafeRatio(resources.Hunger, resources.MaxHunger);
            case ResourceType.Thirst:
                return SafeRatio(resources.Thirst, resources.MaxThirst);
            case ResourceType.Sanity:
                return SafeRatio(resources.Sanity, resources.MaxSanity);
            case ResourceType.Energy:
                return SafeRatio(resources.Energy, resources.MaxEnergy);
            default:
                return 0f;
        }
    }

    private static float SafeRatio(float current, float max)
    {
        if (max <= 0f)
        {
            return 0f;
        }

        return Mathf.Clamp01(current / max);
    }

    private void ConfigureFillImage()
    {
        if (fillImage == null)
        {
            return;
        }

        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Vertical;
        fillImage.fillOrigin = (int)Image.OriginVertical.Bottom;
        fillImage.fillAmount = Mathf.Clamp01(fillImage.fillAmount);
    }

    private void AutoAssignFillImage()
    {
        Transform fillTransform = transform.Find("Fill");
        if (fillTransform != null)
        {
            fillImage = fillTransform.GetComponent<Image>();
        }
    }
}
