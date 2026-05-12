using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(InventoryHandler))]
public class InventoryHandlerEditor : Editor
{
	private bool showDebugControls;

	public override void OnInspectorGUI()
	{
		// Draw default inspector first
		DrawDefaultInspector();

		InventoryHandler inventory = (InventoryHandler)target;

		GUILayout.Space(10);
		GUILayout.Label("DEBUG CONTROLS", EditorStyles.boldLabel);
		showDebugControls = EditorGUILayout.Toggle("Show Debug Controls", showDebugControls);

		if (!showDebugControls) return;

		GUILayout.Space(10);

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

			if (inventory.ItemContainer.ContainerSize + inventory.modifyInventorySizeByThis <= 0)
			{
				Debug.LogWarning("minimum inventory size is 1");
				return;
			}

			inventory.ItemContainer.ModifySize(inventory.modifyInventorySizeByThis, inventory.transform.position);
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
				inventory.itemToSpawnCount = inventory.itemToSpawn.StackLimit;
			else if (inventory.itemToSpawnCount <= 0)
				inventory.itemToSpawnCount = 1;

			inventory.ItemContainer.AddNewItem(new(inventory.itemToSpawn, ItemSpawner.GetItemStackCount(inventory.itemToSpawn)));
		}

		if (GUILayout.Button("Pick Up Random Item"))
		{
			if (!ApplicationPlaying()) return;

			inventory.ItemContainer.AddNewItem(ItemSpawner.GetRandomInventoryItem());
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
		inventory.actionEffectsStack = EditorGUILayout.Toggle("Action Effects Stack", inventory.actionEffectsStack);
		inventory.slotIndex = EditorGUILayout.IntField("Slot Index (0 = base)", inventory.slotIndex);

		if (GUILayout.Button("Sell Item In Player inventory Slot"))
		{
			if (!ApplicationPlaying()) return;

			InventoryHandler npcInventory = TestInventoryManager.Instance.npcObj.GetComponent<InventoryHandler>();
			inventory.SellItemInSlot(npcInventory, inventory.slotIndex, inventory.actionEffectsStack);
		}
		if (GUILayout.Button("Buy Item In NPC Inventory Slot"))
		{
			if (!ApplicationPlaying()) return;

			InventoryHandler npcInventory = TestInventoryManager.Instance.npcObj.GetComponent<InventoryHandler>();
			inventory.BuyItemInSlot(npcInventory, inventory.slotIndex, inventory.actionEffectsStack);
		}
		#endregion

		GUILayout.Space(10);

		#region resetting inventory button
		GUILayout.Label("Reset Inventory", EditorStyles.boldLabel);

		if (GUILayout.Button("Reset Inventory"))
		{
			if (!ApplicationPlaying()) return;

			inventory.ItemContainer.ResetContainer();
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
