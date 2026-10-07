using System.Net.Sockets;
using UnityEngine;

public class GrabbableObject : InteractableObject
{
    [SerializeField] private HandType hand = HandType.Right;

    [SerializeField] private Transform grabPoint;
    public bool CanBeGrabbed { get; set; } = true;
    public HandType Hand => hand;

    public Transform GrabPoint => grabPoint;

    public override void Interact(FirstPersonController player)
    {
        if (!CanBeGrabbed)
        {
            return;
        }
        player.GrabObject(this);
    }
    public void AddToSocket(Transform newParent, bool enableCollider)
    {

        transform.SetParent(newParent, false);

        if (GrabPoint != null)
        {
            Transform grabPoint = GrabPoint;

            transform.localRotation =
                Quaternion.Inverse(grabPoint.localRotation);

            transform.localPosition =
                -grabPoint.localPosition;
        }
        else
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        Rigidbody rb = gameObject.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
        }

        Collider col = gameObject.GetComponent<Collider>();

        if (col != null)
        {
            col.enabled = enableCollider;
        }
    }
    public void RemoveFromSocket()
    {

        transform.SetParent(null, true);

        Rigidbody rb = gameObject.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
        }

        Collider col = gameObject.GetComponent<Collider>();

        if (col != null)
        {
            col.enabled = true;
        }
    }
}