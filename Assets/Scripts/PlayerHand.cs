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

        Transform objectTransform = grabbable.transform;

        objectTransform.SetParent(socket);

        if (grabbable.GrabPoint != null)
        {
            Transform grabPoint = grabbable.GrabPoint;

            objectTransform.localRotation =
                Quaternion.Inverse(grabPoint.localRotation);

            objectTransform.localPosition =
                -grabPoint.localPosition;
        }
        else
        {
            objectTransform.localPosition = Vector3.zero;
            objectTransform.localRotation = Quaternion.identity;
        }

        Rigidbody rb = grabbable.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
        }

        Collider col = grabbable.GetComponent<Collider>();

        if (col != null)
        {
            col.enabled = false;
        }

        HeldObject = grabbable;
    }

    public void DropObject()
    {
        if (HeldObject == null)
        {
            return;
        }

        HeldObject.transform.SetParent(null);

        Rigidbody rb = HeldObject.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
        }

        Collider col = HeldObject.GetComponent<Collider>();

        if (col != null)
        {
            col.enabled = true;
        }

        HeldObject = null;
    }
}