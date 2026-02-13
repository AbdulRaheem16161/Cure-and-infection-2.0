using UnityEditor;

[CustomEditor(typeof(ItemDefinition), true)]
public class ItemDefinitionEditor : Editor
{
	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUI.BeginDisabledGroup(true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
		EditorGUI.EndDisabledGroup();

		EditorGUILayout.PropertyField(serializedObject.FindProperty("itemId"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("itemName"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("itemDescription"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("itemPrice"));

		EditorGUILayout.PropertyField(serializedObject.FindProperty("tradable"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("allowedSlots"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("stackLimit"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("itemWeight"));

		serializedObject.ApplyModifiedProperties();
	}
}
