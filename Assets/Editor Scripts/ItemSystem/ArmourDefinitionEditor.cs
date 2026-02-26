using UnityEditor;

[CustomEditor(typeof(ArmourDefinition), true)]
public class ArmourDefinitionEditor : ItemDefinitionEditor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		EditorGUILayout.PropertyField(serializedObject.FindProperty("armourType"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("protectionProvided"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("inventorySlotsProvided"));

		serializedObject.ApplyModifiedProperties();
	}
}
