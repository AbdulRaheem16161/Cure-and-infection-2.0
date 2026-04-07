using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PatrolPathManager))]
public class PatrolPathManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

		PatrolPathManager TrackGizmos = (PatrolPathManager)target;

        if (GUILayout.Button("Create Track Point"))
        {
            TrackGizmos.CreateAreaPoint();
        }

    }
}
