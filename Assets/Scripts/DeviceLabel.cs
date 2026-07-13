using UnityEngine;

/// <summary>
/// Floating status label above a device (world space, aligned with the camera).
/// Created automatically by ShipDevice in Start; no manual setup needed.
/// Content comes from ShipDevice.GetStatusText().
/// </summary>
public class DeviceLabel : MonoBehaviour
{
    private ShipDevice device;
    private TextMesh textMesh;
    private Transform cam;

    /// <summary>Called by ShipDevice to create and initialize a label.</summary>
    public static DeviceLabel Create(ShipDevice device, float height, float textSize)
    {
        GameObject go = new GameObject($"{device.DeviceName} Label");
        go.transform.SetParent(device.transform);
        go.transform.localPosition = Vector3.up * height;

        DeviceLabel label = go.AddComponent<DeviceLabel>();
        label.device = device;

        TextMesh tm = go.AddComponent<TextMesh>();
        tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tm.fontSize = 64;                       // font texture resolution (bigger = sharper)
        tm.characterSize = textSize;            // actual world-space size
        tm.anchor = TextAnchor.LowerCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = Color.white;
        go.GetComponent<MeshRenderer>().material = tm.font.material;

        label.textMesh = tm;
        return label;
    }

    private void LateUpdate()
    {
        if (device == null)
        {
            Destroy(gameObject);
            return;
        }

        textMesh.text = device.GetStatusText();
        textMesh.color = device.IsBroken ? new Color(1f, 0.35f, 0.25f) : Color.white;

        // Stay parallel to the screen (suits the fixed top-down camera:
        // adopt the camera's rotation so no label ever looks skewed)
        if (cam == null && Camera.main != null)
        {
            cam = Camera.main.transform;
        }
        if (cam != null)
        {
            transform.rotation = cam.rotation;
        }
    }
}
