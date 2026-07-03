using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Door))]
public class DoorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Door door = (Door)target;

        GUILayout.Space(10);
        GUILayout.Label("DEBUG Interacts", EditorStyles.boldLabel);

        if (GUILayout.Button("Press Interact"))
        {
            if (!ApplicationPlaying()) return;

            door.InteractPress(null);
        }
    }

    private bool ApplicationPlaying()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Must be in Play Mode");
            return false;
        }

        return true;
    }
}
