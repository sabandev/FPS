using System.Collections;
using UnityEngine;

public class CameraHeadBob : MonoBehaviour
{
    [SerializeField] private bool doHeadbob = true;
    [SerializeField] private float amplitude = 0.015f;
    [SerializeField] private float frequency = 10.0f;
    [SerializeField] private float startHeadbobSpeed = 3.0f;
    // [SerializeField] private Transform cameraTarget;

    // private bool _invoked = false;

    public void Initialise(Transform cameraTarget)
    {
        transform.position = cameraTarget.transform.position;
    }

    public void UpdateHeadbob(Transform target, Vector3 characterVelocity, bool grounded)
    {
        if (!doHeadbob) { return; }

        CheckMotion(target, characterVelocity, grounded);
    }

    private void CheckMotion(Transform cameraTarget, Vector3 characterVelocity, bool grounded)
    {
        float speed = new Vector3(characterVelocity.x, 0, characterVelocity.z).magnitude;

        if (speed < startHeadbobSpeed) { return; }
        if (!grounded) { return; }

        PlayMotion(FootStepMotion());
    }

    private void PlayMotion(Vector3 motion)
    {
        transform.position += motion;
    }

    public void ResetPosition(Transform target)
    {
        transform.position = target.position;
        // _invoked = false;
    }

    private Vector3 FootStepMotion()
    {
        Vector3 pos = Vector3.zero;
        pos.y += Mathf.Sin(Time.time * frequency) * amplitude;
        pos.x += Mathf.Cos(Time.time * frequency / 2) * amplitude * 2;
        return pos;
    }
}
