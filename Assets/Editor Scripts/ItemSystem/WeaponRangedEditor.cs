using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RangedWeaponItem))]
public class WeaponRangedEditor : Editor, IAmmoGiver
{
	private bool showDebugControls;
	private WeaponRangedDefinition Definition;
	private string animationName;

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		RangedWeaponItem weapon = (RangedWeaponItem)target;

		GUILayout.Space(10);
		GUILayout.Label("DEBUG CONTROLS", EditorStyles.boldLabel);
		showDebugControls = EditorGUILayout.Toggle("Show Debug Controls", showDebugControls);

		if (!showDebugControls) return;

		Definition = (WeaponRangedDefinition)EditorGUILayout.ObjectField("Weapon Definition", Definition, typeof(WeaponRangedDefinition), true);

		if (GUILayout.Button("Initialize Weapon"))
		{
			weapon.InitializeItem(Definition, 1);
		}
		if (GUILayout.Button("Fire Weapon"))
		{
			if (!ApplicationPlaying()) return;

			weapon.Shoot();
		}
		if (GUILayout.Button("Reload Weapon"))
		{
			if (!ApplicationPlaying()) return;

			weapon.Reload(this, true);
		}

		GUILayout.Space(10);

		animationName = EditorGUILayout.TextField("Weapon Definition", animationName);
		if (GUILayout.Button("Play Weapon Animation"))
		{
			if (!ApplicationPlaying()) return;

			weapon.PlayAnimation(animationName);
		}
		if (GUILayout.Button("Reset Current Weapon Animation"))
		{
			if (!ApplicationPlaying()) return;

			weapon.ResetAnimation();
		}

		serializedObject.ApplyModifiedProperties();
	}

	#region IAmmoGiver interfaces
	public bool AmmoAvailable(ProjectileDefinition projectileDefinition)
	{
		return true;
	}
	public int TakeAmmo(ProjectileDefinition projectileDefinition, int amountNeeded, bool takeForFree = true)
	{
		return amountNeeded;
	}
	#endregion

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
