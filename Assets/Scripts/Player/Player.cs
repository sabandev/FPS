using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerCharacter playerCharacter;
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private CameraSpring cameraSpring;
    [SerializeField] private CameraLean cameraLean;
    [SerializeField] private PlayerSFX playerSFX;

    private PlayerInputActions _inputActions;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        _inputActions = new PlayerInputActions();
        _inputActions.Enable();

        playerCharacter.Initialise();
        playerCamera.Initialise(playerCharacter.GetCameraTarget());

        cameraSpring.Initialise();
        cameraLean.Initialise();

        playerSFX.Initialise();
    }

    private void OnDestroy()
    {
        _inputActions.Dispose(); 
    }

    private void Update()
    {
        var input = _inputActions.Gameplay;
        var deltaTime = Time.deltaTime;
        var state = playerCharacter.GetState();

        // Get camera input and update its rotations
        var cameraInput = new CameraInput { Look = input.Look.ReadValue<Vector2>() };
        playerCamera.UpdateRotation(cameraInput);

        // Get character input and update it
        var characterInput = new CharacterInput
        {
            Rotation = playerCamera.transform.rotation,
            Move = input.Move.ReadValue<Vector2>(),
            Jump = input.Jump.WasPressedThisFrame(),
            JumpSustain = input.Jump.IsPressed(),
            Crouch = input.Crouch.WasPressedThisFrame()
            ? CrouchInput.Toggle : CrouchInput.None
        };
        playerCharacter.UpdateInput(characterInput);
        playerCharacter.UpdateBody(deltaTime);
        playerSFX.UpdateSFX(state.InputVelocity, state.Velocity, playerCharacter.GetLastState().Grounded, state.InputJump);

        #if UNITY_EDITOR
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            var ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out var hit))
            {
                Teleport(hit.point);
            }
        }
        #endif
    }

    private void LateUpdate()
    {
        var deltaTime = Time.deltaTime;
        var cameraTarget = playerCharacter.GetCameraTarget();
        var state = playerCharacter.GetState();

        playerCamera.UpdatePosition(cameraTarget);
        cameraSpring.UpdateSpring(deltaTime, cameraTarget.up);
        cameraLean.UpdateLean(deltaTime, state.Acceleration, cameraTarget.up);

        // stanceVignette.UpdateVignette(deltaTime, state.Stance);
    }
    
    private void Teleport(Vector3 position)
    {
        playerCharacter.SetPosition(position);
    }
}