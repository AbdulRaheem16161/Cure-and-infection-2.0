//using UnityEngine;
//using UnityEditor;
//using Game.MyPlayer;

//[CustomEditor(typeof(PlayerStateMachine))]
//public class PlayerStateMachineEditor : Editor
//{
//    private bool showReferences = true;

//    public override void OnInspectorGUI()
//    {
//        PlayerStateMachine psm = (PlayerStateMachine)target;

//        // Collapsible References section
//        showReferences = EditorGUILayout.Foldout(showReferences, "References");
//        if (showReferences)
//        {
//            EditorGUI.indentLevel++;
//            psm.Controller = (CharacterController)EditorGUILayout.ObjectField("Controller", psm.Controller, typeof(CharacterController), true);
//            psm.InputReader = (InputReader)EditorGUILayout.ObjectField("Input Reader", psm.InputReader, typeof(InputReader), true);
//            psm.CameraTransform = (Transform)EditorGUILayout.ObjectField("Camera Transform", psm.CameraTransform, typeof(Transform), true);
//            psm.Body = (Transform)EditorGUILayout.ObjectField("Body", psm.Body, typeof(Transform), true);
//            psm.Animator = (Animator)EditorGUILayout.ObjectField("Animator", psm.Animator, typeof(Animator), true);
//            psm.Hitbox = (PlayerHitbox)EditorGUILayout.ObjectField("Hitbox", psm.Hitbox, typeof(PlayerHitbox), true);
//            EditorGUI.indentLevel--;
//        }

//        // Draw the rest of the inspector automatically
//        DrawDefaultInspector();
//    }
//}
