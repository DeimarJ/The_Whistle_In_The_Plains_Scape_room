using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }

    public bool JumpPressed { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool CrouchPressed { get; private set; }
    public bool LanternPressed { get; private set; }
    public bool InteractPressed { get; private set; }
    public bool DropPressed { get; private set; }
    public bool RefillPressed { get; private set; }
    public bool PausePressed { get; private set; }
    public bool CancelPressed { get; private set; }
    public bool NextConsumablePressed { get; private set; }
    public bool PreviousConsumablePressed { get; private set; }
    public bool NextConsumableVariantPressed { get; private set; }
    public bool PreviousConsumableVariantPressed { get; private set; }

    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        inputActions.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => MoveInput = Vector2.zero;

        inputActions.Player.Look.performed += ctx => LookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => LookInput = Vector2.zero;

        inputActions.Player.Jump.performed += ctx => JumpPressed = true;
        inputActions.Player.Jump.canceled += ctx => JumpPressed = false;

        inputActions.Player.Sprint.performed += ctx => SprintHeld = true;
        inputActions.Player.Sprint.canceled += ctx => SprintHeld = false;

        inputActions.Player.Crouch.performed += ctx => CrouchPressed = !CrouchPressed;
        inputActions.Player.Lantern.performed += ctx => LanternPressed = true;
        inputActions.Player.Interact.performed += ctx => InteractPressed = true; 
        inputActions.Player.Drop.performed += ctx => DropPressed = true;
        inputActions.Player.Refill.performed += ctx => RefillPressed = true;
        inputActions.Player.Pause.performed += ctx => PausePressed = true;
        inputActions.UI.Cancel.performed += ctx => CancelPressed = true;

        inputActions.Player.Next.performed += ctx => NextConsumablePressed = true;
        inputActions.Player.Previous.performed += ctx => PreviousConsumablePressed = true;

        inputActions.Player.NextVariant.performed += ctx => NextConsumableVariantPressed = true;
        inputActions.Player.PreviousVariant.performed += ctx => PreviousConsumableVariantPressed = true;
    }

    public void ConsumeJump()
    {
        JumpPressed = false;
    }
    public void ConsumeInteract()
    {
        InteractPressed = false;
    }
    public void ConsumeDrop()
    {
        DropPressed = false;
    }
    public void ConsumeLantern()
    {
        LanternPressed = false;
    }
    public void ConsumeRefill()
    {
        RefillPressed = false;
    }
    public void ConsumePause()
    {
        PausePressed = false;
    }
    public void ConsumeCancel()
    {
        CancelPressed = false;
    }

    public void ConsumeNextConsumableVariant()
    {
        NextConsumableVariantPressed = false;
    }

    public void ConsumePreviousConsumableVariant()
    {
        PreviousConsumableVariantPressed = false;
    }
    public void ConsumeNextConsumable()
    {
        NextConsumablePressed = false;
    }

    public void ConsumePreviousConsumable()
    {
        PreviousConsumablePressed = false;
    }
    private void OnDisable()
    {
        inputActions.Disable();
    }
    public void SwitchToGameplay()
    {
        inputActions.UI.Disable();
        inputActions.Player.Enable();
    }
    public void SwitchToUI()
    {
        inputActions.Player.Disable();
        inputActions.UI.Enable();

        MoveInput = Vector2.zero;
        LookInput = Vector2.zero;
        SprintHeld = false;
    }

}