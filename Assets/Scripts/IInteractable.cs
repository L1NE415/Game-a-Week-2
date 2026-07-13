/// <summary>
/// Anything the player can interact with by pressing E
/// (greenhouse, solar panel, broken devices, etc.) implements this interface.
/// </summary>
public interface IInteractable
{
    /// <summary>Prompt shown at the bottom of the screen, e.g. "[E] Harvest food". Return null when not interactable right now.</summary>
    string GetPrompt();

    /// <summary>Called when the player presses E.</summary>
    void Interact();
}
