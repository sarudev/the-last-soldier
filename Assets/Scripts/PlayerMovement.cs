using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
  public float lookSpeed = .75f;

  private readonly float walkSpeed = 6f;
  private readonly float runSpeed = 12f;
  private readonly float jumpPower = 2f;
  private readonly float gravity = -20f;
  private readonly float lookXLimit = 90f;
  private readonly float defaultHeight = 2f;
  private readonly float crouchHeight = 1f;
  private readonly float crouchSpeed = 3f;

  private float currentSpeed = 0;
  private bool isRunning = false;
  private bool isCrouching = false;
  private bool isGrounded = false;
  private bool isJumping = false;
  private bool isFalling = false;

  private Vector3 moveDirection = Vector3.zero;
  private float rotationX = 0;
  private CharacterController characterController;
  private Camera playerCamera;
  private InputActions inputActions;

  void Awake()
  {
    inputActions = new InputActions();
  }

  void Start()
  {
    characterController = GetComponent<CharacterController>();
    playerCamera = Camera.main;
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
  }

  void OnDisable()
  {
    inputActions.Disable();
  }

  void OnEnable()
  {
    inputActions.Enable();
  }

  void Update()
  {
    Vector3 forward = transform.TransformDirection(Vector3.forward);
    Vector3 right = transform.TransformDirection(Vector3.right);

    var movement = inputActions.Player.Move.ReadValue<Vector2>();
    var look = inputActions.Player.Look.ReadValue<Vector2>();
    isCrouching = inputActions.Player.Crouch.IsPressed();
    isRunning = inputActions.Player.Sprint.IsPressed() && !isCrouching;
    isGrounded = characterController.isGrounded;

    if (isCrouching)
    {
      characterController.height = crouchHeight;
      currentSpeed = crouchSpeed;
    }
    else
    {
      characterController.height = defaultHeight;
      currentSpeed = walkSpeed;
    }

    if (isRunning)
      currentSpeed = runSpeed;

    float curSpeedX = currentSpeed * movement.y;
    float curSpeedY = currentSpeed * movement.x;
    float movementDirectionY = moveDirection.y;
    moveDirection = (forward * curSpeedX) + (right * curSpeedY);

    if (inputActions.Player.Jump.IsPressed() && isGrounded)
    {
      moveDirection.y = Mathf.Sqrt(jumpPower * -2f * gravity);
      isJumping = true;
    }
    else
    {
      if (isGrounded)
      {
        isJumping = false;
        isFalling = false;
      }
      moveDirection.y = movementDirectionY;
    }

    if (!isGrounded)
    {
      moveDirection.y += gravity * Time.deltaTime;
    }

    if (moveDirection.y < 0 && !isGrounded)
      isFalling = true;

    if (!isJumping && !isFalling && isGrounded)
    {
      moveDirection.y = -1;
    }

    characterController.Move(moveDirection * Time.deltaTime);

    var lookSpeedMod = lookSpeed / 10;
    rotationX += -look.y * lookSpeedMod;
    rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
    playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
    transform.rotation *= Quaternion.Euler(0, look.x * lookSpeedMod, 0);
  }
}
