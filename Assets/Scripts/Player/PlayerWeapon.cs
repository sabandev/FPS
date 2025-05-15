using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    // Inspector Properties
    [SerializeField] private bool doWeaponSway = true;
    [SerializeField] private float positionalSway = 0.1f;
    [SerializeField] private float rotationalSway = 0.1f;
    [SerializeField] private float weaponSwaySmoothness = 1.0f;

    // Private Properties
    private Vector3 initialPosition = Vector3.zero;
    private Quaternion initialRotation = Quaternion.identity;

    private float _lookX;
    private float _lookY;

    private bool _requestedShoot;

    public void Initialise()
    {
        _requestedShoot = false;

        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    public void UpdateInput(CharacterInput characterInput, CameraInput cameraInput)
    {
        _requestedShoot = characterInput.Shoot;

        _lookX = cameraInput.Look.x;
        _lookY = cameraInput.Look.y;
    }

    public void UpdateWeapon()
    {
        if (_requestedShoot)
            Debug.Log("Shot");

        ApplyWeaponSway();
    }

    private void ApplyWeaponSway()
    {
        Vector3 positionOffset = new Vector3(_lookX, _lookY, 0.0f) * positionalSway;
        Quaternion rotationOffset = Quaternion.Euler(new Vector3(-_lookY, -_lookX, _lookX) * rotationalSway);

        transform.localPosition = Vector3.Lerp(transform.localPosition, initialPosition - positionOffset, Time.deltaTime * weaponSwaySmoothness);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, initialRotation * rotationOffset, Time.deltaTime * weaponSwaySmoothness);
    }
}
