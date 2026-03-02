//// Summary: 
//// Provides custom Inspector buttons for the following scripts:
//// - AIPatrolManager  
//// - AIRandomManager  
//// - AISpawner  
//// - AIManager

//using UnityEngine;
//using UnityEditor;
//using System.Collections.Generic;
//using System.Text.RegularExpressions;

//#region Base Editor
//// Base editor: shared logic for patrol + spawner
//public class NPCManagerEditor_old : Editor
//{
//    public enum CharacterType
//    {
//        NPC,
//        Guard,
//        Zombie
//    }

//    public override void OnInspectorGUI()
//    {
//        DrawDefaultInspector();

//        // Handle AIPatrolManager buttons
//        if (target is NPCPatrolManager AIPatrolManager)
//        {
//            #region Patrol Manager Buttons
//            // 🔘 Button: Run AutoSetup
//            if (GUILayout.Button("Setup Patrol Track"))
//            {
//                AIPatrolManager.AutoSetup();
//            }

//            // 🔘 Button: Add Track Point
//            if (GUILayout.Button("Add Track Point"))
//            {
//                if (AIPatrolManager.Track == null)
//                {
//                    Debug.LogWarning("⚠ Cannot add Track Point: Track object is missing!");
//                    return;
//                }

//                // 🧹 Step 1: Clean nulls from the list
//                for (int i = AIPatrolManager.TrackPoints.Count - 1; i >= 0; i--)
//                {
//                    if (AIPatrolManager.TrackPoints[i] == null)
//                    {
//                        AIPatrolManager.TrackPoints.RemoveAt(i);
//                    }
//                }

//                // 🕵 Step 2: Collect existing numbers
//                HashSet<int> usedNumbers = new HashSet<int>();
//                Regex regex = new Regex(@"Track Point \((\d+)\)");

//                foreach (var point in AIPatrolManager.TrackPoints)
//                {
//                    if (point != null)
//                    {
//                        Match match = regex.Match(point.name);
//                        if (match.Success && int.TryParse(match.Groups[1].Value, out int num))
//                        {
//                            usedNumbers.Add(num);
//                        }
//                    }
//                }

//                // 🔢 Step 3: Find the smallest missing number
//                int nextNumber = 1;
//                while (usedNumbers.Contains(nextNumber))
//                {
//                    nextNumber++;
//                }

//                // 🏗 Step 4: Create the new Track Point
//                GameObject newPoint = new GameObject($"Track Point ({nextNumber})");
//                newPoint.transform.SetParent(AIPatrolManager.Track.transform);
//                newPoint.transform.localPosition = Vector3.zero;

//                Undo.RegisterCreatedObjectUndo(newPoint, "Create Track Point");
//                AIPatrolManager.TrackPoints.Add(newPoint);
//            }
//            #endregion
//        }

//        // Handle AIPatrolManager buttons
//        if (target is NPCRandomMoveManager AIRandomManager)
//        {
//            #region Random Manager Buttons
//            // 🔘 Button: Run AutoSetup
//            if (GUILayout.Button("Setup Area Points"))
//            {
//                AIRandomManager.AutoSetup();
//            }

//            // 🔘 Button: Add Track Point
//            if (GUILayout.Button("Add Area Point"))
//            {
//                // 🧹 Step 1: Clean nulls from the list
//                for (int i = AIRandomManager.AreaPoints.Count - 1; i >= 0; i--)
//                {
//                    if (AIRandomManager.AreaPoints[i] == null)
//                    {
//                        AIRandomManager.AreaPoints.RemoveAt(i);
//                    }
//                }

//                // 🕵 Step 2: Collect existing numbers
//                HashSet<int> usedNumbers = new HashSet<int>();
//                Regex regex = new Regex(@"Area Point \((\d+)\)");

//                foreach (var point in AIRandomManager.AreaPoints)
//                {
//                    if (point != null)
//                    {
//                        Match match = regex.Match(point.name);
//                        if (match.Success && int.TryParse(match.Groups[1].Value, out int num))
//                        {
//                            usedNumbers.Add(num);
//                        }
//                    }
//                }

//                // 🔢 Step 3: Find the smallest missing number
//                int nextNumber = 1;
//                while (usedNumbers.Contains(nextNumber))
//                {
//                    nextNumber++;
//                }

//                // 🏗 Step 4: Create the new Track Point
//                GameObject newPoint = new GameObject($"Area Point ({nextNumber})");
//                newPoint.transform.SetParent(AIRandomManager.Area.transform);
//                newPoint.transform.localPosition = Vector3.zero;

//                Undo.RegisterCreatedObjectUndo(newPoint, "Create Track Point");
//                AIRandomManager.AreaPoints.Add(newPoint);
//            }
//            #endregion
//        }

//        // Handle AISpawner buttons 
//        if (target is NPCSpawner_old spawner)
//        {
//            #region Spawner Buttons
//            // 🔘 Button: Run SpawnNPC
//            if (GUILayout.Button("Spawn NPC"))
//            {
//                spawner.Spawn("NPC");
//            }
//            if (GUILayout.Button("Spawn Guard"))
//            {
//                spawner.Spawn("Guard");
//            }
//            if (GUILayout.Button("Spawn Zombie"))
//            {
//                spawner.Spawn("Zombie");
//            }
//            #endregion
//        }


//        // Handle AISpawner buttons
//        if (target is NPCManager_old AIManager)
//        {
//            #region AIManager Buttons
//            // 🔘 Button: Run Automatic Setup
//            if (GUILayout.Button("Automatic SetUp"))
//            {
//                AIManager.AutomaticSetup();
//            }
//            #endregion
//        }
//    }
//}
//#endregion

//#region Patrol Manager Editor
//[CustomEditor(typeof(NPCPatrolManager))]
//public class AIPatrolManagerEditor : NPCManagerEditor_old { }
//#endregion

//#region Patrol Manager Editor
//[CustomEditor(typeof(NPCRandomMoveManager))]
//public class AIRandomManagerEditor : NPCManagerEditor_old { }
//#endregion

//#region Spawner Editor
//[CustomEditor(typeof(NPCSpawner_old))]
//public class AISpawnerEditor : NPCManagerEditor_old { }
//#endregion

//#region Manager Editor
//[CustomEditor(typeof(NPCManager_old))]
//public class AIManagerEditor : NPCManagerEditor_old { }
//#endregion
