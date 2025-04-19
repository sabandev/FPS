using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;

/// <summary>
/// SlideUpDoor
/// A custom environment script that slides the door upwards
/// </summary>
public class SlideUpDoor : MonoBehaviour
{
    // Public Variables
    public float duration = 2.0f;

    // Private Variables
    private Vector3 openPosition;

    // Public Functions
    public void OpenDoor() => StartCoroutine(OpenDoorCoroutine());

    // Private Functions
    private void Start()
    {
        openPosition = transform.position + new Vector3(0.0f, 4.0f, 0.0f);
    }

    private IEnumerator OpenDoorCoroutine()
    {
        float timer = 0.0f;
        while (timer < duration)
        {
            transform.position = Vector3.Lerp(transform.position, openPosition, timer / duration * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        gameObject.GetComponentInChildren<NavMeshLink>().activated = false;
    }
}
