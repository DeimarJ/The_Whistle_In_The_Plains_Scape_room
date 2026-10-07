using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private PlayerInputHandler input;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float crouchSpeed = 2f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 1.1f;
    [SerializeField] private float gravity = -25f;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 200f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("Crouch")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchingHeight = 1f;
    [SerializeField] private float crouchTransitionSpeed = 8f;
    [SerializeField] private LayerMask ceilingMask;
    [SerializeField] private float ceilingCheckDistance = 0.2f;

    [Header("Camera")]
    [SerializeField] private float standingCameraHeight = 1.6f;
    [SerializeField] private float crouchingCameraHeight = 0.8f;

    [Header("Hands")]
    [SerializeField] private PlayerHand leftHand;
    [SerializeField] private PlayerHand rightHand;
    [SerializeField] private Animator animator;

    [Header("Swimming")]
    private bool isSwimming = false;
    [SerializeField] private float swimSpeed=3f;
    public event Action<bool> OnSwimmingStateChanged;

    [Header("Interaction")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private float interactRadius = 0.5f;
    [SerializeField] private LayerMask interactMask;

    [Header("Inventory")]
    [SerializeField] private Inventory inventory;

    [Header("Footsteps")]
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float sprintStepInterval = 0.3f;
    [SerializeField] private float crouchStepInterval = 0.7f;
    private float stepTimer = 0f;

    public Inventory Inventory => inventory;
    public PlayerHand RightHand => rightHand;
    public PlayerHand LeftHand => leftHand;

    private CharacterController controller;

    private Vector3 velocity;

    private float xRotation;

    private bool isGrounded;

    private IInteractuable currentTarget;
    

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void Start()
    {
        SetCharacterHeight(standingHeight);
        SetCameraHeight(standingCameraHeight);
        MainScene.MainCanvas.HUD.InteractionPrompt?.gameObject.SetActive(false);
    }
    private void Update()
    {
        HandleLook();
        if (isSwimming)
        {
            HandleSwimming();
        }
        else
        {
            HandleMovement();
        }
        HandleJump();
        HandleCrouch();
        HandleConsumableSelection();
        HandleConsumableUse();
        HandleLantern();
        HandleInteraction();
        HandleDrop();
        HandleTogglePause();
        HandleCancel();
        ApplyGravity();
        DetectInteractable();
    }

    private void HandleMovement()
    {
        isGrounded = controller.isGrounded;

        Vector2 moveInput = input.MoveInput;

        Vector3 move =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;

        float currentSpeed = walkSpeed;
        float currentStepInterval = walkStepInterval;

        if (input.CrouchPressed)
        {
            currentSpeed = crouchSpeed;
            currentStepInterval = crouchStepInterval;
        }
        else if (input.SprintHeld)
        {
            currentSpeed = sprintSpeed;
            currentStepInterval = sprintStepInterval;
        }

        controller.Move(move * currentSpeed * Time.deltaTime);

        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        if (isMoving && isGrounded)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                SFXType stepSound = input.CrouchPressed
                    ? SFXType.PlayerCrouchStep
                    : SFXType.PlayerSteps;

                SoundManager.Instance?.PlaySFX(stepSound);
                stepTimer = currentStepInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    public void SetSwimming(bool swimming)
    {
        isSwimming = swimming;
        OnSwimmingStateChanged?.Invoke(isSwimming);
        animator.SetBool("IsSwimming", swimming);
    }

    public bool GetIsSwimming()
    {
        return isSwimming;
    }

    private void HandleSwimming()
    {
        Vector2 moveInput = input.MoveInput;

        Vector3 move =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;

        controller.Move(move * swimSpeed * Time.deltaTime);

        /*float swimMagnitude = moveInput.magnitude;

        animator.SetFloat("swingMagnitude", swimMagnitude);*/
    }


    private void HandleLantern()
    {
        if (!input.LanternPressed)
        {
            return;
        }

        input.ConsumeLantern();

        Lantern lantern = GetEquippedLantern();

        if (lantern == null)
        {
            return;
        }

        lantern.ToggleLantern();
    }

    private Lantern GetEquippedLantern()
    {
        if (rightHand.HeldObject != null)
        {
            Lantern lantern = rightHand.HeldObject.GetComponent<Lantern>();

            if (lantern != null)
            {
                return lantern;
            }
        }

        if (leftHand.HeldObject != null)
        {
            Lantern lantern = leftHand.HeldObject.GetComponent<Lantern>();

            if (lantern != null)
            {
                return lantern;
            }
        }

        return null;
    }
    private void HandleLook()
    {
        Vector2 lookInput =
        input.LookInput *
        mouseSensitivity *
        Time.deltaTime;

        xRotation -= lookInput.y;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * lookInput.x);
    }

    private void HandleJump()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (input.JumpPressed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            SoundManager.Instance?.PlaySFX(SFXType.PlayerJump);
            input.ConsumeJump();
        }
    }
    private void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
    private void HandleCancel()
    {
        if (input.CancelPressed)
        {
            UIFocusManager.Instance.CancelTopScreen();

            input.ConsumeCancel();
        }
    }
    private void HandleCrouch()
    {
        bool wantsToCrouch = input.CrouchPressed;

        // Si quiere levantarse pero hay techo
        if (!wantsToCrouch && !CanStandUp())
        {
            wantsToCrouch = true;
        }

        float targetHeight = wantsToCrouch
            ? crouchingHeight
            : standingHeight;

        float targetCameraHeight = wantsToCrouch
            ? crouchingCameraHeight
            : standingCameraHeight;

        controller.height = Mathf.Lerp(
            controller.height,
            targetHeight,
            crouchTransitionSpeed * Time.deltaTime
        );

        Vector3 center = controller.center;
        center.y = controller.height / 2f;
        controller.center = center;

        Vector3 cameraPos = cameraHolder.localPosition;

        cameraPos.y = Mathf.Lerp(
            cameraPos.y,
            targetCameraHeight,
            crouchTransitionSpeed * Time.deltaTime
        );

        cameraHolder.localPosition = cameraPos;
    }

    private void SetCharacterHeight(float height)
    {
        controller.height = height;

        Vector3 center = controller.center;
        center.y = height / 2f;
        controller.center = center;
    }

    private void SetCameraHeight(float height)
    {
        Vector3 pos = cameraHolder.localPosition;
        pos.y = height;
        cameraHolder.localPosition = pos;
    }
    private bool CanStandUp()
    {
        float castDistance = standingHeight - controller.height;

        Vector3 origin = transform.position + Vector3.up * controller.height;

        return !Physics.SphereCast(
            origin,
            controller.radius,
            Vector3.up,
            out _,
            castDistance + ceilingCheckDistance,
        ceilingMask
        );
    }

    public void GrabObject(GrabbableObject grabbable)
    {
        GetHand(grabbable.Hand).GrabObject(grabbable);
        AnimGrabObject(grabbable.Hand, true);
        SoundManager.Instance?.PlaySFX(SFXType.PlayerGrab);
    }
    public void AnimGrabObject(HandType hand, bool state)
    {
            if (hand == HandType.Left)
            {
                SetLeftHand(state);
            }
            else
            {
                SetRightHand(state);
            }  
        if (hand == HandType.Left)
        {
            SetLeftHand(state);
        }
        else
        {
            SetRightHand(state);
        }


    }
    public void DropObject(HandType hand)
    {
        GetHand(hand).DropObject();
        AnimGrabObject(hand, false);
        SoundManager.Instance?.PlaySFX(SFXType.PlayerDrop);
    }

    private PlayerHand GetHand(HandType hand)
    {
        return hand == HandType.Left
            ? leftHand
            : rightHand;
    }

    private void HandleInteraction()
    {
        if (!input.InteractPressed)
        {
            return;
        }

        Debug.Log("Interaction consumed");
        input.ConsumeInteract();

        Ray ray = new Ray(
    playerCamera.transform.position,
    playerCamera.transform.forward
);

        Debug.DrawRay(
            ray.origin,
            ray.direction * interactDistance,
            Color.red,
            1f
        );

        // Primero raycast exacto
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask))
        {
            InteractableObject interactable =
                hit.collider.GetComponentInParent<InteractableObject>();

            if (interactable != null)
            {
                interactable.Interact(this);
                currentTarget?.Highlight(false);
                return;
            }
        }

        // Si no encontr  exacto, busca cerca del centro
        Vector3 spherePosition =
            playerCamera.transform.position +
            playerCamera.transform.forward * interactDistance;

        Debug.DrawLine(
    playerCamera.transform.position,
    spherePosition,
    Color.blue,
    1f
);
        Collider[] hits = Physics.OverlapSphere(
            spherePosition,
            interactRadius,
            interactMask
        );

        float closestDot = -1f;
        InteractableObject bestInteractable = null;

        foreach (Collider col in hits)
        {
            InteractableObject interactable =
                col.GetComponentInParent<InteractableObject>();

            if (interactable == null)
            {
                continue;
            }

            Vector3 direction =
                (interactable.transform.position - playerCamera.transform.position).normalized;

            float dot = Vector3.Dot(playerCamera.transform.forward, direction);

            if (dot > closestDot)
            {
                closestDot = dot;
                bestInteractable = interactable;
            }
        }

        if (bestInteractable != null)
        {
            bestInteractable.Interact(this);
        }
    }

    public void SetLeftHand(bool active)
    {
        animator.SetLayerWeight(1, active ? 1f : 0f);
    }

    public void SetRightHand(bool active)
    {
        animator.SetLayerWeight(2, active ? 1f : 0f);
    }
    private void HandleTogglePause()
    {
        if (!input.PausePressed)
        {
            return;
        }
        input.ConsumePause();
        if (MainGame.Instance != null)
        {
            MainGame.Pause(PauseReason.PauseMenu);
        }
    }
    private void HandleDrop()
    {
        if (!input.DropPressed)
        {
            return;
        }

        input.ConsumeDrop();

        // Solo suelta la mano derecha
        DropObject(HandType.Right);
    }


    private void DetectInteractable()
    {
        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f));

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactDistance,
            interactMask))
        {
            IInteractuable interactable =
                hit.collider.GetComponent<IInteractuable>();
            if (interactable != currentTarget)
            {
                currentTarget?.Highlight(false);

                currentTarget = interactable;

                currentTarget?.Highlight(true);
            }

            if (currentTarget != null)
            {
                MainScene.MainCanvas.HUD.InteractionPrompt?.gameObject.SetActive(true);
                MainScene.MainCanvas.HUD.InteractionPrompt.text = currentTarget.InteractionText;
            }
            else
            {
                MainScene.MainCanvas.HUD.InteractionPrompt?.gameObject.SetActive(false);
            }

        }
        else
        {
            MainScene.MainCanvas.HUD.InteractionPrompt?.gameObject.SetActive(false);
            currentTarget?.Highlight(false);
            currentTarget = null;

        }
    }
    private void HandleConsumableUse()
    {
        if (!input.RefillPressed)
        {
            return;
        }

        input.ConsumeRefill();

        ConsumableData consumable =
            MainScene.MainCanvas.HUD.ConsumableSelector.SelectedConsumable;

        if (consumable == null) { return; }

        switch (consumable.effectType)
        {
            case ConsumableEffectType.Oil:
                HandleLanternRefill(consumable);
                break;

            case ConsumableEffectType.Heal:
                HandleTakeMedicine(consumable);
                break;

            case ConsumableEffectType.Chili:
                HandleUseChili(consumable);
                break;
        }
    }
    private void HandleTakeMedicine(
    ConsumableData consumable)
    {
        if (playerHealth == null)
        {
            return;
        }

        if (!inventory.HasResource(
            consumable.resourceType))
        {
            return;
        }

        bool healed =
            playerHealth.Heal(consumable.amount);

        if (!healed)
        {
            return;
        }

        inventory.ConsumeResource(
            consumable.resourceType,
            1);
    }
    private void HandleLanternRefill(
    ConsumableData consumable)
    {
        Lantern lantern = GetEquippedLantern();

        if (lantern == null)
        {
            return;
        }

        if (!inventory.HasResource(
            consumable.resourceType))
        {
            return;
        }

        bool refilled =
            lantern.RefillOil(consumable.amount);

        if (!refilled)
        {
            return;
        }

        inventory.ConsumeResource(
            consumable.resourceType,
            1);
    }

    private void HandleUseChili(
    ConsumableData consumable)
    {
        if (!inventory.HasResource(
            consumable.resourceType))
        {
            return;
        }

        inventory.ConsumeResource(
            consumable.resourceType,
            1);

        // Aplicar efecto usando consumable.amount
    }

    private void HandleConsumableSelection()
    {
        if (input.NextConsumablePressed)
        {
            if (MainScene.MainCanvas != null)
            {
                MainScene.MainCanvas.HUD.ConsumableSelector.Next();
            }

            input.ConsumeNextConsumable();
        }

        if (input.PreviousConsumablePressed)
        {
            if (MainScene.MainCanvas != null)
            {
                MainScene.MainCanvas.HUD.ConsumableSelector.Previous();
            }

            input.ConsumePreviousConsumable();
        }

        if (input.NextConsumableVariantPressed)
        {
            if (MainScene.MainCanvas != null)
            {
                MainScene.MainCanvas.HUD.ConsumableSelector.NextVariant();
            }

            input.ConsumeNextConsumableVariant();
        }

        if (input.PreviousConsumableVariantPressed)
        {
            if (MainScene.MainCanvas != null)
            {
                MainScene.MainCanvas.HUD.ConsumableSelector.PreviousVariant();
            }

            input.ConsumePreviousConsumableVariant();
        }
    }

}