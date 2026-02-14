using UnityEngine;

public class InventoryHandler : MonoBehaviour
{
	[Header("Inventory Settings")]
	[SerializeField] private int InventorySize;
	[SerializeField] private InventoryItem[] inventoryItems;
	[SerializeField] private int Money;

	[Header("Debug Settings")]
	public int modifyInventorySizeByThis;

	public void InitilizeInventory()
	{
		inventoryItems = new InventoryItem[InventorySize];
	}

	#region adjust inventory size
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
				inventoryItems[i].ItemDefinition.DropItem();
			}
		}

		inventoryItems = newInventory;
	}
	#endregion

	#region money methods
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
	#region item pickup
	public void AddNewItemPickUp(ItemDefinition item, int currentStack)
	{
		InventoryItem newItem = new(item, currentStack);

		newItem = TryStackItem(newItem);

		if (newItem.CurrentStack > 0) //check stack count after try stacking
		{
			for (int i = 0; i < inventoryItems.Length; i++)
			{
				if (inventoryItems[i].ItemDefinition != null) continue; //filter for empty slots
				AddInventoryItemToSlot(newItem, i); //add to first empty slot
			}
		}
	}
	#endregion

	#region removing item stacks
	public void RemoveItemsFromSlot(int slot, int stackToRemove)
	{
		if (inventoryItems[slot] == null) return;
		if (inventoryItems[slot].ItemDefinition == null) return;

		inventoryItems[slot].RemoveItemStack(stackToRemove);
		if (inventoryItems[slot].CurrentStack <= 0)
			RemoveInventoryItemFromSlot(slot);
	}
	#endregion

	#region buying/selling items
	public void BuyItemInSlot(InventoryHandler sellersInventory, int slot, bool buyingStack)
	{
		if (sellersInventory.inventoryItems[slot] == null) return;
		if (sellersInventory.inventoryItems[slot].ItemDefinition == null) return;

		InventoryItem buyingItem = sellersInventory.inventoryItems[slot];

		ProcessTransaction(sellersInventory, buyingItem, slot, buyingStack, false);
		ProcessTransaction(this, buyingItem, slot, buyingStack, true);
	}
	public void SellItemInSlot(InventoryHandler buyersInventory, int slot, bool sellingStack)
	{
		if (inventoryItems[slot] == null) return;
		if (inventoryItems[slot].ItemDefinition == null) return;

		InventoryItem buyingItem = inventoryItems[slot];

		ProcessTransaction(this, buyingItem, slot, sellingStack, false);
		ProcessTransaction(buyersInventory, buyingItem, slot, sellingStack, true);
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
				Debug.LogWarning($"cant afford to buy {stackCount}x {item.ItemDefinition.ItemName}");
				return;
			}

			inventory.RemoveMoney(price);
			inventory.TryStackItem(new(item.ItemDefinition, stackCount));
		}
		else
		{
			if (!inventory.HasEnoughMoney(price)) //show in ui to player when ui exists
			{
				Debug.LogWarning($"buyer cant afford to buy {stackCount}x {item.ItemDefinition.ItemName}");
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
		inventoryItems[slot] = item;
	}
	private void RemoveInventoryItemFromSlot(int slot)
	{
		inventoryItems[slot] = null;
	}
	#endregion

	#region item stacking helpers
	private InventoryItem TryStackItem(InventoryItem itemToSack)
	{
		foreach (InventoryItem existingItem in inventoryItems)
		{
			if (existingItem.ItemDefinition == null) continue; //filter empty
			if (itemToSack.ItemDefinition != existingItem.ItemDefinition) continue; //filter for same item

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
			existingItem.AddItemStack(existingItem.ItemDefinition.StackLimit); //set max stack limit
			newStackCount -= existingItem.ItemDefinition.StackLimit; //carry overflow
			itemToSack.SetItemStack(newStackCount); //set to overflow
		}
		else
			existingItem.AddItemStack(newStackCount); //add to stack

		return itemToSack;
	}
	#endregion

	#region item unstacking helpers
	private void UnstackItem(int slot, int stackToRemove)
	{
		inventoryItems[slot].RemoveItemStack(stackToRemove);

		if (inventoryItems[slot].CurrentStack <= 0)
			RemoveInventoryItemFromSlot(slot);
	}
	#endregion
}
