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

    public GameObject HeldObject { get; private set; }

    public void GrabObject(GameObject objectToGrab)
    {
        if (objectToGrab == null)
        {
            return;
        }

        DropObject();

        objectToGrab.transform.SetParent(socket);
        objectToGrab.transform.localPosition = Vector3.zero;
        objectToGrab.transform.localRotation = Quaternion.identity;

        Rigidbody rb = objectToGrab.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
        }

        Collider col = objectToGrab.GetComponent<Collider>();

        if (col != null)
        {
            col.enabled = false;
        }

        HeldObject = objectToGrab;
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