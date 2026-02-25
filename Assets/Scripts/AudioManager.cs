using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public GameObject audioSourcePrefab;
    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySound(AudioClip clip)
    {
        if (audioSourcePrefab != null)
        {
            GameObject audioSourceObject = Instantiate(audioSourcePrefab, transform);
            AudioSource audioSource = audioSourceObject.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
                Destroy(audioSourceObject, clip.length);
            }
        }
        else
        {
            Debug.LogWarning("AudioSource prefab is not assigned.");
        }
    }
}
