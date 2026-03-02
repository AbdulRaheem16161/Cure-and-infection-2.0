//using UnityEngine;

//#if UNITY_EDITOR
//using UnityEditor;
//#endif

//[ExecuteInEditMode]
//public class NPCManager_old : MonoBehaviour
//{
//    public GameObject PadestrianPrefab;
//    public GameObject GuardPrefab;
//    public GameObject ZombiePrefab;
//    public GameObject RaptorPrefab;
//    public GameObject PatrolFollowPointPrefab;
//    public GameObject RandomFollowPrefab;
//    public Object docPDF;


//    #region Automatic Setup
//    public void AutomaticSetup()
//    {
//        // create a local veriable called "spawner". Add NPCSpawner.cs Script on the NPC Manager GameObject and store it in the spawner veriable.
//        NPCSpawner_old spawner = gameObject.AddComponent<NPCSpawner_old>();

//        // create a local veriable called "patrolManager". Add NPCPatrolManager.cs Script on the NPC Manager GameObject and store it in the patrolManager veriable.
//        NPCPatrolManager patrolManager = gameObject.AddComponent<NPCPatrolManager>();

//        // create a local veriable called "randomManager". Add NPCRandomMoveManager.cs Script on the NPC Manager GameObject and store it in the randomManager veriable.
//        NPCRandomMoveManager randomManager = gameObject.AddComponent<NPCRandomMoveManager>();

//#if UNITY_EDITOR
//        // characters is a list of CharacterData Class in the NPCSpawn.cs
//        spawner.characters.Add(new NPCSpawner_old.CharacterData { typeName = "NPC", prefab = PadestrianPrefab });
//        spawner.characters.Add(new NPCSpawner_old.CharacterData { typeName = "Guard", prefab = GuardPrefab });
//        spawner.characters.Add(new NPCSpawner_old.CharacterData { typeName = "Zombie", prefab = ZombiePrefab });

//        spawner.AIPatrolManager = patrolManager;
//        spawner.AIRandomManager = randomManager;

//        patrolManager.PatrolFollowPoint = PatrolFollowPointPrefab;
//        randomManager.RandomFollowPoint = RandomFollowPrefab;

//        // mark dirty so Unity saves changes
//        EditorUtility.SetDirty(spawner);
//        EditorUtility.SetDirty(patrolManager);
//        EditorUtility.SetDirty(randomManager);
//        EditorUtility.SetDirty(this);

//        Debug.Log("✅ AIManager: AutomaticSetup completed with ZERO manual setup!");
//#else
//        Debug.LogError("⚠ AutomaticSetup only works inside the Unity Editor.");
//#endif
//    }
//    #endregion
//}
