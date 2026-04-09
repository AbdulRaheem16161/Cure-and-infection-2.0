using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PatrolPathManager))]
public class PatrolPathManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

		PatrolPathManager patrolPathManager = (PatrolPathManager)target;

        if (GUILayout.Button("Create Track Point"))
        {
			Undo.RecordObject(patrolPathManager, "Create Patrol Point");
			patrolPathManager.CreateNewPatrolPoint();
			EditorUtility.SetDirty(patrolPathManager);
		}
    }
}
