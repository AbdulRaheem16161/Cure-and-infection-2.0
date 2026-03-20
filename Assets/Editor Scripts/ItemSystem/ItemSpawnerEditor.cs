using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemSpawner), true)]
public class ItemSpawnerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		ItemSpawner spawner = (ItemSpawner)target;

		EditorGUILayout.LabelField("Spawner Controls", EditorStyles.boldLabel);

		#region Spawn Item at world position
		EditorGUILayout.LabelField("Spawn Item at world position", EditorStyles.boldLabel);
		spawner.itemToSpawn = (ItemDefinition)EditorGUILayout.ObjectField("Item To Spawn", spawner.itemToSpawn, typeof(ItemDefinition), true);
		spawner.itemCountToSpawn = EditorGUILayout.IntField("Count To Spawn", spawner.itemCountToSpawn);
		spawner.locationToSpawnItem = EditorGUILayout.Vector3Field("Spawn Position", spawner.locationToSpawnItem);

		if (GUILayout.Button("Spawn World Item At"))
		{
			if (!ApplicationPlaying()) return;
			ItemSpawner.GetItem(spawner.itemToSpawn, spawner.itemCountToSpawn, null, spawner.locationToSpawnItem, Quaternion.identity);
		}
		#endregion

		EditorGUILayout.Space(10);

		#region remove WorldItem
		EditorGUILayout.LabelField("Remove Item From World", EditorStyles.boldLabel);
		spawner.worldItemToCleanUp = (Item)EditorGUILayout.ObjectField("Remove World Item", spawner.worldItemToCleanUp, typeof(Item), true);

		if (GUILayout.Button("Remove World Item"))
		{
			if (!ApplicationPlaying()) return;

			if (spawner.itemCountToSpawn > spawner.itemToSpawn.StackLimit)
				spawner.itemCountToSpawn = spawner.itemToSpawn.StackLimit;
			else if (spawner.itemCountToSpawn <= 0)
				spawner.itemCountToSpawn = 1;

			if (spawner.worldItemToCleanUp.IsInHands || spawner.worldItemToCleanUp.IsEquipped)
			{
				Debug.LogError("Cannot remove a world item currently equipped by an EquipmentHandler");
				return;
			}
			spawner.worldItemToCleanUp.CleanUpItem();
		}
		#endregion

		serializedObject.ApplyModifiedProperties();
	}

	private bool ApplicationPlaying()
	{
		if (!Application.isPlaying)
		{
			Debug.LogWarning("Must be in Play Mode");
			return false;
		}

		return true;
	}
}
