using UnityEngine;
using UnityEngine.Events;

public abstract class UnlockableObject : InteractableObject, IUnlockable
{
    [Header("Events")]
    [SerializeField]
    protected UnityEvent onUnlocked;

    [SerializeField]
    protected UnityEvent onLocked;

    protected bool unlocked;

    public virtual bool IsUnlocked => unlocked;

    public virtual void Unlock()
    {
        if (unlocked)
        {
            return;
        }

        unlocked = true;

        onUnlocked?.Invoke();
    }

    public virtual void Lock()
    {
        if (!unlocked)
        {
            return;
        }

        unlocked = false;

        onLocked?.Invoke();
    }
}