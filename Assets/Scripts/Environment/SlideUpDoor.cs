using UnityEngine;

/// <summary>
/// SlideUpDoor
/// A custom environment script that slides the door upwards
/// </summary>
public class SlideUpDoor : MonoBehaviour
{
    // Inspector Variables
    [SerializeField] private float openSpeed = 2.0f;

    // Private Variables
    private Vector3 openOffset = new Vector3(0, 4, 0);
    private Vector3 openPosition;

    private bool isOpen = false;

    // Private Functions
    private void Start()
    {
        openPosition = transform.position + openOffset;
    }

    private void Update()
    {
        Vector3 target = isOpen ? openPosition : transform.position;
        transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * openSpeed);
    }

    // Public Functions
    public void OpenDoor() => isOpen = true;
}
