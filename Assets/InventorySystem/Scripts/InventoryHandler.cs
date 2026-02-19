using UnityEngine;
using static UnityEditor.Progress;

public class InventoryHandler : MonoBehaviour
{
	#region inventory settings
	[Header("Inventory Settings")]
	[SerializeField] private int money;
	[SerializeField] private int inventorySize;
	[SerializeField] private InventoryItem[] inventoryItems;
	#endregion

    #region inventory readonly settings
	public int Money => money;
	public int InventorySize => inventorySize;
	public InventoryItem[] InventoryItems => inventoryItems;
	#endregion

	#region debug settings
	[Header("Debug Settings")]
	[HideInInspector] public int addMoney;
	[HideInInspector] public int modifyInventorySizeByThis;
	[HideInInspector] public bool actionEffectsStack = false;
	[HideInInspector] public int slotIndex = 0;
	[HideInInspector] public ItemDefinition itemToSpawn;
	[HideInInspector] public int itemToSpawnCount;
	#endregion

	private void Awake()
	{
		InitilizeInventory();
	}

	private void InitilizeInventory() //call on awake (can be changed)
	{
		inventoryItems = new InventoryItem[InventorySize];
	}

	#region adjust inventory size
	public void ModifyInventorySize(int sizeAdjustment)
	{
		if (sizeAdjustment > 0)
			IncreaseInventorySize(inventoryItems.Length + sizeAdjustment);
		else
			DecreaseInventorySize(inventoryItems.Length + sizeAdjustment);

		inventorySize = inventoryItems.Length;
	}
	private void IncreaseInventorySize(int newSize)
	{
		InventoryItem[] newInventory = new InventoryItem[newSize];

		for (int i = 0; i < inventoryItems.Length; i++) //copy items
			newInventory[i] = inventoryItems[i];

		inventoryItems = newInventory;
	}
	private void DecreaseInventorySize(int newSize)
	{
		InventoryItem[] newInventory = new InventoryItem[newSize];

		for (int i = 0; i < newSize; i++)
			newInventory[i] = inventoryItems[i];  //copy items

		for (int i = newSize; i < inventoryItems.Length; i++) //drop items on floor if they dont fit
		{
			if (ItemExists(inventoryItems[i]))
			{
				Debug.LogWarning($"Item {inventoryItems[i].ItemDefinition.ItemName} was dropped on the ground");
				DropItem(i, true);
			}
		}

		inventoryItems = newInventory;
	}
	#endregion

	#region modifying money
	public bool HasEnoughMoney(int cost)
	{
		if (cost > money)
			return false;
		else return true;
	}
	public void SetMoney(int moneyToSet)
	{
		money = moneyToSet;
	}
	public void AddMoney(int moneyToAdd)
	{
		money += moneyToAdd;
	}
	public void RemoveMoney(int moneyToRemove)
	{
		money -= moneyToRemove;
	}
	#endregion

	//ways to add/remove items to/from inventory
	#region item pickup (TODO handle destroying world items/leaving them if stack count not 0)
	public void AddNewItemPickUp(InventoryItem newItem)
	{
		newItem = TryStackItem(newItem);

		if (InventoryFull() && newItem.CurrentStack > 0)
		{
			Debug.LogWarning("inventory full and cant stack anymore items");
			return;
		}
		else if (newItem.CurrentStack > 0)
		{
			for (int i = 0; i < inventoryItems.Length; i++)
			{
				if (!SlotExists(i) || ItemExists(inventoryItems[i])) continue;

				Debug.Log($"added new item: {newItem.ItemDefinition.ItemName}");
				AddInventoryItemToSlot(newItem, i); //add to first empty slot
				return;
			}
		}
	}
	#endregion

	#region item drop (TODO: update so world item is spawned)
	public void DropItem(int slot, bool dropStack)
	{
		if (!SlotExists(slot) || !ItemExists(inventoryItems[slot]))
		{
			Debug.LogError($"no item exists in slot {slot}");
			return;
		}

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
		if (!SlotExists(slot) || !ItemExists(inventoryItems[slot]))
		{
			Debug.LogError($"no item exists in slot {slot}");
			return;
		}

		inventoryItems[slot].RemoveItemStack(stackToRemove);
		if (inventoryItems[slot].CurrentStack <= 0)
			RemoveInventoryItemFromSlot(slot);
	}
	#endregion

	#region buying/selling items
	public void BuyItemInSlot(InventoryHandler otherInventory, int slot, bool buyingStack)
	{
		if (InventoryFull() || otherInventory.InventoryFull()) return;
		if (!otherInventory.SlotExists(slot) || !ItemExists(otherInventory.inventoryItems[slot]))
		{
			Debug.LogError($"no item exists in slot {slot}");
			return;
		}

		InventoryItem item = otherInventory.inventoryItems[slot];
		ProcessTransaction(this, otherInventory, item, slot, buyingStack);
	}
	public void SellItemInSlot(InventoryHandler otherInventory, int slot, bool sellingStack)
	{
		if (InventoryFull() || otherInventory.InventoryFull()) return;
		if (!SlotExists(slot) || !ItemExists(inventoryItems[slot]))
		{
			Debug.LogError($"no item exists in slot {slot}");
			return;
		}

		InventoryItem item = inventoryItems[slot];
		ProcessTransaction(otherInventory, this, item, slot, sellingStack);
	}
	private void ProcessTransaction(InventoryHandler buyer, InventoryHandler seller, InventoryItem item, int slot, bool fullStack)
	{
		int stackCount = fullStack ? item.CurrentStack : 1;
		int price = item.ItemDefinition.ItemPrice * stackCount;

		if (!buyer.HasEnoughMoney(price))
		{
			Debug.LogWarning($"Buyer cannot afford {stackCount}x {item.ItemDefinition.ItemName} ({buyer.Money}/{price})");
			return;
		}

		if (buyer.InventoryFull())
		{
			Debug.LogWarning("Buyer inventory full.");
			return;
		}

		//money transfer
		buyer.RemoveMoney(price);
		seller.AddMoney(price);

		//item transfer
		buyer.AddNewItemPickUp(new(item.ItemDefinition, stackCount));
		seller.RemoveItemsFromSlot(slot, stackCount);
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

	#region reset inventory
	public void ResetInventory()
	{
		for (int i = 0; i < inventoryItems.Length; i++)
			RemoveInventoryItemFromSlot(i);
	}
	#endregion

	#region item stacking helpers
	private InventoryItem TryStackItem(InventoryItem itemToSack)
	{
		Debug.Log($"trying to stack item: {itemToSack.ItemDefinition.ItemName} ({itemToSack.CurrentStack}x)");
		foreach (InventoryItem existingItem in inventoryItems)
		{
			if (!ItemExists(existingItem) || !ItemDefinitionMatches(existingItem, itemToSack)) continue; //filter

			Debug.Log($"existing item: {existingItem.ItemDefinition.ItemName} with stack {existingItem.CurrentStack}");

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
			Debug.Log($"stacked existing item: {existingItem.ItemDefinition.ItemName} to {existingItem.CurrentStack}");
		}
		else
		{
			existingItem.AddItemStack(itemToSack.CurrentStack); //add to stack
			itemToSack.SetItemStack(0); //nothing left to stack
			Debug.Log($"stacked existing item: {existingItem.ItemDefinition.ItemName} to {existingItem.CurrentStack}");
		}

		return itemToSack;
	}
	#endregion

	#region item unstacking helpers
	private void UnstackItem(int slot, int stackToRemove)
	{
		if (!SlotExists(slot) || !ItemExists(inventoryItems[slot]))
		{
			Debug.LogError($"no item exists in slot {slot}");
			return;
		}

		inventoryItems[slot].RemoveItemStack(stackToRemove);

		if (inventoryItems[slot].CurrentStack <= 0)
			RemoveInventoryItemFromSlot(slot);
	}
	#endregion

	#region inventory checks
	public bool InventoryFull()
	{
		int fullSlots = 0;
		foreach (InventoryItem item in inventoryItems)
		{
			if (!ItemExists(item)) continue;
			fullSlots++;
		}

		if (InventorySize <= fullSlots)
			return true;
		else
			return false;
	}
	public bool SlotExists(int slot)
	{
		if (slot < 0 || slot >= inventoryItems.Length)
		{
			Debug.LogError("slot index out of range");
			return false;
		}
		return true;
	}
	public bool ItemExists(InventoryItem item)
	{
		if (item == null)
		{
			Debug.LogError("inventory item null");
			return false;
		}

		if (item.ItemDefinition == null)
			return false;

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
