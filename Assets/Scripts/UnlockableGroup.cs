using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UnlockableGroup : MonoBehaviour, IUnlockable
{
    [SerializeField]
    private List<UnlockableObject> unlockableObjects = new();

    [Header("Requirements")]
    [SerializeField]
    private int requiredUnlockedSockets = 1;
    [SerializeField]
    private bool requireExactAmount = true;

    [Header("Events")]
    [SerializeField]
    private UnityEvent onRequirementMet;

    [SerializeField]
    private UnityEvent onRequirementLost;

    private bool requirementMet;

    public void CheckUnlockables()
    {
        int unlockedCount = 0;

        foreach (UnlockableObject unlockable in unlockableObjects)
        {
            if (unlockable.IsUnlocked)
            {
                unlockedCount++;
            }
        }

        bool meetsRequirement =
        requireExactAmount
        ? unlockedCount == requiredUnlockedSockets
        : unlockedCount >= requiredUnlockedSockets;

        // Acaba de cumplirse
        if (meetsRequirement && !requirementMet)
        {
            requirementMet = true;

            LockAllSocketColliders();

            onRequirementMet?.Invoke();
        }

        // Acaba de perderse
        else if (!meetsRequirement && requirementMet)
        {
            requirementMet = false;

            UnlockAllSocketColliders();

            onRequirementLost?.Invoke();
        }
    }

    private void LockAllSocketColliders()
    {
        foreach (UnlockableObject unlockable in unlockableObjects)
        {
            UnlockableSocket socket =
                unlockable as UnlockableSocket;

            if (socket != null)
            {
                socket.DisableSocketColliderPermanently();
            }
        }
    }

    private void UnlockAllSocketColliders()
    {
        foreach (UnlockableObject unlockable in unlockableObjects)
        {
            UnlockableSocket socket =
                unlockable as UnlockableSocket;

            if (socket != null)
            {
                socket.EnableSocketCollider();
            }
        }
    }

    public bool IsUnlocked => requirementMet;
}