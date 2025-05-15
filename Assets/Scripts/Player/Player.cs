
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerCharacter playerCharacter;
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private CameraSpring cameraSpring;
    // [SerializeField] private CameraLean cameraLean;
    [SerializeField] private CameraHeadBob cameraHeadBob;
    [SerializeField] private PlayerSFX playerSFX;
    [SerializeField] private PlayerWeapon playerWeapon;

    private PlayerInputActions _inputActions;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        _inputActions = new PlayerInputActions();
        _inputActions.Enable();

        playerCharacter.Initialise();
        playerCamera.Initialise(playerCharacter.GetCameraTarget());

        playerWeapon.Initialise();
        cameraHeadBob.Initialise();

        cameraSpring.Initialise();
        // cameraLean.Initialise();

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

        var device = input.Look.activeControl?.device;
        var cameraInput = new CameraInput { Look = input.Look.ReadValue<Vector2>(), usingController = device is Gamepad};

        playerCamera.UpdateRotation(cameraInput);

        // Get character input and update it
        var characterInput = new CharacterInput
        {
            Rotation = playerCamera.transform.rotation,
            Move = input.Move.ReadValue<Vector2>(),
            Jump = input.Jump.WasPressedThisFrame(),
            JumpSustain = input.Jump.IsPressed(),
            Crouch = input.Crouch.WasPressedThisFrame()
            ? CrouchInput.Toggle : CrouchInput.None,
            Shoot = input.Shoot.WasPressedThisFrame()
        };

        playerCharacter.UpdateInput(characterInput);
        playerCharacter.UpdateBody(deltaTime);

        playerWeapon.UpdateInput(state, characterInput, cameraInput);
        playerWeapon.UpdateWeapon(state.Grounded, state.Stance is Stance.Crouch);

        playerSFX.UpdateSFX(playerCharacter.transform, state.InputVelocity, state.Velocity, playerCharacter.GetLastState().Grounded, state.InputJump, state.Stance is Stance.Crouch);

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
        cameraHeadBob.UpdateHeadbob(state.Velocity, state.Grounded, state.Stance is Stance.Crouch);
        cameraSpring.UpdateSpring(deltaTime, cameraTarget.up);
        // cameraLean.UpdateLean(deltaTime, state.Acceleration, cameraTarget.up);

        // stanceVignette.UpdateVignette(deltaTime, state.Stance);
    }
    
    private void Teleport(Vector3 position)
    {
        playerCharacter.SetPosition(position);
    }
}