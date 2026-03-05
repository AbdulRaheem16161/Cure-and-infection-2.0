using Game.MyNPC;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StatsHandler))]
public class StatsHandlerEditor : Editor
{
	private bool showDebugControls;

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		StatsHandler stats = (StatsHandler)target;

		GUILayout.Space(10);
		GUILayout.Label("DEBUG CONTROLS", EditorStyles.boldLabel);
		showDebugControls = EditorGUILayout.Toggle("Show Debug Controls", showDebugControls);

		if (!showDebugControls) return;

		#region deal damage and kill 
		if (GUILayout.Button("Damage Npc (25)"))
		{
			if (!ApplicationPlaying()) return;

			stats.RecieveDamage(25);
		}
		if (GUILayout.Button("Kill Npc"))
		{
			if (!ApplicationPlaying()) return;

			stats.RecieveDamage(stats.health + 1);
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
