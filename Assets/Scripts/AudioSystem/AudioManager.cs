using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	/// <summary>
	/// if switching to SoundDefinition, object pooling will need updating to dictionary, key = SoundDefinition, value =  Queue<GameObject>
	/// to group similar objects by sound definition so audioSources dont need instantiating and destroying on one shot sound objects.
	/// private Dictionary<SoundDefinition, Queue<GameObject>> pooledObjectsByDefinition = new Dictionary<SoundDefinition, Queue<GameObject>>();
	/// </summary>

	public GameObject audioPrefab;
	public static AudioManager Instance { get; private set; }

	public List<AudioHandler> audioHandlerObjectPooling = new();

	[Header("Debug Controls")]
	[HideInInspector] public Vector3 positionToSpawnAt;
	[HideInInspector] public AudioClip audioToPlay;

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
	#region create world audio
	public static void PlayAudio(Vector3 position, AudioClip audioClip)
	{
		AudioHandler audioHandler = Instance.TryGetAudioFromObjectPooling();

		audioHandler.gameObject.transform.SetParent(null);
		audioHandler.gameObject.transform.SetPositionAndRotation(position, Quaternion.identity);
		audioHandler.PlayAudio(audioClip);
	}
	#endregion

	#region audio handler object pooling
	public AudioHandler TryGetAudioFromObjectPooling()
	{
		if (audioHandlerObjectPooling.Count > 0)
		{
			AudioHandler audioHandler = audioHandlerObjectPooling[0];
			audioHandlerObjectPooling.RemoveAt(0);
			return audioHandler;
		}

		return Instantiate(audioPrefab, transform).GetComponent<AudioHandler>();
	}
	public void CleanUpAudioObject(AudioHandler audioHandler)
	{
		audioHandlerObjectPooling.Add(audioHandler);
		audioHandler.gameObject.transform.SetParent(transform);
		audioHandler.gameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
	}
	#endregion
}
