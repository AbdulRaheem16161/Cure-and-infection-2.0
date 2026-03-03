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

		

		#region deal damage and kill 
		if (GUILayout.Button("Damage Npc (25% of health)"))
		{
			if (!ApplicationPlaying()) return;

			stats.RecieveDamage(stats.NpcController.NpcDefinition.MaxHealth / 4);
		}
		if (GUILayout.Button("Kill Npc"))
		{
			if (!ApplicationPlaying()) return;

			stats.RecieveDamage(stats.NpcController.NpcDefinition.MaxHealth + 1);
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
