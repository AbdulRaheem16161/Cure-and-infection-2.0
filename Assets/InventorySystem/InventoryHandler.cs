using UnityEngine;
using static UnityEditor.Progress;

public class InventoryHandler : MonoBehaviour
{
	[Header("Inventory Settings")]
	[SerializeField] private int InventorySize;
	[SerializeField] private InventoryItem[] inventoryItems;
	[SerializeField] private int Money;

	[Header("Debug Settings")]
	[HideInInspector] public int modifyInventorySizeByThis;
	[HideInInspector] public bool actionEffectsStack = false;
	[HideInInspector] public int slotIndex = 0;
	[HideInInspector] public ItemDefinition itemToSpawn;
	[HideInInspector] public int itemToSpawnCount;

	private void Awake()
	{
		InitilizeInventory();
	}

	private void InitilizeInventory() //call on awake (can be changed)
	{
		inventoryItems = new InventoryItem[InventorySize];
	}

	#region adjust inventory size (TODO methods need testing for bugs)
	public void ModifyInventorySize(int sizeAdjustment)
	{
		if (sizeAdjustment > 0)
			IncreaseInventorySize(inventoryItems.Length + sizeAdjustment);
		else
			DecreaseInventorySize(inventoryItems.Length - sizeAdjustment);
	}
	public void IncreaseInventorySize(int newSize)
	{
		InventoryItem[] newInventory = new InventoryItem[newSize];

		for (int i = 0; i < inventoryItems.Length; i++) //copy items
			newInventory[i] = inventoryItems[i];

		inventoryItems = newInventory;
	}
	public void DecreaseInventorySize(int newSize)
	{
		InventoryItem[] newInventory = new InventoryItem[newSize];

		for (int i = 0; i < newSize; i++)
			newInventory[i] = inventoryItems[i];  //copy items

		for (int i = newSize; i < inventoryItems.Length; i++) //drop items on floor if they dont fit
		{
			if (inventoryItems[i] != null)
			{
				Debug.LogWarning($"Item {inventoryItems[i].ItemDefinition.ItemName} was dropped on the ground");
				DropItem(i, true);
			}
		}

		inventoryItems = newInventory;
	}
	#endregion

	#region money methods (TODO add and test methods individually, currently seem to work with buying/selling tho)
	public bool HasEnoughMoney(int cost)
	{
		if (cost > Money)
			return false;
		else return true;
	}
	public void SetMoney(int moneyToSet)
	{
		Money = moneyToSet;
	}
	public void AddMoney(int moneyToAdd)
	{
		Money += moneyToAdd;
	}
	public void RemoveMoney(int moneyToRemove)
	{
		Money -= moneyToRemove;
	}
	#endregion

	//ways to add/remove items to/from inventory
	#region item pickup (TODO handle inventory full but same item has stack space, handle destroying world items/leaving them if stack count not 0)
	public void AddNewItemPickUp(InventoryItem newItem)
	{
		if (InventoryFull())
		{
			Debug.LogWarning("inventory full");
			return;
		}

		newItem = TryStackItem(newItem);

		if (newItem.CurrentStack > 0) //check stack count after try stacking
		{
			for (int i = 0; i < inventoryItems.Length; i++)
			{
				if (!SlotExists(i, true) || ItemExists(inventoryItems[i], true)) continue;

				Debug.LogError($"added new item: {newItem.ItemDefinition.ItemName}");
				AddInventoryItemToSlot(newItem, i); //add to first empty slot
				return;
			}
		}
	}
	#endregion

	#region item drop (TODO: update so world item is spawned)
	public void DropItem(int slot, bool dropStack)
	{
		if (!SlotExists(slot, true) || !ItemExists(inventoryItems[slot], true)) return;

		InventoryItem itemToDrop = inventoryItems[slot];

		if (dropStack)
			RemoveItemsFromSlot(slot, itemToDrop.CurrentStack);
		else
			RemoveItemsFromSlot(slot, 1);
	}
	#endregion

	#region removing items
	public void RemoveItemsFromSlot(int slot, int stackToRemove)
	{
		if (!SlotExists(slot, true) || !ItemExists(inventoryItems[slot], true)) return;

		inventoryItems[slot].RemoveItemStack(stackToRemove);
		if (inventoryItems[slot].CurrentStack <= 0)
			RemoveInventoryItemFromSlot(slot);
	}
	#endregion

	#region buying/selling items (TODO: buying method still needs testing)
	public void BuyItemInSlot(InventoryHandler sellersInventory, int slot, bool buyingStack)
	{
		if (InventoryFull(true)) return;
		if (!SlotExists(slot, true) || !ItemExists(inventoryItems[slot], true)) return;

		InventoryItem item = sellersInventory.inventoryItems[slot];

		ProcessTransaction(sellersInventory, item, slot, buyingStack, false);
		ProcessTransaction(this, item, slot, buyingStack, true);
	}
	public void SellItemInSlot(InventoryHandler buyersInventory, int slot, bool sellingStack)
	{
		if (buyersInventory.InventoryFull(true)) return;
		if (!SlotExists(slot, true) || !ItemExists(inventoryItems[slot], true)) return;

		InventoryItem item = inventoryItems[slot];

		ProcessTransaction(buyersInventory, item, slot, sellingStack, true);
		ProcessTransaction(this, item, slot, sellingStack, false);
	}
	private void ProcessTransaction(InventoryHandler inventory, InventoryItem item, int slot, bool sellingStack, bool isBuying)
	{
		int price = item.ItemDefinition.ItemPrice;
		int stackCount = 1;

		if (sellingStack)
		{
			price *= item.CurrentStack;
			stackCount = item.CurrentStack;
		}

		if (isBuying)
		{
			if (!inventory.HasEnoughMoney(price)) //show in ui to player when ui exists
			{
				Debug.LogWarning($"cant afford to buy {stackCount}x {item.ItemDefinition.ItemName} : {inventory.Money}/{price}");
				return;
			}

			inventory.RemoveMoney(price);
			inventory.AddNewItemPickUp(new(item.ItemDefinition, stackCount));
		}
		else
		{
			if (!inventory.HasEnoughMoney(price)) //show in ui to player when ui exists
			{
				Debug.LogWarning($"buyer cant afford to buy {stackCount}x {item.ItemDefinition.ItemName} : {inventory.Money}/{price}");
				return;
			}

			inventory.AddMoney(price);
			inventory.RemoveItemsFromSlot(slot, stackCount);
		}
	}
	#endregion

	#region adding/removing InventoryItem to/from inventory
	private void AddInventoryItemToSlot(InventoryItem item, int slot)
	{
		if (!SlotExists(slot)) return;
		inventoryItems[slot] = item;
	}
	private void RemoveInventoryItemFromSlot(int slot)
	{
		if (!SlotExists(slot)) return;
		inventoryItems[slot] = null;
	}
	#endregion

	#region reset inventory (TODO needs testing)
	public void ResetInventory()
	{
		for (int i = 0; i < inventoryItems.Length; i++)
			RemoveInventoryItemFromSlot(i);
	}
	#endregion

	#region item stacking helpers
	private InventoryItem TryStackItem(InventoryItem itemToSack)
	{
		Debug.LogError($"trying to stack item: {itemToSack.ItemDefinition.ItemName} ({itemToSack.CurrentStack}x)");
		foreach (InventoryItem existingItem in inventoryItems)
		{
			if (!ItemExists(existingItem) || !ItemDefinitionMatches(existingItem, itemToSack)) continue; //filter

			Debug.LogError($"existing item: {existingItem.ItemDefinition.ItemName} with stack {existingItem.CurrentStack}");

			if (existingItem.CurrentStack < existingItem.ItemDefinition.StackLimit) //check for valid stack space
				itemToSack = StackItem(existingItem, itemToSack); //stack item
		}

		return itemToSack;
	}
	private InventoryItem StackItem(InventoryItem existingItem, InventoryItem itemToSack)
	{
		int newStackCount = existingItem.CurrentStack + itemToSack.CurrentStack;

		if (newStackCount > existingItem.ItemDefinition.StackLimit) //handle stacking overflow
		{
			existingItem.SetItemStack(existingItem.ItemDefinition.StackLimit); //set max stack limit
			newStackCount -= existingItem.ItemDefinition.StackLimit; //carry overflow
			itemToSack.SetItemStack(newStackCount); //set to overflow
			Debug.LogError($"stacked existing item: {existingItem.ItemDefinition.ItemName} to {existingItem.CurrentStack}");
		}
		else
		{
			existingItem.AddItemStack(itemToSack.CurrentStack); //add to stack
			itemToSack.SetItemStack(0); //nothing left to stack
			Debug.LogError($"stacked existing item: {existingItem.ItemDefinition.ItemName} to {existingItem.CurrentStack}");
		}

		return itemToSack;
	}
	#endregion

	#region item unstacking helpers
	private void UnstackItem(int slot, int stackToRemove)
	{
		if (!SlotExists(slot) || !ItemExists(inventoryItems[slot])) return;

		inventoryItems[slot].RemoveItemStack(stackToRemove);

		if (inventoryItems[slot].CurrentStack <= 0)
			RemoveInventoryItemFromSlot(slot);
	}
	#endregion

	#region inventory checks
	public bool InventoryFull(bool log = false)
	{
		int fullSlots = 0;
		foreach (InventoryItem item in inventoryItems)
		{
			if (!ItemExists(item)) continue;
			fullSlots++;
		}

		if (InventorySize <= fullSlots)
		{
			if (log)
				Debug.LogWarning("inventory is full");
			return true;
		}
		else
			return false;
	}
	public bool SlotExists(int slot, bool log = false)
	{
		if (slot < 0 || slot >= inventoryItems.Length)
		{
			if (log) Debug.LogError($"Invalid slot index {slot}. Inventory size is 0-{inventoryItems.Length - 1}");
			return false;
		}
		return true;
	}
	public bool ItemExists(InventoryItem item, bool log = false)
	{
		if (item == null)
		{
			if (log) Debug.LogError("Item reference null");
			return false;
		}

		if (item.ItemDefinition == null)
		{
			if (log) Debug.LogError($"InventoryItems, ItemDefinition is null");
			return false;
		}

		return true;
	}
	private bool ItemDefinitionMatches(InventoryItem itemOne, InventoryItem itemTwo)
	{
		if (itemOne.ItemDefinition == itemTwo.ItemDefinition)
			return true;
		else
			return false;
	}
	#endregion
}
