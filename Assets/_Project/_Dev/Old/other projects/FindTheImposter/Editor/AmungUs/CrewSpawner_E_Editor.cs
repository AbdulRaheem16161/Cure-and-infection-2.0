#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CrewSpawner_E))]
public class CrewSpawner_E_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CrewSpawner_E spawner = (CrewSpawner_E)target;

        GUILayout.Space(10);

        if (GUILayout.Button("🚀 Spawn a Crew Mate"))
        {
            spawner.SpawnACrewMate();
        }
    }
}
#endif
