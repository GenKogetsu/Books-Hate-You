public interface IInteractable
{
    string InteractionPrompt { get; }
    bool CanInteract(PlayerInventory inventory);
    void Interact(PlayerInventory inventory);
}
