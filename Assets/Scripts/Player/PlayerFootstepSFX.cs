using UnityEngine;

public class PlayerFootstepSFX : MonoBehaviour
{
    public AudioClip[] footstepSounds;
    [Range(0.0f, 1.0f)] public float volume = 0.45f;
}
