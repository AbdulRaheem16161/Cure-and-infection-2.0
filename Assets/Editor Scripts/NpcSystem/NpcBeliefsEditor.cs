using Game.MyNPC;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NpcBeliefs))]
public class NpcBeliefsEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		var beliefs = (NpcBeliefs)target;

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Runtime Beliefs (Read Only)", EditorStyles.boldLabel);

		if (!ApplicationPlaying())
		{
			EditorGUILayout.HelpBox("Enter Play Mode to see runtime belief values.", MessageType.Warning);
			return;
		}

		EditorGUILayout.Space(10);

		EditorGUI.indentLevel++;
		EditorGUILayout.Toggle("Alert", beliefs.Alert);
		EditorGUI.indentLevel--;

		#region Movement Beliefs
		EditorGUILayout.LabelField("Movement Beliefs", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		EditorGUILayout.Toggle("Stunned", beliefs.Stunned);
		EditorGUILayout.Toggle("Idling", beliefs.Idling);
		EditorGUILayout.Toggle("Moving", beliefs.Moving);
		EditorGUI.indentLevel--;
		#endregion

		EditorGUILayout.Space(10);

		#region Stat Beliefs
		EditorGUILayout.LabelField("Stat Beliefs", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		EditorGUILayout.Toggle("Hurt", beliefs.Hurt);
		EditorGUILayout.Toggle("Thirsty", beliefs.Thirsty);
		EditorGUILayout.Toggle("Hungry", beliefs.Hungry);
		EditorGUILayout.Toggle("Exhausted", beliefs.Exhausted);
		EditorGUI.indentLevel--;
		#endregion

		EditorGUILayout.Space(10);

		#region Investigation Beliefs
		EditorGUILayout.LabelField("Investigation Beliefs", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		EditorGUILayout.Toggle("Free To Investigate", beliefs.FreeToInvestigate);
		if (beliefs.InvestigateLocation == null)
			EditorGUILayout.TextField("No Investigate Location");
		else
			EditorGUILayout.Vector3Field("Investigate Location", (Vector3)beliefs.InvestigateLocation);
		EditorGUI.indentLevel--;
		#endregion

		EditorGUILayout.Space(10);

		#region Target Beliefs
		EditorGUILayout.LabelField("Target Beliefs", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		if (beliefs.StatsHandler.LifeState == NpcDefinition.LifeState.zombified)
			EditorGUILayout.Toggle("Has Eatable Target", beliefs.HasEatableTarget);

		EditorGUILayout.Toggle("Has Target", beliefs.HasTarget);
		EditorGUILayout.Toggle("Target In Shooting Range", beliefs.TargetInShootingRange);
		EditorGUILayout.Toggle("Target In Melee Range", beliefs.TargetInMeleeRange);
		EditorGUI.indentLevel--;
		#endregion

		EditorGUILayout.Space(10);

		#region Flee Beliefs
		EditorGUILayout.LabelField("Flee Beliefs", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		EditorGUILayout.Toggle("Target In Flee Range", beliefs.TargetInFleeRange);
		EditorGUILayout.Toggle("Safe From Flee Target", beliefs.SafeFromFleeTarget);
		EditorGUI.indentLevel--;
		#endregion

		EditorGUILayout.Space(10);

		#region Equipment Beliefs
		EditorGUILayout.LabelField("Equipment Beliefs", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		EditorGUILayout.Toggle("Ranged Weapon In Hands", beliefs.RangedWeaponInHands);
		EditorGUILayout.Toggle("Melee Weapon In Hands", beliefs.MeleeWeaponInHands);
		EditorGUILayout.Toggle("Can Heal", beliefs.CanHeal);
		EditorGUILayout.Toggle("Can Drink", beliefs.CanDrink);
		EditorGUILayout.Toggle("Can Eat", beliefs.CanEat);
		EditorGUI.indentLevel--;
		#endregion
	}

	private bool ApplicationPlaying()
	{
		if (!Application.isPlaying)
			return false;
		return true;
	}
}
