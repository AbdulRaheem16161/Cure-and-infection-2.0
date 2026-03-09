using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;


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

	private float radius = 30;

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
	public void PlayAudio(AudioClip clip)
	{
		primaryAudioSource.clip = clip;
		primaryAudioSource.Play();
		NotifyNpcsOfSound();
		StartCoroutine(CleanUpAudio());
	}

	#endregion

	#region tell npcs about this sound
	private void NotifyNpcsOfSound()
	{
		int foundNpcs = Physics.OverlapSphereNonAlloc(transform.position, radius, npcColliders);

		for (int i = 0; i < foundNpcs; i++)
		{
			if (npcColliders[i].TryGetComponent(out NpcController npc))
			{
				npc.StateMachine.DetectSound(transform.position);
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
