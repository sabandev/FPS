using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    // Inspector Properties
    [SerializeField] private bool doWeaponSway = true;
    [SerializeField] private float positionalSway = 0.1f;
    [SerializeField] private float rotationalSway = 0.1f;
    [SerializeField] private float weaponSwaySmoothness = 1.0f;

    [SerializeField] private bool doWeaponBob = true;
    [SerializeField] private float weaponBobbingSpeed = 5.0f;
    [SerializeField] private float weaponBobbingAmount = 5.0f;
    [SerializeField] private float weaponBobThreshold = 3.0f;

    [SerializeField] private float recoilAmount = 0.2f;
    [SerializeField] private float recoilSmoothness = 5.0f;
    [SerializeField] private float maxRecoil = 0.05f;

    // Private Properties
    private Vector3 initialPosition = Vector3.zero;
    private Vector3 currentRecoil = Vector3.zero;

    private Quaternion initialRotation = Quaternion.identity;

    private float _moveSpeed;

    private float _lookX;
    private float _lookY;

    private float _bobTimer;

    private bool _isRecoiling = false;
    private bool _requestedShoot;

    public void Initialise()
    {
        _requestedShoot = false;

        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    public void UpdateInput(CharacterState characterState, CharacterInput characterInput, CameraInput cameraInput)
    {
        _requestedShoot = characterInput.Shoot;

        _moveSpeed = characterState.Velocity.magnitude;

        _lookX = cameraInput.Look.x;
        _lookY = cameraInput.Look.y;
    }

    public void UpdateWeapon(bool grounded)
    {
        ApplyWeaponSway();
        ApplyWeaponBob(grounded);
        ApplyRecoil();

        if (_requestedShoot)
            _isRecoiling = true;

    }

    private void ApplyWeaponSway()
    {
        if (!doWeaponSway) { return; }

        Vector3 positionOffset = new Vector3(_lookX, _lookY, 0.0f) * positionalSway;
        Quaternion rotationOffset = Quaternion.Euler(new Vector3(-_lookY, -_lookX, _lookX) * rotationalSway);

        transform.localPosition = Vector3.Lerp(transform.localPosition, initialPosition - positionOffset, Time.deltaTime * weaponSwaySmoothness);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, initialRotation * rotationOffset, Time.deltaTime * weaponSwaySmoothness);
    }

    private void ApplyWeaponBob(bool grounded)
    {
        if (!doWeaponBob) { return; }

        float bobOffset = 0.0f;

        if (_moveSpeed > weaponBobThreshold && grounded)
        {
            _bobTimer += Time.deltaTime * weaponBobbingSpeed;
            bobOffset = Mathf.Sin(_bobTimer) * weaponBobbingAmount;
        }
        else
        {
            _bobTimer = 0.0f;
            bobOffset = Mathf.Lerp(_bobTimer, 0.0f, Time.deltaTime * weaponSwaySmoothness);
        }

        transform.localPosition += new Vector3(0.0f, bobOffset, 0.0f);
    }

    private void ApplyRecoil()
    {
        Vector3 targetRecoil = Vector3.zero;

        if (_isRecoiling)
        {
            targetRecoil = new Vector3(0.0f, 0.0f, -recoilAmount);

            if (Vector3.Distance(currentRecoil, targetRecoil) < maxRecoil)
            {
                _isRecoiling = false;
            }
        }

        currentRecoil = Vector3.Lerp(currentRecoil, targetRecoil, Time.deltaTime * recoilSmoothness);
        transform.localPosition += currentRecoil;
    }
}
