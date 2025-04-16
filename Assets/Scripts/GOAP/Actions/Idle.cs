using System.Collections;
using UnityEngine;

/// <summary>
/// Idle
/// Animate ACTION
/// Plays a simple idle animation
/// </summary>
public class Idle : GOAP_ACTION_Animate
{
    // Inspector Variables
    [SerializeField] private float idleDuration = 3.0f;
    [SerializeField] private float timeBetweenLooks = 1.5f;
    [SerializeField] private float rotationSpeed = 2.0f;
    [SerializeField] private float minAngle = 45.0f;
    [SerializeField] private float maxAngle = 180.0f;

    // Private Variables
    private Vector3 lookDirection;

    private float _idleTimer = 0.0f;

    // Override Functions
    public override bool PreAction()
    {
        StartCoroutine(LookAtRoutine());

        return base.PreAction();
    }

    public override bool DuringAction()
    {
        if (running)
            _idleTimer += Time.deltaTime;
        
        if (_idleTimer > idleDuration)
        {
            StopAllCoroutines();
            _idleTimer = 0.0f;
            _hasAnimated = true;
        }

        return true;
    }

    private void Update()
    {
        if (_idleTimer > idleDuration)
            Debug.Log("beyond idle");
    }

    public override bool PostAction()
    {
        return true;
    }

    private IEnumerator LookAtRoutine()
    {
        CalculateLookDirection();
        yield return StartCoroutine(LookAt(lookDirection));
        yield return new WaitForSeconds(timeBetweenLooks);
        StartCoroutine(LookAtRoutine());
    }

    private IEnumerator LookAt(Vector3 dir)
    {
        dir.y = 0f;
        Quaternion lookRot = Quaternion.LookRotation(dir);

        // Rotate to face NavMeshLink
        float timer = 0f;
        float duration = rotationSpeed;
        while (timer < 1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, timer / duration);
            //transform.rotation = lookRot;  FAST

            timer += Time.deltaTime;
            yield return null;
        }
    }

    private void CalculateLookDirection()
    {
        float randomAngle = Random.Range(minAngle, maxAngle);
        Vector3 lookDir = Quaternion.Euler(0.0f, randomAngle, 0.0f) * transform.forward;

        lookDirection = lookDir;
    }
}