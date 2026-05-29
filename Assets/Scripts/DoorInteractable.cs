using UnityEngine;
using PrimeTween;

public class DoorInteractable : UnlockableObject
{
    [Header("Door")]
    [SerializeField] private Transform doorPivot;

    [SerializeField] private Vector3 openedRotation;

    [SerializeField] private float duration = 0.4f;

    [Header("Behaviour")]
    [SerializeField] private bool startsUnlocked = false;

    [SerializeField] private bool startsOpen = false;

    private Quaternion closedRotation;

    private bool isOpen;

    private void Start()
    {
        closedRotation = doorPivot.localRotation;

        if (startsUnlocked)
        {
            Unlock();
        }
        else
        {
            Lock();
        }

        if (startsOpen)
        {
            OpenInstant();
        }
    }

    public override void Interact(FirstPersonController player)
    {

        ToggleDoor();
    }

    public void ToggleDoor()
    {
        if (isOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    public void Open()
    {

        if (!IsUnlocked)
        {
            return;
        }

        if (isOpen)
        {
            return;
        }

        isOpen = true;

        Tween.LocalRotation(
            doorPivot,
            openedRotation,
            duration
        );
    }

    public void Close()
    {
        if (!isOpen)
        {
            return;
        }

        isOpen = false;

        Tween.LocalRotation(
            doorPivot,
            closedRotation.eulerAngles,
            duration
        );
    }

    public void UnlockAndOpen()
    {
        Unlock();
        Open();
    }

    public void LockAndClose()
    {
        Close();
        Lock();
    }

    private void OpenInstant()
    {
        isOpen = true;

        doorPivot.localRotation =
            Quaternion.Euler(openedRotation);
    }
}