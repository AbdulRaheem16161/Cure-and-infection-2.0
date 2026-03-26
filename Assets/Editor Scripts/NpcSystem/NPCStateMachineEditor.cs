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
        // General Values
        EditorGUILayout.LabelField("General Values", EditorStyles.boldLabel);
        npc.Animator = (Animator)EditorGUILayout.ObjectField("Animator", npc.Animator, typeof(Animator), true);
        npc.Agent = (UnityEngine.AI.NavMeshAgent)EditorGUILayout.ObjectField("Agent", npc.Agent, typeof(UnityEngine.AI.NavMeshAgent), true);
        npc.CurrentSpeed = EditorGUILayout.FloatField("Current Speed", npc.CurrentSpeed);
        npc.CurrentStateName = EditorGUILayout.TextField("Current State Name", npc.CurrentStateName);
        npc.CurrentDestination = EditorGUILayout.Vector3Field("Current State Name", npc.CurrentDestination);
        npc.RotationSpeed = EditorGUILayout.FloatField("Rotation Speed", npc.RotationSpeed);

        EditorGUILayout.Space(10);

        // ─────────────────────────────
        // Free Move
        EditorGUILayout.LabelField("FreeMove Settings", EditorStyles.boldLabel);
        npc.EnableFreeMove = EditorGUILayout.Toggle("Enable Free Move", npc.EnableFreeMove);
        if (npc.EnableFreeMove)
        {
            EditorGUI.indentLevel++;
			npc.PatrolSpeed = EditorGUILayout.FloatField("Patrol Speed", npc.PatrolSpeed);

			EditorGUILayout.LabelField("Random Move Settings", EditorStyles.boldLabel);
			npc.moveOnRandomPath = EditorGUILayout.Toggle("Move On Random Path", npc.moveOnRandomPath);
			npc.RandomMovementManager = (RandomMovementManager)EditorGUILayout.ObjectField(
				"Random Follow Point", npc.RandomMovementManager, typeof(RandomMovementManager), true);

			EditorGUILayout.LabelField("Patrol Move Settings", EditorStyles.boldLabel);
			npc.moveOnPatrolPath = EditorGUILayout.Toggle("Move On Patrol Path", npc.moveOnPatrolPath);
			npc.PatrolPoints = (TrackGizmos)EditorGUILayout.ObjectField("Patrol Points", npc.PatrolPoints, typeof(TrackGizmos), true);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

		// ─────────────────────────────
		// Eat Corpse
		EditorGUILayout.LabelField("Eat Corpse Settings", EditorStyles.boldLabel);
		npc.EnableEatCorpseState = EditorGUILayout.Toggle("Enable Eat Corpse", npc.EnableEatCorpseState);

		EditorGUILayout.Space(10);

		// ─────────────────────────────
		// Investigate
		EditorGUILayout.LabelField("Investigate Settings", EditorStyles.boldLabel);
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
		EditorGUILayout.LabelField("Chase Settings", EditorStyles.boldLabel);
        npc.EnableChase = EditorGUILayout.Toggle("Enable Chase", npc.EnableChase);
        if (npc.EnableChase)
        {
            EditorGUI.indentLevel++;
            npc.ChaseSpeed = EditorGUILayout.FloatField("Chase Speed", npc.ChaseSpeed);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

		// ─────────────────────────────
		// Attack (General)
		EditorGUILayout.LabelField("Attack Settings", EditorStyles.boldLabel);

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
