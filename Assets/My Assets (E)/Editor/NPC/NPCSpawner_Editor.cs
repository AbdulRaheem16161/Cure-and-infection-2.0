using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NPCSpawner))]
public class NPCSpawner_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NPCSpawner spawner = (NPCSpawner)target;

        if (GUILayout.Button("Spawn T-Rex"))
        {
            spawner.SpawnNPC("TRex");
        }
        if (GUILayout.Button("Spawn Guard"))
        {
            spawner.SpawnNPC("Guard");
        }
        if (GUILayout.Button("Spawn Zombie"))
        {
            spawner.SpawnNPC("Zombie");
        }
    }
}
