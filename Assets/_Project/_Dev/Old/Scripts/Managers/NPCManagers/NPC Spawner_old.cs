//using System.Collections.Generic;
//using Game.MyNPC;
//using UnityEngine;

//public class NPCSpawner_old : MonoBehaviour
//{
//    #region Character Data
//    [System.Serializable]
//    public class CharacterData
//    {
//        // The type of character, e.g. "NPC", "Guard", "Zombie"
//        public string typeName;

//        // The prefab for this character
//        public GameObject prefab;

//        // Folder that holds spawned instances (auto-created at runtime)
//        [HideInInspector] public GameObject folder;
//    }

//    // List of all character types set in the Inspector
//    public List<CharacterData> characters = new List<CharacterData>();

//    #endregion

//    #region Attachments
//    [Header("General Attachments")]
//    public NPCPatrolManager AIPatrolManager;
//    public NPCRandomMoveManager AIRandomManager;
//    #endregion

//    #region Spawn Function
//    public void Spawn(string typeName)
//    {
//        #region Find Matching Character
//        // Loop through all character entries to find the one we want
//        CharacterData data = null;
//        foreach (CharacterData c in characters)
//        {
//            if (c.typeName == typeName)
//            {
//                data = c;
//                break;
//            }
//        }

//        // If we didn’t find anything, stop here
//        if (data == null || data.prefab == null)
//        {
//            Debug.LogError("No prefab set for type: " + typeName);
//            return;
//        }
//        #endregion

//        #region Create Folder (if missing)
//        if (data.folder == null)
//        {
//            data.folder = new GameObject(typeName + " Folder");
//            data.folder.transform.SetParent(transform);
//            data.folder.transform.localPosition = Vector3.zero;

//        }
//        #endregion

//        #region Get Next Available ID
//        int id = GetNextAvailableNumber(data.folder.transform, typeName);
//        #endregion

//        #region Spawn Character
//        GameObject instance = Instantiate(data.prefab, data.folder.transform);
//        instance.name = typeName + " " + id;
//        instance.transform.localPosition = Vector3.zero;
//        #endregion

//        #region Character Specific Setup
//        // Run setup depending on which type of character was spawned
//        if (typeName == "NPC")
//        {
//            SetupPatrolMovement(instance, id, typeName);
//            SetupRandomMovement(instance, id, typeName);
//        }
//        else if (typeName == "Guard" || typeName == "Zombie")
//        {
//            SetupPatrolMovement(instance, id, typeName);
//            SetupRandomMovement(instance, id, typeName);
//        }
//        #endregion
//    }
//    #endregion

//    #region Utility Functions
//    private int GetNextAvailableNumber(Transform parent, string prefix)
//    {
//        // Store all used numbers
//        HashSet<int> usedNumbers = new HashSet<int>();

//        // Loop through all children
//        foreach (Transform child in parent)
//        {
//            // If the child’s name starts with "prefix "
//            if (child.name.StartsWith(prefix + " "))
//            {
//                // Grab the number part after the prefix
//                string numberPart = child.name.Substring(prefix.Length + 1);

//                // Try to convert into an int
//                int num;
//                if (int.TryParse(numberPart, out num))
//                {
//                    usedNumbers.Add(num);
//                }
//            }
//        }

//        // Start counting from 1 until we find a free number
//        int next = 1;
//        while (usedNumbers.Contains(next))
//        {
//            next++;
//        }

//        return next;
//    }
//    #endregion

//    #region Setup Functions
//    private void SetupPatrolMovement(GameObject character, int id, string typeName)
//    {
//        // Spawn the patrol follow point
//        GameObject point = Instantiate(AIPatrolManager.PatrolFollowPoint, character.transform.parent);
//        point.name = "PatrolFollowPoint " + id;
//        point.transform.localPosition = Vector3.zero;

//        // NPC logic
//        if (typeName == "NPC")
//        {
//            NPCBrain brain = character.GetComponent<NPCBrain>();
//            if (brain != null) brain.PatrolFollowPoint = point.transform;

//            PatrolFollowPoint followPoint = point.GetComponent<PatrolFollowPoint>();
//            if (followPoint != null) followPoint.ItsFollower = character;
//            if (followPoint != null) followPoint.Manager = AIPatrolManager;
//        }

//        // Enemy logic
//        else if (typeName == "Guard" || typeName == "Zombie")
//        {
//            NPCStateMachine enemy = character.GetComponent<NPCStateMachine>();
//            if (enemy != null) enemy.PatrolFollowPoint = point.transform;

//            PatrolFollowPoint followPoint = point.GetComponent<PatrolFollowPoint>();
//            if (followPoint != null) followPoint.ItsFollower = character;
//            if (followPoint != null) followPoint.Manager = AIPatrolManager;
//        }
//    }

//    private void SetupRandomMovement(GameObject character, int id, string typeName)
//    {
//        // Spawn the random follow point
//        GameObject point = Instantiate(AIRandomManager.RandomFollowPoint, character.transform.parent);
//        point.name = "RandomFollowPoint " + id;
//        point.transform.localPosition = Vector3.zero;

//        // NPC logic
//        if (typeName == "NPC")
//        {
//            NPCBrain brain = character.GetComponent<NPCBrain>();
//            if (brain != null) brain.RandomFollowPoint = point.transform;

//            RandomFollowPoint followPoint = point.GetComponent<RandomFollowPoint>();
//            if (followPoint != null) followPoint.ItsFollower = character;
//            if (followPoint != null) followPoint.Manager = AIRandomManager;
//        }

//        // Enemy logic
//        else if (typeName == "Guard" || typeName == "Zombie")
//        {
//            NPCStateMachine enemy = character.GetComponent<NPCStateMachine>();
//            if (enemy != null) enemy.RandomFollowPoint = point.transform;

//            RandomFollowPoint followPoint = point.GetComponent<RandomFollowPoint>();
//            if (followPoint != null) followPoint.ItsFollower = character;
//            if (followPoint != null) followPoint.Manager = AIRandomManager;
//        }

//        // Add this point to the manager’s list
//        AIRandomManager.RandomFollowPoints.Add(point);
//    }
//    #endregion

//}
