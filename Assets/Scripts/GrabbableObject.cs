using UnityEngine;

public class GrabbableObject : InteractableObject
{
    [SerializeField] private HandType hand = HandType.Right;

    [SerializeField] private Transform grabPoint;

    public HandType Hand => hand;

    public Transform GrabPoint => grabPoint;

    public override void Interact(FirstPersonController player)
    {
        player.GrabObject(this);
    }
}