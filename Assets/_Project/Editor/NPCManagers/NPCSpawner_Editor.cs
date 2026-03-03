using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NPCSpawner))]
public class NPCSpawner_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NPCSpawner spawner = (NPCSpawner)target;

        if (GUILayout.Button("Spawn Npc Based On Definition"))
        {
            if (!ApplicationPlaying()) return;
            spawner.SpawnNPC(spawner.npcDefinitionToSpawn);
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
