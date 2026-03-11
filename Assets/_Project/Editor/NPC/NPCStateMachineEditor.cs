using UnityEngine;
using UnityEditor;
using Game.MyNPC;

[CustomEditor(typeof(NPCStateMachine))]
public class NPCStateMachineEditor : Editor
{
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
        npc.CurrentFollowPoint = (Transform)EditorGUILayout.ObjectField("Current Follow Point", npc.CurrentFollowPoint, typeof(Transform), true);
        npc.RotationSpeed = EditorGUILayout.FloatField("Rotation Speed", npc.RotationSpeed);

        EditorGUILayout.Space(10);

        // ─────────────────────────────
        // Free Move
        EditorGUILayout.LabelField("FreeMove Settings", EditorStyles.boldLabel);
        npc.EnableFreeMove = EditorGUILayout.Toggle("Enable Free Move", npc.EnableFreeMove);
        if (npc.EnableFreeMove)
        {
            EditorGUI.indentLevel++;
            npc.PatrolFollowPoint = (GameObject)EditorGUILayout.ObjectField("Patrol Follow Point", npc.PatrolFollowPoint, typeof(GameObject), true);
            npc.RandomFollowPoint = (GameObject)EditorGUILayout.ObjectField("Random Follow Point", npc.RandomFollowPoint, typeof(GameObject), true);
            npc.moveOnPatrolPath = EditorGUILayout.Toggle("Move On Patrol Path", npc.moveOnPatrolPath);
            npc.moveOnRandomPath = EditorGUILayout.Toggle("Move On Random Path", npc.moveOnRandomPath);
            npc.PatrolSpeed = EditorGUILayout.FloatField("Patrol Speed", npc.PatrolSpeed);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        // ─────────────────────────────
        // Chase
        EditorGUILayout.LabelField("Chase State", EditorStyles.boldLabel);
        npc.EnableChase = EditorGUILayout.Toggle("Enable Chase", npc.EnableChase);
        if (npc.EnableChase)
        {
            EditorGUI.indentLevel++;
            npc.ChaseSpeed = EditorGUILayout.FloatField("Chase Speed", npc.ChaseSpeed);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

		// ─────────────────────────────
		// Investigate
		EditorGUILayout.LabelField("Investigate State", EditorStyles.boldLabel);
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
		// Attack (General)
		EditorGUILayout.LabelField("Attack Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("targetTags"), true);

        EditorGUILayout.Space(10);

        // ─────────────────────────────
        // Melee Attack
        EditorGUILayout.LabelField("Melee Attack State", EditorStyles.boldLabel);
        npc.EnableMeleeAttack = EditorGUILayout.Toggle("Enable Melee Attack", npc.EnableMeleeAttack);
        if (npc.EnableMeleeAttack)
        {
            EditorGUI.indentLevel++;
            npc.Hitbox = (GameObject)EditorGUILayout.ObjectField("Hitbox", npc.Hitbox, typeof(GameObject), true);
            npc.AttackRangeTrigger = (GameObject)EditorGUILayout.ObjectField("Attack Range Trigger", npc.AttackRangeTrigger, typeof(GameObject), true);
            npc.Damage = EditorGUILayout.IntField("Damage", npc.Damage);
            npc.OpponentInMeleeAttackRange = EditorGUILayout.Toggle("Player In Melee Range", npc.OpponentInMeleeAttackRange);
            npc.AttackDuration = EditorGUILayout.FloatField("Attack Duration", npc.AttackDuration);
            npc.HitboxActivationDelay = EditorGUILayout.FloatField("Hitbox Activation Delay", npc.HitboxActivationDelay);
            npc.MinWaitBeforeFreeMove = EditorGUILayout.FloatField("Min Wait Before Free Move", npc.MinWaitBeforeFreeMove);
            npc.MaxWaitBeforeFreeMove = EditorGUILayout.FloatField("Max Wait Before Free Move", npc.MaxWaitBeforeFreeMove);
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
            npc.OpponentInRangedAttackRange = EditorGUILayout.Toggle("Opponent In Ranged Attack Range", npc.OpponentInRangedAttackRange);
            npc.RangedAttackRotSpeed = EditorGUILayout.FloatField("Ranged Attack RotSpeed", npc.RangedAttackRotSpeed);
            EditorGUI.indentLevel--;
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(npc);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
