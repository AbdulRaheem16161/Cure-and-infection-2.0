using Game.MyNPC;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StatsHandler))]
public class StatsHandlerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		StatsHandler stats = (StatsHandler)target;

		GUILayout.Space(10);
		GUILayout.Label("DEBUG CONTROLS", EditorStyles.boldLabel);
		stats.showControls = EditorGUILayout.Toggle("Show Controls", stats.showControls);

		if (!stats.showControls) return;

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
