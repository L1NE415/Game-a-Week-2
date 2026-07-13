using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Attach to the Player. Every frame, SphereCasts forward to find the nearest
/// IInteractable and triggers it on E. Device interaction takes priority over
/// item pickup (PlayerCarryController checks HasTarget).
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private Key interactKey = Key.E;
    [SerializeField] private float interactDistance = 2f;
    [SerializeField] private float interactRadius = 0.6f;
    [SerializeField] private LayerMask interactLayers = ~0;

    /// <summary>Interactable currently aimed at (HUD shows its prompt, CarryController yields E to it).</summary>
    public IInteractable CurrentTarget { get; private set; }

    /// <summary>Whether there is an interaction target this frame.</summary>
    public bool HasTarget => CurrentTarget != null && CurrentTarget.GetPrompt() != null;

    private void Update()
    {
        CurrentTarget = FindNearestInteractable();

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard[interactKey].wasPressedThisFrame && HasTarget)
        {
            CurrentTarget.Interact();
        }
    }

    private IInteractable FindNearestInteractable()
    {
        Vector3 origin = transform.position + Vector3.up * 0.75f;

        RaycastHit[] hits = Physics.SphereCastAll(
            origin,
            interactRadius,
            transform.forward,
            interactDistance,
            interactLayers,
            QueryTriggerInteraction.Collide);

        IInteractable nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (RaycastHit hit in hits)
        {
            IInteractable candidate = hit.collider.GetComponentInParent<IInteractable>();

            if (candidate == null || candidate.GetPrompt() == null)
            {
                continue;
            }

            if (hit.distance < nearestDistance)
            {
                nearestDistance = hit.distance;
                nearest = candidate;
            }
        }

        return nearest;
    }
}
