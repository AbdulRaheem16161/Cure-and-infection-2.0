using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RandomAreaMoveManager))]
public class RandomAreaMoveManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RandomAreaMoveManager areaGizmos = (RandomAreaMoveManager)target;

        if (GUILayout.Button("Create Area Point"))
        {
			Undo.RecordObject(areaGizmos, "Create Patrol Point");
			areaGizmos.CreateNewAreaPoint();
			EditorUtility.SetDirty(areaGizmos);
		}
    }
}
