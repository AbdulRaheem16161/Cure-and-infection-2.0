using UnityEditor;

[CustomEditor(typeof(ConsumableDefinition), true)]
public class ConsumableDefinitionEditor : ItemDefinitionEditor
{
	ConsumableDefinition data;

	public void OnEnable()
	{
		data = (ConsumableDefinition)target;
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		EditorGUILayout.PropertyField(serializedObject.FindProperty("restorationType"));

		if (data.RestorationTypes.HasFlag(ConsumableDefinition.RestorationType.health))
			EditorGUILayout.PropertyField(serializedObject.FindProperty("healthRestored"));

		if (data.RestorationTypes.HasFlag(ConsumableDefinition.RestorationType.food))
			EditorGUILayout.PropertyField(serializedObject.FindProperty("foodRestored"));

		if (data.RestorationTypes.HasFlag(ConsumableDefinition.RestorationType.water))
			EditorGUILayout.PropertyField(serializedObject.FindProperty("waterRestored"));

		serializedObject.ApplyModifiedProperties();
	}
}
