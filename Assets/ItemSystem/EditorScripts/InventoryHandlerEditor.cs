using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(InventoryHandler))]
public class InventoryHandlerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		// Draw default inspector first
		DrawDefaultInspector();

		InventoryHandler inventory = (InventoryHandler)target;

		GUILayout.Space(10);
		GUILayout.Label("DEBUG CONTROLS", EditorStyles.boldLabel);

		#region modifying money buttons
		GUILayout.Label("Debug money", EditorStyles.boldLabel);
		inventory.addMoney = EditorGUILayout.IntField("Add money", inventory.addMoney);

		if (GUILayout.Button("Modify Money"))
		{
			if (!ApplicationPlaying()) return;

			inventory.AddMoney(inventory.addMoney);
		}
		#endregion

		GUILayout.Space(10);

		#region inventory resize buttons
		GUILayout.Label("Inventory Resizing", EditorStyles.boldLabel);
		inventory.modifyInventorySizeByThis = EditorGUILayout.IntField("Modify Inventory By", inventory.modifyInventorySizeByThis);

		if (GUILayout.Button("Modify Inventory Size"))
		{
			if (!ApplicationPlaying()) return;

			if (inventory.InventorySize + inventory.modifyInventorySizeByThis <= 0)
			{
				Debug.LogWarning("minimum inventory size is 1");
				return;
			}

			inventory.ModifyInventorySize(inventory.modifyInventorySizeByThis);
		}
		#endregion

		GUILayout.Space(10);

		#region adding items to inventory buttons
		GUILayout.Label("Item Adding", EditorStyles.boldLabel);
		inventory.itemToSpawn = (ItemDefinition)EditorGUILayout.ObjectField("Item To Spawn", inventory.itemToSpawn, typeof(ItemDefinition), false);
		inventory.itemToSpawnCount = EditorGUILayout.IntField("Item To Spawn Count", inventory.itemToSpawnCount);

		if (GUILayout.Button("Pick Up Specific Item"))
		{
			if (!ApplicationPlaying()) return;

			if (inventory.itemToSpawn == null)
			{
				Debug.LogError("no item specified in itemToSpawn field");
				return;
			}

			if (inventory.itemToSpawnCount > inventory.itemToSpawn.StackLimit)
			{
				Debug.LogWarning($"{inventory.itemToSpawnCount} is higher then stack limit of {inventory.itemToSpawn.StackLimit}, setting to max");
				inventory.itemToSpawnCount = inventory.itemToSpawn.StackLimit;
			}
			if (inventory.itemToSpawnCount < 1)
			{
				Debug.LogWarning($"{inventory.itemToSpawnCount} is smaller then minimum limit of 1, setting to 1");
				inventory.itemToSpawnCount = 1;
			}

			inventory.AddNewItemPickUp(TestInventoryManager.GenerateSpecificInventoryItem(inventory.itemToSpawn, inventory.itemToSpawnCount));
		}

		if (GUILayout.Button("Pick Up Random Item"))
		{
			if (!ApplicationPlaying()) return;

			inventory.AddNewItemPickUp(TestInventoryManager.GenerateRandomInventoryItem());
		}
		#endregion

		GUILayout.Space(10);

		#region debugging specified slot options
		GUILayout.Label("Debug Specific Slot", EditorStyles.boldLabel);
		inventory.actionEffectsStack = EditorGUILayout.Toggle("Action Effects Stack", inventory.actionEffectsStack);
		inventory.slotIndex = EditorGUILayout.IntField("Slot Index (0 = base)", inventory.slotIndex);
		#endregion

		#region destroy item button (TODO update to a proper way to destroy item)
		if (GUILayout.Button("Destory Item/stack"))
		{
			if (!ApplicationPlaying()) return;

			inventory.DropItem(inventory.slotIndex, inventory.actionEffectsStack);
		}
		#endregion

		#region drop item button (TODO: may need updating once drop item actually drops items)
		if (GUILayout.Button("Drop Item"))
		{
			if (!ApplicationPlaying()) return;

			inventory.DropItem(inventory.slotIndex, inventory.actionEffectsStack);
		}
		#endregion

		GUILayout.Space(10);

		#region buy/sell debug buttons
		GUILayout.Label("Debug Sell/buy", EditorStyles.boldLabel);

		if (GUILayout.Button("Sell Item In Player inventory Slot"))
		{
			if (!ApplicationPlaying()) return;

			inventory.SellItemInSlot(TestInventoryManager.Instance.npcInventory, inventory.slotIndex, inventory.actionEffectsStack);
		}
		if (GUILayout.Button("Buy Item In NPC Inventory Slot"))
		{
			if (!ApplicationPlaying()) return;

			inventory.BuyItemInSlot(TestInventoryManager.Instance.npcInventory, inventory.slotIndex, inventory.actionEffectsStack);
		}
		#endregion

		GUILayout.Space(10);

		#region resetting inventory button
		GUILayout.Label("Reset Inventory", EditorStyles.boldLabel);

		if (GUILayout.Button("Reset Inventory"))
		{
			if (!ApplicationPlaying()) return;

			inventory.ResetInventory();
		}
		#endregion
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
