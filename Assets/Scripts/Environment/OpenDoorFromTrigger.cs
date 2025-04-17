using UnityEngine;

/// <summary>
/// OpenDoor
/// A custom environment script that opens a door when the AI enters the gameObject's trigger
/// </summary>
public class OpenDoorFromTrigger : MonoBehaviour
{
    // Inspector Variables
    [SerializeField] private SlideUpDoor door;

    // Private Functions
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("AI"))
        {
            door.OpenDoor();
        }
    }
}
