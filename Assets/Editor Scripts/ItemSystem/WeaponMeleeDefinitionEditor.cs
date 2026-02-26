using UnityEditor;

[CustomEditor(typeof(WeaponMeleeDefinition), true)]
public class WeaponMeleeDefinitionEditor : ItemDefinitionEditor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponType"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("damage"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("attackSpeed"));

		serializedObject.ApplyModifiedProperties();
	}
}
