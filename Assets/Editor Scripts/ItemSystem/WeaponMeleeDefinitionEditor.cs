using UnityEditor;

[CustomEditor(typeof(WeaponMeleeDefinition), true)]
public class WeaponMeleeDefinitionEditor : ItemDefinitionEditor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponType"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("damage"));

		EditorGUILayout.PropertyField(serializedObject.FindProperty("lightSwingSpeed"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("lightSwingCooldown"));

		EditorGUILayout.PropertyField(serializedObject.FindProperty("heavySwingSpeed"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("heavySwingCooldown"));

		serializedObject.ApplyModifiedProperties();
	}
}
