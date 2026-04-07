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

		#region Runtime Info
		EditorGUILayout.LabelField("Runtime Info", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		npc.CurrentStateName = EditorGUILayout.TextField("Current State Name", npc.CurrentStateName);
		npc.CurrentSpeed = EditorGUILayout.FloatField("Current Speed", npc.CurrentSpeed);
		npc.CurrentDestination = EditorGUILayout.Vector3Field("Current Destination", npc.CurrentDestination);
		EditorGUI.indentLevel--;
		#endregion

		EditorGUILayout.Space(10);

		#region State Toggles
		EditorGUILayout.LabelField("State Toggles", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		npc.EnableFlee = EditorGUILayout.Toggle("Enable Flee", npc.EnableFlee);
		npc.EnableRangedAttack = EditorGUILayout.Toggle("Enable Ranged Attack", npc.EnableRangedAttack);
		npc.EnableMeleeAttack = EditorGUILayout.Toggle("Enable Melee Attack", npc.EnableMeleeAttack);
		npc.EnableChase = EditorGUILayout.Toggle("Enable Chase", npc.EnableChase);
		npc.EnableInvestigate = EditorGUILayout.Toggle("Enable Investigate", npc.EnableInvestigate);
		npc.EnableEatCorpseState = EditorGUILayout.Toggle("Enable Eat Corpse", npc.EnableEatCorpseState);
		EditorGUI.indentLevel--;
		#endregion

		EditorGUILayout.Space(10);

		#region Movement State Toggles
		EditorGUILayout.LabelField("Movement State Toggles", EditorStyles.boldLabel);
        npc.EnableMovement = EditorGUILayout.Toggle("Enable Movement", npc.EnableMovement);

		if (npc.EnableMovement)
        {
			EditorGUI.indentLevel++;
			npc.movementType = (NPCStateMachine.MovementType)EditorGUILayout.EnumPopup("Movement Type", npc.movementType);
			EditorGUILayout.LabelField("Patrol Move", EditorStyles.boldLabel);
			npc.movementType = (NPCStateMachine.MovementType)EditorGUILayout.EnumPopup("Movement Type", npc.movementType);
			if (npc.movementType == NPCStateMachine.MovementType.patrolMove)
			{
				npc.PatrolPathManager = (PatrolPathManager)EditorGUILayout.ObjectField(
					"Patrol Points", npc.PatrolPathManager, typeof(PatrolPathManager), true);
			}
			else if (npc.movementType == NPCStateMachine.MovementType.randomAreaMove)
			{
				npc.RandomAreaMoveManager = (RandomAreaMoveManager)EditorGUILayout.ObjectField(
					"Random Area Move Manager", npc.RandomAreaMoveManager, typeof(RandomAreaMoveManager), true);
			}
			EditorGUI.indentLevel--;
        }
		#endregion

		if (GUI.changed)
        {
            EditorUtility.SetDirty(npc);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
