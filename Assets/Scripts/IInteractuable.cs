using UnityEngine;

public interface IInteractuable
{
    string InteractionText { get; }
    void Highlight(bool state);
    void Interact();
}
