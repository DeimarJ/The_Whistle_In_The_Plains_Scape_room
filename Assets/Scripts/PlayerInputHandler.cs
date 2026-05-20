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
    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Enable();

        inputActions.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => MoveInput = Vector2.zero;

        inputActions.Player.Look.performed += ctx => LookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => LookInput = Vector2.zero;

        inputActions.Player.Jump.performed += ctx => JumpPressed = true;
        inputActions.Player.Jump.canceled += ctx => JumpPressed = false;

        inputActions.Player.Sprint.performed += ctx => SprintHeld = true;
        inputActions.Player.Sprint.canceled += ctx => SprintHeld = false;

        inputActions.Player.Crouch.performed += ctx => CrouchPressed = !CrouchPressed;
        inputActions.Player.Lantern.performed += ctx => LanternPressed = !LanternPressed;
        inputActions.Player.Interact.performed += ctx => InteractPressed = true;
    }

    public void ConsumeJump()
    {
        JumpPressed = false;
    }
    public void ConsumeInteract()
    {
        InteractPressed = false;
    }
    private void OnDisable()
    {
        inputActions.Disable();
    }
}