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

        #region Npc Capabilities And Movement Toggles
        EditorGUILayout.LabelField("Npc Capabilities", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        SerializedProperty capabilityProp = serializedObject.FindProperty("capabilityOverrides");
        EditorGUILayout.PropertyField(capabilityProp);

        EditorGUILayout.LabelField("Movement Toggles", EditorStyles.boldLabel);
        npc.showFullMovePath = EditorGUILayout.Toggle("Show Full Move Path", npc.showFullMovePath);
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
        #endregion

        EditorGUILayout.Space(10);

        #region Npc Range Consideration Toggles
        EditorGUILayout.LabelField("Npc Range Toggles", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        npc.showUnholsteredWeaponRange = EditorGUILayout.Toggle("Show Unholstered Weapon Range", npc.showUnholsteredWeaponRange);
		npc.showFleeRange = EditorGUILayout.Toggle("Show Flee Range", npc.showFleeRange);
        EditorGUI.indentLevel--;
        #endregion

        EditorGUILayout.Space(10);

        #region Npc Interactables Context
        if (npc.DoorInPath != null)
        {
            SerializedProperty doorProp = serializedObject.FindProperty("DoorInPath");
            EditorGUILayout.PropertyField(doorProp, true);
        }
        #endregion

        if (GUI.changed)
        {
            EditorUtility.SetDirty(npc);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
