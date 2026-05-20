using UnityEngine;
using PrimeTween;

public class DoorInteractable : InteractableObject
{
    [Header("Door")]
    [SerializeField] private Transform doorPivot;

    [SerializeField] private Vector3 openedRotation;
    [SerializeField] private float duration = 0.4f;

    private Quaternion closedRotation;
    private bool isOpen;

    private void Start()
    {
        closedRotation = doorPivot.localRotation;
    }

    public override void Interact(FirstPersonController player)
    {
        isOpen = !isOpen;

        Quaternion targetRotation = isOpen
            ? Quaternion.Euler(openedRotation)
            : closedRotation;

        Tween.LocalRotation(
            doorPivot,
            targetRotation.eulerAngles,
            duration
        );
    }
}