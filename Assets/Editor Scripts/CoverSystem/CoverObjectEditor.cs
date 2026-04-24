using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CoverObject))]
public class CoverObjectEditor : Editor
{
	private bool overrideExistingCoverPoints;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        CoverObject coverObject = (CoverObject)target;

		EditorGUILayout.Space(10);

		EditorGUILayout.LabelField("CoverPoint Actions", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;

		overrideExistingCoverPoints = EditorGUILayout.Toggle("Override Existing Cover Points", overrideExistingCoverPoints);

		if (GUILayout.Button("Auto Generate Cover Points"))
			coverObject.AutoGenerateCoverPoints(overrideExistingCoverPoints);

		if (GUILayout.Button("Create Cover Point"))
            coverObject.CreateNewCoverPoint(coverObject.transform.position);

		if (GUILayout.Button("Remove Last Cover Point"))
			coverObject.RemoveLastCoverPoint();

		if (GUILayout.Button("Update Cover Points"))
			coverObject.UpdateCoverPoint();

		if (GUILayout.Button("Clear All Cover Points"))
			coverObject.ClearAllCoverPoints(true);

		EditorGUI.indentLevel--;
	}
}
