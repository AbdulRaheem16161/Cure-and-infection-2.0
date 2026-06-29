using Game.MyNPC;
using System.Collections.Generic;
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
		EditorGUILayout.Toggle("IsExhausted", beliefs.IsExhausted);
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
		if (beliefs.LookDirection == null)
			EditorGUILayout.TextField("No Look Direction");
		else
			EditorGUILayout.Vector3Field("Look Direction", (Vector3)beliefs.LookDirection);
		EditorGUI.indentLevel--;
		#endregion

		EditorGUILayout.Space(10);

		#region Cover Beliefs
		EditorGUILayout.LabelField("Cover Beliefs", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		EditorGUILayout.Toggle("Return Fire", beliefs.ReturnFire);
		EditorGUILayout.Toggle("Moving To Cover", beliefs.MovingToCover);
		EditorGUILayout.Toggle("In Cover", beliefs.InCover);
		if (beliefs.CoverPosition == null)
			EditorGUILayout.TextField("No Cover Position");
		else
			EditorGUILayout.Vector3Field("Cover Position", (Vector3)beliefs.CoverPosition);
		EditorGUI.indentLevel--;
		#endregion

		EditorGUILayout.Space(10);

		#region Target Beliefs
		EditorGUILayout.LabelField("Target Beliefs", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		if (beliefs.StatsHandler.LifeState == EntityDefinition.LifeState.zombified)
			DrawTargetData("Eatable Target", beliefs.EatableTarget);

		DrawTargetData("Target", beliefs.Target);
		EditorGUILayout.Toggle("Target In Shooting Range", beliefs.TargetInShootingRange);
		EditorGUILayout.Toggle("Target In Melee Range", beliefs.TargetInMeleeRange);
		EditorGUI.indentLevel--;
		#endregion

		EditorGUILayout.Space(10);

		#region Flee Beliefs
		EditorGUILayout.LabelField("Flee Beliefs", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		EditorGUILayout.Toggle("Target In Flee Range", beliefs.TargetInFleeRange);
		DrawTargetData("Flee Target", beliefs.FleeTarget);
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

        EditorGUILayout.Space(10);

        #region Loot Beliefs
        EditorGUILayout.LabelField("Loot Beliefs", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        DrawInteractableContextData("Best Lootable", beliefs.LootableContainer);
        EditorGUILayout.Toggle("Can Loot Container", beliefs.CanLootContainer);
		DrawInteractableContextData("Looted Containers", beliefs.lootedContainers);
        EditorGUI.indentLevel--;
        #endregion
    }

    private bool ApplicationPlaying()
	{
		if (!Application.isPlaying)
			return false;
		return true;
	}

	private void DrawTargetData(string label, TargetData target)
	{
		EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

		if (target == null)
		{
			EditorGUILayout.LabelField("None");
			return;
		}

		EditorGUI.indentLevel++;

        EditorGUILayout.ObjectField("Stats Handler", target.StatsHandler, typeof(StatsHandler), true);
        EditorGUILayout.ObjectField("Collider", target.Collider, typeof(Collider), true);
        EditorGUILayout.Vector3Field("Position", target.Position);
        EditorGUILayout.FloatField("Distance", target.SquaredDistance);

		EditorGUI.indentLevel--;
	}

    private void DrawInteractableContextData(string label, List<InteractContext> targets)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        if (targets == null || targets.Count == 0)
        {
            EditorGUILayout.LabelField("None");
            return;
        }

        foreach (var target in targets)
        {
            DrawInteractableContextData(string.Empty, target);
            EditorGUILayout.Space();
        }
    }
    private void DrawInteractableContextData(string label, InteractContext target)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        if (target == null)
        {
            EditorGUILayout.LabelField("None");
            return;
        }

        EditorGUI.indentLevel++;

        EditorGUILayout.TextField(target.name);
        EditorGUILayout.EnumFlagsField("Loot state", target.lootState);
        EditorGUILayout.FloatField("Distance", target.squaredDistance);

        EditorGUI.indentLevel--;
    }
}
