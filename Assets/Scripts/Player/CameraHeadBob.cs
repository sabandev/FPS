using System.Collections;
using UnityEngine;

public class CameraHeadBob : MonoBehaviour
{
    [SerializeField] private bool doHeadbob = true;
    [SerializeField] private float amplitude = 0.015f;
    [SerializeField] private float frequency = 10.0f;
    [SerializeField] private float verticalMultiplier = 2.0f;
    [SerializeField] private float horizontalMultiplier = 2.0f;
    [SerializeField] private float headbobSpeedThreshold = 3.0f;

    private Vector3 _startPos;

    public void Initialise()
    {
        _startPos = Vector3.zero;
    }

    public void UpdateHeadbob(Vector3 characterVelocity, bool grounded)
    {
        if (!doHeadbob) { return; }

        float speed = new Vector3(characterVelocity.x, 0, characterVelocity.z).magnitude;

        if (speed < headbobSpeedThreshold) { return; }

        ResetPosition();

        Debug.Log(speed);

        if (grounded)
            PlayMotion(FootStepMotion());

        ResetPosition();
    }

    private void PlayMotion(Vector3 motion)
    {
        transform.localPosition += motion;
    }

    public void ResetPosition()
    {
        transform.localPosition = Vector3.Lerp
        (
            a: transform.localPosition,
            b: _startPos,
            t: 1.0f - Mathf.Exp(-20 * Time.deltaTime)
        );
    }

    private Vector3 FootStepMotion()
    {
        Vector3 pos = Vector3.zero;
        pos.y += Mathf.Sin(Time.time * frequency) * amplitude * verticalMultiplier;
        pos.x += Mathf.Cos(Time.time * frequency) * amplitude * horizontalMultiplier;
        return pos;
    }
}
