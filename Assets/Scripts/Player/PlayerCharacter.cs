using UnityEngine;
using KinematicCharacterController;
using TMPro;
using UnityEngine.InputSystem.Controls;

public enum CrouchInput
{
    None, Toggle, KeepCrouch, Uncrouch
}

public enum Stance
{
    Stand, Crouch, Slide
}

public struct CharacterState
{
    public bool Grounded;
    public Stance Stance;
    public Vector3 Velocity;
    public Vector3 Acceleration;
}

public struct CharacterInput
{
    public Quaternion Rotation;
    public Vector2 Move;

    public bool Jump;
    public bool JumpSustain;

    public CrouchInput Crouch;
}

public class PlayerCharacter : MonoBehaviour, ICharacterController
{
    [SerializeField] private KinematicCharacterMotor motor;

    [SerializeField] private Transform root;
    [SerializeField] private Transform cameraTarget;

    [SerializeField] private float walkSpeed = 20.0f;
    [SerializeField] private float crouchSpeed = 10.0f;

    [SerializeField] private float walkResponse = 25.0f;
    [SerializeField] private float crouchResponse = 20.0f;

    [SerializeField] private float airSpeed = 20.0f;
    [SerializeField] private float airAcceleration = 70.0f;

    [SerializeField] private float slideStartSpeed = 25.0f;
    [SerializeField] private float slideEndSpeed = 15.0f;
    [SerializeField] private float slideFriction = 0.8f;
    [SerializeField] private float slideSteerAcceleration = 5.0f;
    [SerializeField] private float slideGravity = -90f;
    
    [SerializeField] private float jumpSpeed = 20.0f;
    [Range(0.0f, 1.0f)] [SerializeField] private float jumpSustainGravity = 0.4f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float gravity = -90.0f;

    [SerializeField] private float standHeight = 2.0f;
    [SerializeField] private float crouchHeight = 1.0f;
    [SerializeField] private float crouchHeightResponse = 15.0f;
    [Range(0.0f, 1.0f)] [SerializeField] private float standCameraTargetHeight = 0.9f;
    [Range(0.0f, 1.0f)] [SerializeField] private float crouchCameraTargetHeight = 0.7f;
    

    private CharacterState _state;
    private CharacterState _lastState;
    private CharacterState _tempState;
 
    private Quaternion _requestedRotation;

    private Vector3 _requestedMovement;

    private Collider[] _uncrouchOverlapResults;

    private float _timeSinceUngrounded;
    private float _timeSinceJumpRequest;

    private bool _requestedJump;
    private bool _requestedSustainedJump;
    private bool _requestedCrouch;
    private bool _requestedCrouchInAir;
    private bool _ungroundedDueToJump;

    public void Initialise()
    {
        _state.Stance = Stance.Stand;
        _lastState = _state;
     
        motor.CharacterController = this;
        _uncrouchOverlapResults = new Collider[8];
    }

    public void UpdateInput(CharacterInput input)
    {
        _requestedRotation = input.Rotation;

        // Take 2D input vector and create a 3D movement vector on XZ planes
        _requestedMovement = new Vector3(input.Move.x, 0.0f, input.Move.y);

        // Clamp length to 1 to prevent disproportionately faster movement diagonally
        _requestedMovement = Vector3.ClampMagnitude(_requestedMovement, 1.0f);

        // Orient input so it's relative to where the player is facing (not global)
        _requestedMovement = input.Rotation * _requestedMovement;

        var wasRequestingJump = _requestedJump;

        // Check if we have requested a jump
        _requestedJump = _requestedJump || input.Jump;

        if (_requestedJump && !wasRequestingJump)
            _timeSinceJumpRequest = 0.0f;

        // Check if we have requested a sustained jump
        _requestedSustainedJump = input.JumpSustain;

        var wasRequestingCrouch = _requestedCrouch;

        // Check if we have requested a crouch
        _requestedCrouch = input.Crouch switch
        {
            CrouchInput.Toggle => !_requestedCrouch,
            CrouchInput.None => _requestedCrouch,
            CrouchInput.KeepCrouch => true,
            CrouchInput.Uncrouch => false,
            _ => _requestedCrouch
        };

        if (_requestedCrouch && !wasRequestingCrouch)
            _requestedCrouchInAir = !_state.Grounded;
        else if(!_requestedCrouch && wasRequestingCrouch)
            _requestedCrouchInAir = false;
    }

    public void UpdateBody(float deltaTime)
    {
        // Adjust camera target height depending on whether we're stood up or not
        var currentHeight = motor.Capsule.height;
        var normalisedHeight = currentHeight / standHeight;

        var cameraTargetHeight = currentHeight * 
        (
            _state.Stance is Stance.Stand
            ? standCameraTargetHeight
            : crouchCameraTargetHeight
        );
        var rootTargetScale = new Vector3(1.0f, normalisedHeight, 1.0f);

        cameraTarget.localPosition = Vector3.Lerp
        (
            a: cameraTarget.localPosition, 
            b: new Vector3(0.0f, cameraTargetHeight, 0.0f), 
            t: 1.0f - Mathf.Exp(-crouchHeightResponse * deltaTime) // Frame-rate independant
        );

        root.localScale = Vector3.Lerp
        (
            a: root.localScale,
            b: rootTargetScale,
            t: 1.0f - Mathf.Exp(-crouchHeightResponse * deltaTime) // Frame-rate independant
        );
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        _state.Acceleration = Vector3.zero;

        // If grounded, allow movement. If not, apply gravity
        if (motor.GroundingStatus.IsStableOnGround)
        {
            _timeSinceUngrounded = 0.0f;
            _ungroundedDueToJump = false;

            var groundedMovement = motor.GetDirectionTangentToSurface
            (
                direction: _requestedMovement,
                surfaceNormal: motor.GroundingStatus.GroundNormal
            ) * _requestedMovement.magnitude;

            // Start sliding
            {
                var moving = groundedMovement.sqrMagnitude > 0.0f;
                var crouching = _state.Stance is Stance.Crouch;
                var wasStanding = _lastState.Stance is Stance.Stand; // Check if we were standing LAST frame
                var wasInAir = !_lastState.Grounded; // Check if we were standing LAST frame

                if (moving && crouching && (wasStanding || wasInAir))
                {
                    _state.Stance = Stance.Slide;

                    // Preserve velocity from the last frame
                    if (wasInAir)
                    {
                        currentVelocity = Vector3.ProjectOnPlane
                        (
                            vector: _lastState.Velocity,
                            planeNormal: motor.GroundingStatus.GroundNormal
                        );
                    }
                    
                    var effectiveSlideStartSpeed = slideStartSpeed; 

                    if (!_lastState.Grounded && !_requestedCrouchInAir)
                    {
                        effectiveSlideStartSpeed = 0.0f;
                        _requestedCrouchInAir = false;
                    }

                    var slideSpeed = Mathf.Max(effectiveSlideStartSpeed, currentVelocity.magnitude);
                    currentVelocity = motor.GetDirectionTangentToSurface
                    (
                        direction: currentVelocity,
                        surfaceNormal: motor.GroundingStatus.GroundNormal
                    ) * slideSpeed;
                }
            }

            // Grounded Movement
            if (_state.Stance is Stance.Stand or Stance.Crouch)
            {
                // Calculate speed and responsiveness of movement
                var speed = _state.Stance is Stance.Stand ? walkSpeed : crouchSpeed;

                var response = _state.Stance is Stance.Stand ? walkResponse : crouchResponse;

                var targetVelocity = groundedMovement * speed;
                var moveVelocity = Vector3.Lerp
                (
                    a: currentVelocity,
                    b: targetVelocity,
                    t: 1.0f - Mathf.Exp(-response * deltaTime)
                );
                _state.Acceleration = moveVelocity - currentVelocity;
                currentVelocity = moveVelocity;
            }
            // Continue sliding
            else
            {
                // Apply sliding friction
                currentVelocity -= currentVelocity * (slideFriction * deltaTime);

                // Handle slopes
                {
                    var force = Vector3.ProjectOnPlane
                    (
                        vector: -motor.CharacterUp,
                        planeNormal: motor.GroundingStatus.GroundNormal
                    ) * slideGravity;

                    currentVelocity -= force * deltaTime;
                }

                // Steer
                {
                    // Steer sliding
                    var currentSpeed = currentVelocity.magnitude;
                    var targetVelocity = groundedMovement * currentVelocity.magnitude;
                    var steerVelocity = currentVelocity;
                    var steerForce = (targetVelocity - steerVelocity) * slideSteerAcceleration * deltaTime;

                    // Add steer force to velocity
                    steerVelocity += steerForce;

                    // Clamp speed to not exceed target velocity
                    steerVelocity = Vector3.ClampMagnitude(steerVelocity, currentSpeed);

                    _state.Acceleration = (steerVelocity - currentVelocity) / deltaTime;
                    currentVelocity = steerVelocity;
                }

                if (currentVelocity.magnitude < slideEndSpeed)
                    _state.Stance = Stance.Crouch;
            }

        }
        // Else, in the air
        else
        {
            _timeSinceUngrounded += deltaTime;

            // Air movement
            if (_requestedMovement.sqrMagnitude > 0.0f)
            {
                // Movement on XZ plane
                var planarMovement = Vector3.ProjectOnPlane
                (
                    vector: _requestedMovement,
                    planeNormal: motor.CharacterUp
                ) * _requestedMovement.normalized.magnitude;

                var currentPlanarVelocity = Vector3.ProjectOnPlane
                (
                    vector: currentVelocity,
                    planeNormal: motor.CharacterUp
                );

                var movementForce = planarMovement * airAcceleration * deltaTime;

                // Handle sliding into jumps; any movement slower than the max air speed is treated as a steering force
                if (currentPlanarVelocity.magnitude < airSpeed)
                {
                    var targetPlanarVelocity = currentPlanarVelocity + movementForce;

                    targetPlanarVelocity = Vector3.ClampMagnitude(targetPlanarVelocity, airSpeed);

                    // Steer towards current velocity
                    movementForce = targetPlanarVelocity - currentPlanarVelocity;
                }
                // Otherwise, clamp extra movement force to prevent excessive speed in the air
                else if (Vector3.Dot(currentPlanarVelocity, movementForce) > 0.0f)
                {
                    var constrainedMovementForce = Vector3.ProjectOnPlane
                    (
                        vector: movementForce,
                        planeNormal: currentPlanarVelocity.normalized
                    );

                    movementForce = constrainedMovementForce;
                }

                // Prevent air-climbing on colliders
                if (motor.GroundingStatus.FoundAnyGround)
                {
                    // If moving into any ground while in the air, prevent air climbing
                    if (Vector3.Dot(movementForce, currentVelocity + movementForce) > 0.0f)
                    {
                        var obstructionNormal = Vector3.Cross
                        (
                            motor.CharacterUp,
                            Vector3.Cross
                            (
                                motor.CharacterUp,
                                motor.GroundingStatus.GroundNormal
                            )
                        ).normalized;

                        movementForce = Vector3.ProjectOnPlane(movementForce, obstructionNormal);
                    }
                }

                // Steer towards current velocity
                currentVelocity += movementForce;
            }

            // Change gravity depedning on sustained jump
            var effectiveGravity = gravity;
            var verticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);

            if (_requestedSustainedJump && verticalSpeed > 0.0f)
                effectiveGravity *= jumpSustainGravity;

            // Gravity
            currentVelocity += motor.CharacterUp * effectiveGravity * deltaTime;
        }

        // Jump
        if (_requestedJump)
        {
            var grounded = motor.GroundingStatus.IsStableOnGround;
            var canCoyoteJump = _timeSinceUngrounded < coyoteTime && !_ungroundedDueToJump;

            // If the player is grounded, allow a jump
            if (grounded || canCoyoteJump)
            { 
                _requestedJump = false;
                _requestedCrouch = false;
                _requestedCrouchInAir = false;

                // Force unstick the player from the ground
                motor.ForceUnground(time: 0.0f);
                _ungroundedDueToJump = true;

                // Set minimum vertical speed to jump speed
                var currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
                var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);

                // Add the difference between current and target speeds to the character's velocity
                currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
            }
            // If not, deny the jump
            else
            {
                _timeSinceJumpRequest += deltaTime;

                // Defer jump until coyote time
                var canJumpLater = _timeSinceJumpRequest < coyoteTime;

                _requestedJump = canJumpLater;
            }

        }
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        // Ensure we don't pitch our character according to our camera rotation
        var forward = Vector3.ProjectOnPlane
        (
            _requestedRotation * Vector3.forward,
            motor.CharacterUp
        );

        if (forward != Vector3.zero)
            currentRotation = Quaternion.LookRotation(forward, motor.CharacterUp);
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
        // Set character state
        _tempState = _state;

        // Crouch
        if (_requestedCrouch && _state.Stance is Stance.Stand)
        {
            _state.Stance = Stance.Crouch;

            motor.SetCapsuleDimensions
            (
                radius: motor.Capsule.radius,
                height: crouchHeight,
                yOffset: crouchHeight * 0.5f
            );
        }
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        // Uncrouch
        if (!_requestedCrouch && _state.Stance is not Stance.Stand)
        {
            // Test standing up the character to see if we overlap with colliders
            motor.SetCapsuleDimensions
            (
                radius: motor.Capsule.radius,
                height: standHeight,
                yOffset: standHeight* 0.5f
            );

            // See if capsule overlaps any colliders
            var pos = motor.TransientPosition;
            var rot = motor.TransientRotation;
            var mask = motor.CollidableLayers;

            // We cannot stand up, we'd be overlapping with colliders
            if (motor.CharacterOverlap(pos, rot, _uncrouchOverlapResults, mask, QueryTriggerInteraction.Ignore) > 0)
            {
                _requestedCrouch = true;
                motor.SetCapsuleDimensions
                (
                    radius: motor.Capsule.radius,
                    height: crouchHeight,
                    yOffset: crouchHeight * 0.5f
                );
            }
            // We can stand up
            else
            {
                _state.Stance = Stance.Stand;
            }
        }

        _state.Grounded = motor.GroundingStatus.IsStableOnGround;
        _state.Velocity = motor.Velocity;

        _lastState = _tempState;
    }

    public void PostGroundingUpdate(float deltaTime)
    {
        // If we're sliding, then end up in the air, go to a crouch
        if (!motor.GroundingStatus.IsStableOnGround && _state.Stance is Stance.Slide)
            _state.Stance = Stance.Crouch;
    }


    public bool IsColliderValidForCollisions(Collider coll) => true;

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }



    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
    }

    public Transform GetCameraTarget() => cameraTarget;

    public void SetPosition(Vector3 position, bool killVelocity = true)
    {
        motor.SetPosition(position);
        if (killVelocity)
            motor.BaseVelocity = Vector3.zero;
    }

    public CharacterState GetState() => _state;
    public CharacterState GetLastState() => _lastState;
}
