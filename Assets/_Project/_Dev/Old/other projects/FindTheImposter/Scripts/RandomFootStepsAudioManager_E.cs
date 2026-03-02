using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomFootstepsAudioManager_E : MonoBehaviour
{
    [Tooltip("Assign the AudioSource used for footstep playback.")]
    [SerializeField] private AudioSource footstepAudioSource;

    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private float timeBetweenSteps = 0.4f;
    [SerializeField] private float minVolume = 0.7f;
    [SerializeField] private float maxVolume = 1f;
    [SerializeField] private float minPitch = 0.95f;
    [SerializeField] private float maxPitch = 1.05f;

    [Header("3D Audio Settings")]
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 25f;

    [Header("Editor Testing")]
    [SerializeField] private bool testPlayback = false;

    private float nextStepTime;

    private void Awake()
    {
        if (footstepAudioSource == null)
        {
            AudioSource[] sources = GetComponents<AudioSource>();
            footstepAudioSource = sources.Length > 1 ? sources[0] : GetComponent<AudioSource>();
        }

        Apply3DAudioSettings();
    }

    private void Update()
    {
        Apply3DAudioSettings();

        if (testPlayback)
        {
            testPlayback = false; // Optional: only play once
            PlayRandomFootstep();
        }
    }

    private void Apply3DAudioSettings()
    {
        footstepAudioSource.spatialBlend = 1f;
        footstepAudioSource.minDistance = minDistance;
        footstepAudioSource.maxDistance = maxDistance;
    }

    public void PlayRandomFootstep()
    {
        if (footstepClips.Length == 0 || Time.time < nextStepTime) return;

        int index = Random.Range(0, footstepClips.Length);
        footstepAudioSource.clip = footstepClips[index];
        footstepAudioSource.volume = Random.Range(minVolume, maxVolume);
        footstepAudioSource.pitch = Random.Range(minPitch, maxPitch);
        footstepAudioSource.Play();

        nextStepTime = Time.time + timeBetweenSteps;
    }
}
