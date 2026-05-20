using UnityEngine;

public class GrabbableObject : InteractableObject
{
    [SerializeField] private HandType hand = HandType.Right;

    public override void Interact(FirstPersonController player)
    {
        player.GrabObject(gameObject, hand);
    }
}
