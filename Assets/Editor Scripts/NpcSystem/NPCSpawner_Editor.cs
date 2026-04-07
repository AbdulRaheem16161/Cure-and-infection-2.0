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
		EditorGUILayout.LabelField("Spawner Controls", EditorStyles.boldLabel);

		npcDefinition = (NpcDefinition)EditorGUILayout.ObjectField("Npc Definition", npcDefinition, typeof(NpcDefinition), true);
		team = (NPCSpawner.Teams)EditorGUILayout.EnumPopup("Npcs Team", team);
		moveType = (NPCStateMachine.MovementType)EditorGUILayout.EnumPopup("Movement Type", moveType);

		EditorGUILayout.Space(5);

		NPCSpawner spawner = (NPCSpawner)target;

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
