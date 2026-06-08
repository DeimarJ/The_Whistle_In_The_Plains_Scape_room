using UnityEngine;
public enum HandType
{
    Left,
    Right
}
public class PlayerHand : MonoBehaviour
{
    [Header("Socket")]
    [SerializeField] private Transform socket;
    public GrabbableObject HeldObject { get; private set; }

    public void GrabObject(GrabbableObject grabbable)
    {
        if (grabbable == null)
        {
            return;
        }

        DropObject();

        grabbable.AddToSocket(socket, false);

        HeldObject = grabbable;
    }

    public void DropObject()
    {
        if (HeldObject == null)
        {
            return;
        }
        HeldObject.RemoveFromSocket();

        HeldObject = null;
    }
}