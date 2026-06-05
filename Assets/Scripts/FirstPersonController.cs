using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private PlayerInputHandler input;

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

    [Header("Interaction")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private float interactRadius = 0.5f;
    [SerializeField] private LayerMask interactMask;
    [SerializeField] private TMP_Text interactionPrompt;

    [Header("Inventory")]
    [SerializeField] private Inventory inventory;

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
        interactionPrompt.gameObject.SetActive(false);


    }
    private void Update()
    {
        HandleLook();
        HandleMovement();
        HandleJump();
        HandleCrouch();
        HandleLantern();
        HandleInteraction();
        HandleDrop();
        HandleLanternRefill();
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

        if (input.CrouchPressed)
        {
            currentSpeed = crouchSpeed;
        }
        else if (input.SprintHeld)
        {
            currentSpeed = sprintSpeed;
        }

        controller.Move(move * currentSpeed * Time.deltaTime);
    }
    public void GiveLantern()
    {
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
    private void HandleLanternRefill()
    {
        if (!input.RefillPressed)
        {
            return;
        }

        input.ConsumeRefill();

        Lantern lantern = GetEquippedLantern();

        if (lantern == null)
        {
            return;
        }

        if (!inventory.HasResource(ResourceType.Oil))
        {
            return;
        }

        bool refilled = lantern.RefillOil(50f);

        if (!refilled)
        {
            return;
        }

        inventory.ConsumeResource(ResourceType.Oil,1);
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
    }

    public void DropObject(HandType hand)
    {
        GetHand(hand).DropObject();
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

        // Si no encontró exacto, busca cerca del centro
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
                interactionPrompt.gameObject.SetActive(true);
                interactionPrompt.text = currentTarget.InteractionText;
            }
            else
            {
                interactionPrompt.gameObject.SetActive(false);
            }

        }
        else
        {
            interactionPrompt.gameObject.SetActive(false);
            currentTarget?.Highlight(false);
            currentTarget = null;

        }
    }

}