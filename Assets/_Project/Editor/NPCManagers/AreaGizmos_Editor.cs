using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AreaGizmos))]
public class AreaGizmoz_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AreaGizmos areaGizmos = (AreaGizmos)target;

        if (GUILayout.Button("Create Area Point"))
        {
            areaGizmos.CreateAreaPoint();
        }

    }
}
