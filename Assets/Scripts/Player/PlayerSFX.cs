using UnityEngine;

public class PlayerSFX : MonoBehaviour
{
    [SerializeField] private AudioSource sfxSource;

    [Space]

    [Header("Footstep sounds")]
    [SerializeField] private PlayerFootstepSFX footstepSFX;
    [SerializeField] private float walkStepInterval = 0.65f;
    [SerializeField] private float speedThreshold = 4.0f;
    [SerializeField] private float footstepSoundRadius = 10.0f;

    [Space]

    [Header("Jump sounds")]
    [SerializeField] private PlayerJumpSFX jumpSFX;
    [SerializeField] private float jumpSoundRadius = 7.5f;

    private int _lastPlayedFootstepIndex = -1;
    private int _lastPlayedJumpIndex = -1;
    private float _nextStepTime;

    public void Initialise()
    {

    }

    public void UpdateSFX(Transform playerPosition, Vector3 moveInput, Vector3 velocity, bool lastGrounded, bool inputJump, bool crouching)
    {
        if (moveInput != Vector3.zero && Time.time > _nextStepTime && lastGrounded == true && velocity.magnitude > speedThreshold)
        {
            PlayFootstepSounds(playerPosition, crouching);
            _nextStepTime = Time.time + walkStepInterval;
        }

        if (inputJump && velocity.y > 0.0f)
        {
            PlayJumpSound(playerPosition);
        }
    }

    private void PlayFootstepSounds(Transform playerPosition, bool crouching)
    {
        if (crouching) { return; }

        if (footstepSFX == null || footstepSFX.footstepSounds == null || footstepSFX.footstepSounds.Length == 0)
        {
            Debug.LogWarning("WARNING: No foostep sounds (or script) assigned. Cannot play null footstep sounds.");
            return;
        }

        int randomIndex;
        if (footstepSFX.footstepSounds.Length == 1)
            randomIndex = 0;
        else
        {
            randomIndex = Random.Range(0, footstepSFX.footstepSounds.Length - 1);
            if (randomIndex >= _lastPlayedFootstepIndex)
                randomIndex++;
        }

        _lastPlayedFootstepIndex = randomIndex;
        sfxSource.clip = footstepSFX.footstepSounds[randomIndex];

        if (!sfxSource.isPlaying)
        {
            sfxSource.volume = footstepSFX.volume;
            sfxSource.Play();
            GOAP_World.EmitSound(SoundType.Player, playerPosition.position, footstepSoundRadius, playerPosition.gameObject);
        }
    }

    private void PlayJumpSound(Transform playerPosition)
    {
        if (jumpSFX == null || jumpSFX.jumpSounds == null || jumpSFX.jumpSounds.Length == 0)
        {
            Debug.LogWarning("WARNING: No jump sounds (or script) assigned. Cannot play null jump sounds.");
            return;
        }

        int randomIndex;
        if (jumpSFX.jumpSounds.Length == 1)
            randomIndex = 0;
        else
        {
            randomIndex = Random.Range(0, jumpSFX.jumpSounds.Length - 1);

            if (randomIndex > _lastPlayedJumpIndex)
                randomIndex++;
        }

        _lastPlayedJumpIndex = randomIndex;
        sfxSource.clip = jumpSFX.jumpSounds[randomIndex];

        if (!sfxSource.isPlaying)
        {
            sfxSource.volume = jumpSFX.volume;
            sfxSource.Play();
            GOAP_World.EmitSound(SoundType.Player, playerPosition.position, jumpSoundRadius, playerPosition.gameObject);
        }
    }
}
