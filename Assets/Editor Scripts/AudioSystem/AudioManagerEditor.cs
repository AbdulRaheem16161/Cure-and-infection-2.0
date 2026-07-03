using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AudioManager))]
public class AudioManagerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		AudioManager audioManager = (AudioManager)target;

		GUILayout.Space(10);
		GUILayout.Label("DEBUG CONTROLS", EditorStyles.boldLabel);

		#region use consumable in equipment slot button
		GUILayout.Label("Play Audio Clip At World Position", EditorStyles.boldLabel);
		audioManager.positionToSpawnAt = EditorGUILayout.Vector3Field("Position To Spawn At", audioManager.positionToSpawnAt);
		audioManager.audioToPlay = (AudioClip)EditorGUILayout.ObjectField("Audio To Play", audioManager.audioToPlay, typeof(AudioClip), false);

		if (GUILayout.Button("Play Audio"))
		{
			if (!ApplicationPlaying()) return;

			AudioManager.PlayAudio(audioManager.positionToSpawnAt, audioManager.audioToPlay);
		}
		#endregion
	}

	private bool ApplicationPlaying()
	{
		if (!Application.isPlaying)
		{
			Debug.LogWarning("Must be in Play Mode");
			return false;
		}

		return true;
	}
}
