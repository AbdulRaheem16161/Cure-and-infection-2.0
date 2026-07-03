using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicAudioHandler : AudioHandler
{
	private readonly float maxVolume = 0.25f;
	private readonly float fadeTime = 5f;

	public bool disable;

	[Header("Track One")]
	public AudioSource musicTrackOne;

	[Header("Track Two")]
	public AudioSource musicTrackTwo;

	[Header("Music Clip Queue")]
	public AudioClip[] musicClips;
	private Queue<AudioClip> MusicClips = new();

	[Header("Current Playing Track")]
	public AudioSource currentPlayingAudioSource;

	private void Awake()
	{
		MusicClips.Clear();
		foreach (AudioClip clip in musicClips)
			MusicClips.Enqueue(clip);
	}

	private void Start()
	{
		if (disable) return;
		StartCoroutine(PlayTrackCoroutine(musicTrackOne, musicTrackTwo));
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.C))
		{
			DebugSkipToFade();
			Debug.LogError("skipped to fade out start");
		}
	}

	private IEnumerator PlayTrackCoroutine(AudioSource current, AudioSource next)
	{
		AudioClip clip = GetNextMusicClip();
		current.clip = clip;
		current.volume = 0f;
		current.Play();
		currentPlayingAudioSource = current;

		float waitTime = clip.length - fadeTime;

		//fade in/out audio tracks
		StartCoroutine(FadeAudio(next, 0f, fadeTime));
		StartCoroutine(FadeAudio(current, maxVolume, fadeTime));

		//wait for fade time start
		yield return new WaitForSeconds(waitTime);

		next.clip = GetNextMusicClip();
		next.volume = 0f;
		next.Play();
		currentPlayingAudioSource = next;

		//fade in/out audio tracks
		StartCoroutine(FadeAudio(current, 0f, fadeTime));
		StartCoroutine(FadeAudio(next, maxVolume, fadeTime));

		//wait for fate time to end
		yield return new WaitForSeconds(fadeTime);

		current.Stop();

		//schedule next track recursively
		StartCoroutine(PlayTrackCoroutine(next, current));
	}

	private IEnumerator FadeAudio(AudioSource audioSource, float targetVolume, float duration)
	{
		float startVolume = audioSource.volume;
		float timer = 0f;

		while (timer < duration)
		{
			timer += Time.deltaTime;
			audioSource.volume = Mathf.Lerp(startVolume, targetVolume, timer / duration);
			yield return null;
		}

		audioSource.volume = targetVolume;
	}

	public void DebugSkipToFade()
	{
		StopAllCoroutines();

		//determine the next track to fade in
		AudioSource nextTrack = currentPlayingAudioSource == musicTrackOne ? musicTrackTwo : musicTrackOne;
		AudioSource currentTrack = currentPlayingAudioSource;

		//set track time just before fade start time
		if (currentTrack.clip != null)
			currentTrack.time = Mathf.Max(0, currentTrack.clip.length - fadeTime - 1f);

		//start crossfade
		StartCoroutine(PlayTrackCoroutine(nextTrack, currentTrack));
	}

	public AudioClip GetNextMusicClip()
	{
		AudioClip clip = MusicClips.Dequeue();
		MusicClips.Enqueue(clip);
		return clip;
	}
}
