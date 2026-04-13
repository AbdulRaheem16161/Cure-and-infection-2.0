using UnityEngine;
using UnityEditor;
using Game.MyNPC;

[CustomEditor(typeof(NPCSpawner))]
public class NPCSpawner_Editor : Editor
{
	private EntityDefinition Definition;
	private NPCSpawner.Teams team;
	private NPCStateMachine.MovementType moveType;

	public override void OnInspectorGUI()
    {
		serializedObject.Update();

		DrawDefaultInspector();

		EditorGUILayout.Space(10);
		NPCSpawner spawner = (NPCSpawner)target;

		#region Spawner Controls
		EditorGUILayout.LabelField("Spawner Controls", EditorStyles.boldLabel);
		if (GUILayout.Button("Clean Up Dead Npcs"))
		{
			if (!ApplicationPlaying()) return;
			spawner.CleanUpDeadNpcs();
		}

		if (GUILayout.Button("Clean Up All Npcs"))
		{
			if (!ApplicationPlaying()) return;
			spawner.CleanUpAllNpcs(false);
		}

		if (GUILayout.Button("Spawn Custom Npcs (again)"))
		{
			if (!ApplicationPlaying()) return;
			spawner.SpawnCustomNpcs();
		}
		#endregion

		EditorGUILayout.Space(10);

		#region Npc Spawning
		EditorGUILayout.LabelField("Npc Spawning", EditorStyles.boldLabel);

		Definition = (EntityDefinition)EditorGUILayout.ObjectField("Npc Definition", Definition, typeof(EntityDefinition), true);
		team = (NPCSpawner.Teams)EditorGUILayout.EnumPopup("Npcs Team", team);
		moveType = (NPCStateMachine.MovementType)EditorGUILayout.EnumPopup("Movement Type", moveType);

		if (GUILayout.Button("Spawn Npc Based On Definition"))
		{
			if (!ApplicationPlaying()) return;
			spawner.SpawnSpecifiedNpc(Definition, team, moveType);
		}

		EditorGUILayout.Space(10);

		if (GUILayout.Button("Spawn Random Survivor Npc"))
		{
			if (!ApplicationPlaying()) return;
			spawner.SpawnRandomSurvivorNpc(team, moveType);
		}

		if (GUILayout.Button("Spawn Random Zombie Npc"))
		{
			if (!ApplicationPlaying()) return;
			spawner.SpawnRandomZombieNpc(team, moveType);
		}
		#endregion

		EditorGUILayout.Space(10);

		#region Patrol Path and Spawn Point Creation
		EditorGUILayout.LabelField("Patrol Path and Spawn Point Creation", EditorStyles.boldLabel);
		if (GUILayout.Button("Create New Patrol Path"))
		{
			Undo.RecordObject(spawner, "Create New Patrol Path");
			spawner.CreateNewPatrolPointPath();
			EditorUtility.SetDirty(spawner);
		}

		if (GUILayout.Button("Create New Spawn Point"))
		{
			Undo.RecordObject(spawner, "Create New Spawn Point");
			spawner.CreateNewSpawnPoint();
			EditorUtility.SetDirty(spawner);
		}
		#endregion

		serializedObject.ApplyModifiedProperties();
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
