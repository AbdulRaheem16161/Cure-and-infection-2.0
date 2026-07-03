using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using static Unity.VisualScripting.Member;
using static UnityEngine.GraphicsBuffer;


/// <summary>
/// base AudioHandler class for all audios, should be easily extendable for sounds that are more persistant
/// (one shot sounds: gunshots, reloading, explosions, car crashing etc...)
/// (persistant sounds: footsteps, vehicle engine, ambience, music, breathing etc... )
/// 
/// for example can be extended with a the following:
/// 
/// public class VehicleAudioHandler : MonoBehaviour
/// {
///		[SerializeField] private AudioSource engineSource;
///		[SerializeField] private AudioSource doorSource;
///		
///		[SerializeField] private AudioClip engineIdle;
///		[SerializeField] private AudioClip doorOpen;
///	}
/// </summary>

public class AudioHandler : MonoBehaviour
{
	private AudioSource primaryAudioSource;

	private Collider[] npcColliders = new Collider[100];

	private void Awake()
	{
		primaryAudioSource = GetComponent<AudioSource>();

		if (primaryAudioSource == null)
			Debug.LogError("AudioSource component null, add component");
	}

	#region play audio methods (TODO: pass in max range that npcs can hear sound at)
	/// <summary>
	/// play audio and replace existing audio clip
	/// </summary>
	public void PlayAudio(AudioClip audioClip)
	{
		primaryAudioSource.clip = audioClip;
		primaryAudioSource.Play();
		NotifyNpcsOfSound();
		StartCoroutine(CleanUpAudio());
	}

	#endregion

	/// <summary>
	/// region below works okay for now but needs to be changed for final (footsteps and gunshots are heard at the same distance currently)
	/// to do that easiest way would be a SoundDefinition scriptable object storing the audioClip + max distance (any other variables if needed)
	/// 
	/// public class : SoundDefinition : ScriptableObjects
	/// {
	///		public AudioClip audioClip;
	///		public int priority;
	///		public float spacialBlend;
	///		etc...
	/// }
	/// 
	/// public void PlayAudio(SoundDefinition soundDefinition)
	///	{
	///		primaryAudioSource.clip = soundDefinition.audioClip;
	///		primaryAudioSource.maxDistance = soundDefinition.maxDistance;
	///		primaryAudioSource.Play();
	///		NotifyNpcsOfSound();
	///		StartCoroutine(CleanUpAudio());
	///	}
	///	
	/// for more control and tuning same as above but make a simple prefab containing an AudioSource for each sound definition.
	/// tweak values there instead so SoundDefinition is just
	/// public class : SoundDefinition : ScriptableObjects
	/// {
	///		public GameObject audioSource;
	/// }
	/// </summary>
	#region tell npcs about this sound
	/// <summary>
	/// find all npcs within audio sources maxDistance, call detect sound for npc investigate state
	/// </summary>

	private void NotifyNpcsOfSound()
	{
		int foundNpcs = Physics.OverlapSphereNonAlloc(transform.position, primaryAudioSource.maxDistance, npcColliders);

		for (int i = 0; i < foundNpcs; i++)
		{
			if (npcColliders[i].TryGetComponent(out NpcPerception npc))
			{
				npc.InvestigateSound(transform.position);
			}
		}
	}
	#endregion

	#region stop audio methods
	public void StopAudio()
	{
		primaryAudioSource.Stop();
	}
	#endregion

	#region clean up audio after play
	private IEnumerator CleanUpAudio()
	{
		yield return new WaitWhile(() => primaryAudioSource.isPlaying);
		AudioManager.Instance.CleanUpAudioObject(this);
	}
	#endregion
}
