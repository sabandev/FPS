using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    // Private Properties
    private bool _requestedShoot;

    public void Initialise()
    {
        _requestedShoot = false;
    }

    public void UpdateInput(CharacterInput characterInput)
    {
        _requestedShoot = characterInput.Shoot;
    }

    public void UpdateWeapon()
    {
        if (_requestedShoot)
            Debug.Log("Shot");
    }
}
