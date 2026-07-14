using UnityEngine;

/// <summary>
/// Scrolls the ground texture so the sand appears to rush past.
/// Attach to the ground plane (a tiled plane/quad, NOT a Unity Terrain -
/// terrains can't scroll; replace a terrain with a big textured plane).
/// Speed comes from WorldScroller so it matches the moving props.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class GroundScroller : MonoBehaviour
{
    [Tooltip("How many texture tiles fit in one world meter. Tune until ground speed matches the props (texture tiling X or Y divided by the plane's world size)")]
    [SerializeField] private float tilesPerMeter = 0.1f;

    private Material material;
    private Vector2 offset;
    private string textureProperty;

    private void Start()
    {
        // .material creates a per-object instance, so other objects sharing the material are unaffected
        material = GetComponent<Renderer>().material;
        // URP Lit uses _BaseMap; Built-in/legacy shaders use _MainTex
        textureProperty = material.HasProperty("_BaseMap") ? "_BaseMap" : "_MainTex";
    }

    private void Update()
    {
        if (WorldScroller.Instance == null)
        {
            return;
        }

        // Ground moves toward -Z in world space = texture offset moves toward +Y (usually)
        Vector3 dir = WorldScroller.Instance.Direction;
        offset -= new Vector2(dir.x, dir.z) * (WorldScroller.Instance.Speed * tilesPerMeter * Time.deltaTime);
        material.SetTextureOffset(textureProperty, offset);
    }
}
