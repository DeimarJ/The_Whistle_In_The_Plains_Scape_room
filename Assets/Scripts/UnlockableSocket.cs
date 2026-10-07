using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UnlockableSocket : UnlockableObject
{
    [Header("Correct Key")]
    [SerializeField] private string correctKeyID;

    [Header("Wrong Keys")]
    [SerializeField] private List<string> acceptedWrongKeyIDs = new();



    [Header("Behaviour")]
    [SerializeField] private bool allowRemoveCorrectKey = true;

    [SerializeField] private bool allowRemoveWrongKeys = true;


    [Header("Socket")]
    [SerializeField] private Transform keySocket;
    [SerializeField] private Collider socketCollider;

    [Header("Events")]
    [SerializeField] private UnityEvent onKeyInserted;

    [SerializeField] private UnityEvent onKeyRemoved;

    private bool colliderLocked;
    private Key insertedKey;

    public override void Interact(FirstPersonController player)
    {
        if (insertedKey != null)
        {
            return;
        }

        TryInsertKey(player);
    }

    private void TryInsertKey(FirstPersonController player)
    {
        PlayerHand rightHand = player.RightHand;

        if (rightHand.HeldObject == null)
        {
            return;
        }

        Key key =
            rightHand.HeldObject as Key;

        if (key == null)
        {
            return;
        }

        bool isCorrectKey =
    key.KeyID == correctKeyID;

        bool isWrongAccepted =
            acceptedWrongKeyIDs.Contains(key.KeyID);


        if (!isCorrectKey && !isWrongAccepted)
        {
            return;
        }

        InsertKey(player, key);

        if (isCorrectKey)
        {
            Unlock();
        }
    }

    private void InsertKey(
    FirstPersonController player,
    Key key
)
    {
        insertedKey = key;

        player.DropObject(HandType.Right);

        // collider OFF en la llave
        key.AddToSocket(keySocket, true);

        key.CanBeGrabbed = true;

        key.SetSocket(this);

        // desactiva collider del socket
        if (socketCollider != null)
        {
            socketCollider.enabled = false;
        }

        onKeyInserted?.Invoke();
    }

    public void TryRemoveInsertedKey(
    FirstPersonController player
)
    {
        if (insertedKey == null)
        {
            return;
        }

        bool isCorrectKey =
            insertedKey.KeyID == correctKeyID;

        if (isCorrectKey && !allowRemoveCorrectKey)
        {
            return;
        }

        if (!isCorrectKey && !allowRemoveWrongKeys)
        {
            return;
        }

        Key keyToRemove = insertedKey;

        insertedKey = null;

        keyToRemove.ClearSocket();

        keyToRemove.RemoveFromSocket();

        // reactiva collider del socket
        if (!colliderLocked && socketCollider != null)
        {
            socketCollider.enabled = true;
        }
        player.GrabObject(keyToRemove);

        onKeyRemoved?.Invoke();

        if (isCorrectKey)
        {
            Lock();
        }
    }
    public void DisableSocketColliderPermanently()
    {
        colliderLocked = true;

        if (socketCollider != null)
        {
            socketCollider.enabled = false;
        }
    }
    public void EnableSocketCollider()
    {
        colliderLocked = false;

        // Si todavía hay llave insertada,
        // no activar collider
        if (insertedKey != null)
        {
            return;
        }

        if (socketCollider != null)
        {
            socketCollider.enabled = true;
        }
    }
}
