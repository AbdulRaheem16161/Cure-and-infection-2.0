using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyPatrolManager))]
public class EnemyPatrolManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EnemyPatrolManager manager = (EnemyPatrolManager)target;

        EditorGUILayout.Space();

        if (GUILayout.Button("Run Auto Setup"))
        {
            manager.AutoSetup();
            EditorUtility.SetDirty(manager); // Mark scene dirty so Unity saves changes
        }
    }
}
