using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NPCSpawner))]
public class NPCSpawner_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NPCSpawner spawner = (NPCSpawner)target;

		EditorGUILayout.Space(10);
		EditorGUILayout.LabelField("Spawner Controls", EditorStyles.boldLabel);
		spawner.NPCsTeam = (NPCSpawner.Teams)EditorGUILayout.EnumPopup("Npcs Team", spawner.NPCsTeam);
		spawner.npcDefinitionToSpawn = 
			(NpcDefinition)EditorGUILayout.ObjectField("Npc Definition", spawner.npcDefinitionToSpawn, typeof(NpcDefinition), true);

		if (GUILayout.Button("Spawn Npc Based On Definition"))
        {
            if (!ApplicationPlaying()) return;
            spawner.SpawnNPC(spawner.npcDefinitionToSpawn);
        }

		EditorGUILayout.Space(10);

		spawner.NPCsTeam = (NPCSpawner.Teams)EditorGUILayout.EnumPopup("Npcs Team", spawner.NPCsTeam);

		if (GUILayout.Button("Spawn Random Npc"))
		{
			if (!ApplicationPlaying()) return;
			spawner.SpawnRandomNPC(false);
		}
		if (GUILayout.Button("Spawn Random Zombie Npc"))
		{
			if (!ApplicationPlaying()) return;
			spawner.SpawnRandomNPC(true);
		}
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
