using UnityEngine;

public class Key : GrabbableObject
{
    [Header("Key")]
    [SerializeField] private string keyID;

    public string KeyID => keyID;


    private UnlockableSocket currentUnlockableSocket;


    public void SetSocket(UnlockableSocket socket)
    {
        currentUnlockableSocket = socket;
    }

    public void ClearSocket()
    {
        currentUnlockableSocket = null;
    }

    public override void Interact(FirstPersonController player)
    {
        // Si está dentro de un socket,
        // intenta removerse desde el socket
        if (currentUnlockableSocket != null)
        {
            currentUnlockableSocket.TryRemoveInsertedKey(player);
            return;
        }

        base.Interact(player);
    }
}
