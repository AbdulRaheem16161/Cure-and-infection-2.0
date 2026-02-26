using UnityEditor;

[CustomEditor(typeof(WeaponRangedDefinition), true)]
public class WeaponRangedDefinitionEditor : ItemDefinitionEditor
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

		EditorGUILayout.Space();

		EditorGUILayout.PropertyField(serializedObject.FindProperty("aimTime"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("moveSpeedModifier"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("aimMoveSpeedModifer"));

		EditorGUILayout.Space();

		EditorGUILayout.PropertyField(serializedObject.FindProperty("hipFireSpread"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("adsSpread"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("spreadIncreasePerShot"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("spreadRecoveryRate"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("maxSpread"));

		EditorGUILayout.Space();

		EditorGUILayout.PropertyField(serializedObject.FindProperty("recoilPerShot"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("recoilPattern"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("recoilRecoveryRate"));

		EditorGUILayout.PropertyField(serializedObject.FindProperty("ammoType"));

		EditorGUILayout.Space();

		EditorGUILayout.PropertyField(serializedObject.FindProperty("fireSFX"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("reloadSfx"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("muzzleFlashVfx"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("impactVfx"));

		serializedObject.ApplyModifiedProperties();
	}
}
