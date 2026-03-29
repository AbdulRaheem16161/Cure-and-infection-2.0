using UnityEngine;
using UnityEditor;
using Game.MyNPC;

[CustomEditor(typeof(NPCStateMachine))]
public class NPCStateMachineEditor : Editor
{
	public override bool RequiresConstantRepaint()
	{
		return Application.isPlaying;
	}

    public override void OnInspectorGUI()
    {
        var npc = (NPCStateMachine)target;
        serializedObject.Update();

        // ─────────────────────────────
        // Beliefs
        EditorGUILayout.LabelField("Runtime Info", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		npc.CurrentStateName = EditorGUILayout.TextField("Current State Name", npc.CurrentStateName);
		npc.CurrentSpeed = EditorGUILayout.FloatField("Current Speed", npc.CurrentSpeed);
		npc.CurrentDestination = EditorGUILayout.Vector3Field("Current Destination", npc.CurrentDestination);
		EditorGUI.indentLevel--;

		EditorGUILayout.Space(10);

		// ─────────────────────────────
		// Free Move
		EditorGUILayout.LabelField("Movement State Settings", EditorStyles.boldLabel);
        npc.EnableMovement = EditorGUILayout.Toggle("Enable Movement", npc.EnableMovement);
        if (npc.EnableMovement)
        {
			npc.useBackupMovement = EditorGUILayout.Toggle("Use Backup Movement", npc.useBackupMovement);

			EditorGUI.indentLevel++;
			EditorGUILayout.LabelField("Random Move Settings", EditorStyles.boldLabel);
			npc.moveOnRandomPath = EditorGUILayout.Toggle("Move On Random Path", npc.moveOnRandomPath);
			if (npc.moveOnRandomPath)
			{
				npc.RandomMovementManager = (RandomMovementManager)EditorGUILayout.ObjectField(
					"Random Follow Point", npc.RandomMovementManager, typeof(RandomMovementManager), true);
			}

			EditorGUILayout.Space(10);

			EditorGUILayout.LabelField("Patrol Move Settings", EditorStyles.boldLabel);
			npc.moveOnPatrolPath = EditorGUILayout.Toggle("Move On Patrol Path", npc.moveOnPatrolPath);
			if (npc.moveOnPatrolPath)
			{
				npc.PatrolPoints = (TrackGizmos)EditorGUILayout.ObjectField("Patrol Points", npc.PatrolPoints, typeof(TrackGizmos), true);
			}
			EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

		// ─────────────────────────────
		// Flee State
		EditorGUILayout.LabelField("Flee State Settings", EditorStyles.boldLabel);
		npc.EnableFlee = EditorGUILayout.Toggle("Enable Flee", npc.EnableFlee);
		if (npc.EnableFlee)
		{
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(serializedObject.FindProperty("targetInFleeRange"));
			EditorGUI.indentLevel--;
		}

		EditorGUILayout.Space(10);

		// ─────────────────────────────
		// Eat Corpse
		EditorGUILayout.LabelField("Eat Corpse State Settings", EditorStyles.boldLabel);
		npc.EnableEatCorpseState = EditorGUILayout.Toggle("Enable Eat Corpse", npc.EnableEatCorpseState);

		EditorGUILayout.Space(10);

		// ─────────────────────────────
		// Investigate
		EditorGUILayout.LabelField("Investigate State Settings", EditorStyles.boldLabel);
		npc.EnableInvestigate = EditorGUILayout.Toggle("Enable Investigate", npc.EnableInvestigate);
		if (npc.EnableInvestigate)
		{
			EditorGUI.indentLevel++;
			npc.HasLocationToInvestigate = EditorGUILayout.Toggle("Has Location To Investigate", npc.HasLocationToInvestigate);
			npc.HasInvestigatedLocation = EditorGUILayout.Toggle("Has Investigated Location", npc.HasInvestigatedLocation);
			npc.locationToInvestigate = EditorGUILayout.Vector3Field("location To Investigate", npc.locationToInvestigate);
			EditorGUI.indentLevel--;
		}

		EditorGUILayout.Space(10);

		// ─────────────────────────────
		// Chase
		EditorGUILayout.LabelField("Chase State Settings", EditorStyles.boldLabel);
        npc.EnableChase = EditorGUILayout.Toggle("Enable Chase", npc.EnableChase);
		if (npc.EnableChase)
		{
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(serializedObject.FindProperty("targetInChaseRange"));
			EditorGUI.indentLevel--;
		}

        EditorGUILayout.Space(10);

        // ─────────────────────────────
        // Melee Attack
        EditorGUILayout.LabelField("Melee Attack State", EditorStyles.boldLabel);
        npc.EnableMeleeAttack = EditorGUILayout.Toggle("Enable Melee Attack", npc.EnableMeleeAttack);
		if (npc.EnableMeleeAttack)
		{
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(serializedObject.FindProperty("hasEquippedMeleeWeapon"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("targetInMeleeRange"));
			EditorGUI.indentLevel--;
		}

        EditorGUILayout.Space(10);

        // ─────────────────────────────
        // Ranged Attack
        EditorGUILayout.LabelField("Ranged Attack State", EditorStyles.boldLabel);
        npc.EnableRangedAttack = EditorGUILayout.Toggle("Enable Ranged Attack", npc.EnableRangedAttack);
        if (npc.EnableRangedAttack)
		{
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(serializedObject.FindProperty("hasEquippedRangedWeapon"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("targetInShootingRange"));
			EditorGUI.indentLevel--;
        }

		if (GUI.changed)
        {
            EditorUtility.SetDirty(npc);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
