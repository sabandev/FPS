using System.Collections;
using UnityEngine;
using UnityEngine.Animations;

/// <summary>
/// Idle
/// Animate ACTION
/// Plays a simple idle animation
/// </summary>
[CreateAssetMenu(menuName = "GOAP/Actions/Animate/Idle")]
public class Idle : Animate
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
    public override bool PreAction(AI AI)
    {
        AI.StartCoroutine(LookAtRoutine(AI));

        return base.PreAction(AI);
    }

    public override bool DuringAction(AI AI)
    {
        if (running)
            _idleTimer += Time.deltaTime;
        
        if (_idleTimer > idleDuration)
        {
            AI.StopAllCoroutines();
            _idleTimer = 0.0f;
            _hasAnimated = true;
        }

        return true;
    }

    public override bool PostAction(AI AI)
    {
        return true;
    }

    private IEnumerator LookAtRoutine(AI AI)
    {
        CalculateLookDirection(AI);
        yield return AI.StartCoroutine(LookAt(AI, lookDirection));
        yield return new WaitForSeconds(timeBetweenLooks);
        AI.StartCoroutine(LookAtRoutine(AI));
    }

    private IEnumerator LookAt(AI AI, Vector3 dir)
    {
        dir.y = 0f;
        Quaternion lookRot = Quaternion.LookRotation(dir);

        // Rotate to face NavMeshLink
        float timer = 0f;
        float duration = rotationSpeed;
        while (timer < 1f)
        {
            AI.gameObject.transform.rotation = Quaternion.Slerp(AI.gameObject.transform.rotation, lookRot, timer / duration);
            //transform.rotation = lookRot;  FAST

            timer += Time.deltaTime;
            yield return null;
        }
    }

    private void CalculateLookDirection(AI AI)
    {
        float randomAngle = Random.Range(minAngle, maxAngle);
        Vector3 lookDir = Quaternion.Euler(0.0f, randomAngle, 0.0f) * AI.gameObject.transform.forward;

        lookDirection = lookDir;
    }
}