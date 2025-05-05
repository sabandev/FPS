using UnityEngine;

public class PlayerJumpSFX : MonoBehaviour
{
    public AudioClip[] jumpSounds;
    [Range(0.0f, 1.0f)] public float volume = 0.9f;
}
