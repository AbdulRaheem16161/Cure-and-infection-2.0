using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TrackGizmos))]
public class TrackGizmoz_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TrackGizmos TrackGizmos = (TrackGizmos)target;

        if (GUILayout.Button("Create Track Point"))
        {
            TrackGizmos.CreateAreaPoint();
        }

    }
}
