using UnityEngine;
using UnityEditor;
using Game.MyNPC;

[CustomEditor(typeof(NPCSpawner))]
public class NPCSpawner_Editor : Editor
{
	private NpcDefinition npcDefinition;
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
			spawner.CleanUpAllNpcs();
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

		npcDefinition = (NpcDefinition)EditorGUILayout.ObjectField("Npc Definition", npcDefinition, typeof(NpcDefinition), true);
		team = (NPCSpawner.Teams)EditorGUILayout.EnumPopup("Npcs Team", team);
		moveType = (NPCStateMachine.MovementType)EditorGUILayout.EnumPopup("Movement Type", moveType);

		if (GUILayout.Button("Spawn Npc Based On Definition"))
		{
			if (!ApplicationPlaying()) return;
			spawner.SpawnSpecifiedNpc(npcDefinition, team, moveType);
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
			GameObject patrolPath = (GameObject)PrefabUtility.InstantiatePrefab(spawner.patrolPathPrefab);
			patrolPath.transform.SetParent(spawner.movementAreaAndPathsParent.transform);
			patrolPath.transform.position = spawner.transform.position;
			PatrolPathManager patrolPathManager = patrolPath.GetComponent<PatrolPathManager>();
			spawner.PatrolPaths.Add(patrolPathManager);
			patrolPath.name += $"{spawner.movementAreaAndPathsParent.transform.childCount - 1}"; //-1 due to area move manager
			Selection.activeGameObject = patrolPath;
		}

		if (GUILayout.Button("Create New Spawn Point"))
		{
			GameObject spawnPoint = (GameObject)PrefabUtility.InstantiatePrefab(spawner.spawnPointPrefab);
			spawnPoint.transform.SetParent(spawner.spawnPointsParent.transform);
			spawnPoint.transform.position = spawner.transform.position;
			spawnPoint.name += $"{spawner.spawnPointsParent.transform.childCount}";
			Selection.activeGameObject = spawnPoint;
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
