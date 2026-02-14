//using UnityEditor;
//using UnityEngine;

//[CustomEditor(typeof(NPCSpawner))]
//public class NPCSpawnerEditor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        DrawDefaultInspector();
//        var spawner = (NPCSpawner)target;

//        if (GUILayout.Button("Spawn Pedestrian")) spawner.Spawn("Pedestrian");
//        if (GUILayout.Button("Spawn Guard")) spawner.Spawn("Guard");
//        if (GUILayout.Button("Spawn Zombie")) spawner.Spawn("Zombie");
//    }
//}
