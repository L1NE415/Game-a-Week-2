using UnityEngine;

/// <summary>
/// Attach to background props (rocks, dunes, ruins). The prop slides with the
/// world (direction comes from WorldScroller, so it works for any drive
/// direction); once it passes far enough behind, it teleports ahead and comes
/// around again, with a little random sideways shift so the loop is less obvious.
/// </summary>
public class ScrollingProp : MonoBehaviour
{
    [Tooltip("Reference point for 'behind' (usually the car). Empty = world origin")]
    [SerializeField] private Transform center;
    [Tooltip("Recycle once the prop is this far behind the center, along the scroll direction")]
    [SerializeField] private float recycleDistance = 30f;
    [Tooltip("How far the prop jumps forward when recycled (should exceed recycleDistance + how far ahead props start)")]
    [SerializeField] private float loopLength = 80f;
    [Tooltip("Random sideways shift on each recycle, 0 = keep lane")]
    [SerializeField] private float randomSideRange = 6f;

    private void Update()
    {
        if (WorldScroller.Instance == null)
        {
            return;
        }

        Vector3 dir = WorldScroller.Instance.Direction; // direction the world moves (e.g. (-1,0,0) when driving right)
        transform.position += dir * (WorldScroller.Instance.Speed * Time.deltaTime);

        // How far along the scroll direction are we past the center?
        Vector3 origin = center != null ? center.position : Vector3.zero;
        float traveled = Vector3.Dot(transform.position - origin, dir);

        if (traveled > recycleDistance)
        {
            // Jump back ahead of the car
            Vector3 pos = transform.position - dir * loopLength;

            if (randomSideRange > 0f)
            {
                Vector3 side = Vector3.Cross(Vector3.up, dir); // perpendicular, on the ground plane
                pos += side * Random.Range(-randomSideRange, randomSideRange);
            }

            transform.position = pos;
        }
    }
}
