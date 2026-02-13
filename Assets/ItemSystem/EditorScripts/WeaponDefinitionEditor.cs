using UnityEditor;

[CustomEditor(typeof(WeaponDefinition), true)]
public class WeaponDefinitionEditor : ItemDefinitionEditor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponType"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("damage"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("magazineSize"));

		EditorGUILayout.Space();

		EditorGUILayout.PropertyField(serializedObject.FindProperty("fireMode"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("fireRateRPM"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("reloadTime"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("effectiveRange"));

		EditorGUILayout.PropertyField(serializedObject.FindProperty("ammoType"));

		serializedObject.ApplyModifiedProperties();
	}
}
