using System.Collections;
using UnityEngine;

public class CameraHeadBob : MonoBehaviour
{
    [SerializeField] private bool doHeadbob = true;
    [SerializeField] private float amplitude = 0.015f;
    [SerializeField] private float walkingFrequency = 10.0f;
    [SerializeField] private float crouchingFrequency = 8.0f;
    [SerializeField] private float verticalMultiplier = 2.0f;
    [SerializeField] private float horizontalMultiplier = 2.0f;
    [SerializeField] private float crouchMultiplier = 0.5f;
    [SerializeField] private float headbobSpeedThreshold = 3.0f;

    private Vector3 _startPos;

    public void Initialise()
    {
        _startPos = Vector3.zero;
    }

    public void UpdateHeadbob(Vector3 characterVelocity, bool grounded, bool crouching)
    {
        if (!doHeadbob) { return; }

        float speed = new Vector3(characterVelocity.x, 0, characterVelocity.z).magnitude;
        if (speed < headbobSpeedThreshold) { return; }

        ResetPosition();

        if (grounded)
            PlayMotion(FootStepMotion(crouching));

        ResetPosition();
    }

    private void PlayMotion(Vector3 motion)
    {
        transform.localPosition += motion;
    }

    public void ResetPosition()
    {
        transform.localPosition = Vector3.Slerp
        (
            a: transform.localPosition,
            b: _startPos,
            t: 1.0f - Mathf.Exp(-20 * Time.deltaTime)
        );
    }

    private Vector3 FootStepMotion(bool crouching)
    {
        Vector3 pos = Vector3.zero;
        float effectiveFrequency = crouching ? crouchingFrequency : walkingFrequency;

        float effectiveVerticalMultiplier = crouching ? verticalMultiplier * crouchMultiplier : verticalMultiplier;
        pos.y += Mathf.Sin(Time.time * effectiveFrequency) * amplitude * effectiveVerticalMultiplier;

        float effectiveHorizontalMultiplier = crouching ? horizontalMultiplier * crouchMultiplier : horizontalMultiplier;
        pos.x += Mathf.Cos(Time.time * effectiveFrequency) * amplitude * effectiveHorizontalMultiplier;
        return pos;
    }
}
