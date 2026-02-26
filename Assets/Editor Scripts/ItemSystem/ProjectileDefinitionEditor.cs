using UnityEditor;

[CustomEditor(typeof(ProjectileDefinition), true)]
public class ProjectileDefinitionEditor : ItemDefinitionEditor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		serializedObject.ApplyModifiedProperties();
	}
}
