using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    [Header("References")]
    private CharacterController controller;
    [SerializeField] private CinemachineCamera playerCamera;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = 50.0f;
    [SerializeField] private float jumpHeight = 1.0f;

    private float verticalVelocity;
    private float xRotation;

    [Header("Input")]
    [SerializeField] private InputActionAsset playerControls;
    [SerializeField] private float mouseSensitivity;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private Vector2 moveInput;
    private Vector2 lookInput;

    private float verticalInput;
    private float horizontalInput;
    private float mouseX;
    private float mouseY;

    Vector3 move;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        moveAction = playerControls.FindActionMap("Base Movement").FindAction("Move");
        lookAction = playerControls.FindActionMap("Base Movement").FindAction("Look");
        jumpAction = playerControls.FindActionMap("Base Movement").FindAction("Jump");

        moveAction.performed += context => moveInput = context.ReadValue<Vector2>();
        moveAction.canceled += context => moveInput = Vector2.zero;

        lookAction.performed += context => lookInput = context.ReadValue<Vector2>();
        lookAction.canceled += context => lookInput = Vector2.zero;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        jumpAction.Disable();
    }

    private void Update()
    {
        InputManagement();
        Movement();
    }

    private void Movement()
    {
        GroundMovement();
        Turn();
    }

    private void GroundMovement()
    {
        move = new Vector3(horizontalInput, 0, verticalInput);
        move = playerCamera.transform.TransformDirection(move);

        move *= moveSpeed;

        move.y = Jump();

        controller.Move(move * Time.deltaTime);
    }

    private void Turn()
    {
        mouseX *= mouseSensitivity * Time.deltaTime;
        mouseY *= mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -90, 90);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        transform.Rotate(Vector3.up * mouseX);
    }

    private float Jump()
    {
        if (controller.isGrounded)
        {
            verticalVelocity = -1;

            if(jumpAction.triggered)
                verticalVelocity = Mathf.Sqrt(jumpHeight * gravity * 2);
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
        return verticalVelocity;
    }

    private void InputManagement()
    {
        // moveInput = Input.GetAxis("Vertical");
        // turnInput = Input.GetAxis("Horizontal");
        // mouseX = Input.GetAxis("Mouse X");
        // mouseY = Input.GetAxis("Mouse Y");

        mouseX = lookInput.x * mouseSensitivity;
        mouseY = lookInput.y * mouseSensitivity;

        verticalInput = moveInput.y * moveSpeed;
        horizontalInput = moveInput.x * moveSpeed;
    }
}
