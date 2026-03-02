using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerSpawner))]
public class AiSpawner_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlayerSpawner spawner = (PlayerSpawner)target;

        if (GUILayout.Button("Spawn Player"))
        {
            spawner.SpawnPlayer();
        }
    }
}

