using UnityEngine;

public class RadarSweep : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 150f;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        rectTransform.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);
    }
}
