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
            npc.PatrolFollowPoint = (Transform)EditorGUILayout.ObjectField("Patrol Follow Point", npc.PatrolFollowPoint, typeof(Transform), true);
            npc.RandomFollowPoint = (Transform)EditorGUILayout.ObjectField("Random Follow Point", npc.RandomFollowPoint, typeof(Transform), true);
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
            npc.DetectionCone = (DetectionCone)EditorGUILayout.ObjectField("Detection Cone", npc.DetectionCone, typeof(DetectionCone), true);
            npc.DetectionRadius = (DetectionRadius)EditorGUILayout.ObjectField("Detection Radius", npc.DetectionRadius, typeof(DetectionRadius), true);
            npc.ChaseSpeed = EditorGUILayout.FloatField("Chase Speed", npc.ChaseSpeed);
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
            npc.WeaponHolder = (GameObject)EditorGUILayout.ObjectField("Weapon Holder", npc.WeaponHolder, typeof(GameObject), true);
            npc.OpponentInRangedAttackRange = EditorGUILayout.Toggle("Opponent In Ranged Attack Range", npc.OpponentInRangedAttackRange);
            npc.RangedAttackRotSpeed = EditorGUILayout.FloatField("Ranged Attack RotSpeed", npc.RangedAttackRotSpeed);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        // ─────────────────────────────
        // Health
        EditorGUILayout.LabelField("Health Settings", EditorStyles.boldLabel);
        npc.EnableHeatlh = EditorGUILayout.Toggle("Enable Health", npc.EnableHeatlh);
        if (npc.EnableHeatlh)
        {
            EditorGUI.indentLevel++;
            npc.TotalHealth = EditorGUILayout.IntField("Total Health", npc.TotalHealth);
            npc.CurrentHealth = EditorGUILayout.IntField("Current Health", npc.CurrentHealth);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        // ─────────────────────────────
        // Death
        EditorGUILayout.LabelField("Death Settings", EditorStyles.boldLabel);
        npc.EnableDeath = EditorGUILayout.Toggle("Enable Death", npc.EnableDeath);
        if (npc.EnableDeath)
        {
            EditorGUI.indentLevel++;
            npc.isDead = EditorGUILayout.Toggle("Is Dead", npc.isDead);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        // ─────────────────────────────
        // Respawn
        EditorGUILayout.LabelField("Respawn Settings", EditorStyles.boldLabel);
        npc.EnableRespawn = EditorGUILayout.Toggle("Enable Respawn", npc.EnableRespawn);
        if (npc.EnableRespawn)
        {
            EditorGUI.indentLevel++;
            npc.SpawnPoint = (Transform)EditorGUILayout.ObjectField("Spawn Point", npc.SpawnPoint, typeof(Transform), true);
            npc.BodyParts = (GameObject)EditorGUILayout.ObjectField("Body Parts", npc.BodyParts, typeof(GameObject), true);
            npc.WaitBeforeRespawn = EditorGUILayout.IntField("Wait Before Respawn", npc.WaitBeforeRespawn);
            EditorGUI.indentLevel--;
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(npc);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
