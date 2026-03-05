using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemContainer : IAmmoGiver
{
	[SerializeField] private InventoryItem[] items;

	public int ContainerSize => items.Length;
	public InventoryItem[] Items => items;

	public event Action<int> OnContainerSizeChanged;
	public event Action<int, InventoryItem> OnContainerItemChanged;

	private Dictionary<ProjectileDefinition, int> ammoCounts = new();
	public Dictionary<ProjectileDefinition, int> AmmoCounts => ammoCounts;

	#region constructor
	public ItemContainer(int initialSize)
	{
		items = new InventoryItem[initialSize];
		OnContainerItemChanged += RecalculateAmmoCounts;
	}
	#endregion

	#region adjust container size
	public void ModifySize(int sizeAdjustment)
	{
		if (sizeAdjustment > 0)
			IncreaseSize(items.Length + sizeAdjustment);
		else
			DecreaseSize(items.Length + sizeAdjustment);

		OnContainerSizeChanged?.Invoke(items.Length);
	}
	private void IncreaseSize(int newSize)
	{
		InventoryItem[] newInventory = new InventoryItem[newSize];

		for (int i = 0; i < items.Length; i++) //copy items
			newInventory[i] = items[i];

		items = newInventory;
	}
	private void DecreaseSize(int newSize)
	{
		InventoryItem[] newInventory = new InventoryItem[newSize];

		for (int i = 0; i < newSize; i++)
			newInventory[i] = items[i];  //copy items

		for (int i = newSize; i < items.Length; i++) //drop items on floor if they dont fit
		{
			if (ItemExists(items[i]))
			{
				Debug.LogWarning($"Item {items[i].ItemDefinition.ItemName} was dropped on the ground");
				DropItem(i, true);
			}
		}

		items = newInventory;
	}
	#endregion

	#region ammo counting + checking
	private void RecalculateAmmoCounts(int _, InventoryItem item)
	{
		if (!ItemExists(item)) return;
		if (item.ItemDefinition is not ProjectileDefinition _) return;

		ammoCounts.Clear();

		foreach (InventoryItem inventoryItem in items)
		{
			if (!ItemExists(inventoryItem)) continue;

			if (inventoryItem.ItemDefinition is ProjectileDefinition projectileDef)
			{
				if (ammoCounts.ContainsKey(projectileDef))
					ammoCounts[projectileDef] += inventoryItem.CurrentStack;
				else
					ammoCounts[projectileDef] = inventoryItem.CurrentStack;
			}
		}
	}
	#endregion

	#region ammo interface methods
	public int GetAmmo(ProjectileDefinition projectileDefinition, int amountNeeded)
	{
		return amountNeeded;
	}
	public int TakeAmmo(ProjectileDefinition projectileDefinition, int amountNeeded)
	{
		int ammoFound = 0;

		foreach (var item in Items) //collect ammo needed
		{
			if (item == null) continue;
			if (item.ItemDefinition is not ProjectileDefinition pd) continue;
			if (pd != projectileDefinition) continue;

			int remainingNeeded = amountNeeded - ammoFound;

			if (item.CurrentStack <= remainingNeeded)
			{
				ammoFound += item.CurrentStack;
				item.RemoveItemStack(item.CurrentStack);
			}
			else
			{
				ammoFound += remainingNeeded;
				item.RemoveItemStack(remainingNeeded);
			}

			if (ammoFound >= amountNeeded)
				break;
		}

		return ammoFound;
	}
	public bool AmmoAvailable(ProjectileDefinition projectileDefinition)
	{
		return AmmoCounts.TryGetValue(projectileDefinition, out int count) && count > 0;
	}
	#endregion

	#region item adding
	/// <summary>
	/// add new items to inventory, by default trying to stack them
	/// </summary>
	public void AddNewItem(InventoryItem newItem, bool tryStack = true)
	{
		if (tryStack)
			newItem = TryStackNewItem(newItem);

		if (ContainerFull() && newItem.CurrentStack > 0)
		{
			//leave world item on ground (stack already updated)
			Debug.LogWarning("inventory full and cant stack anymore items");
			return;
		}
		else if (newItem.CurrentStack > 0)
		{
			for (int i = 0; i < items.Length; i++)
			{
				if (!SlotExists(i) || ItemExists(items[i])) continue;

				Debug.Log($"added new item: {newItem.ItemDefinition.ItemName}");
				AddInventoryItemToSlot(i, newItem); //add to first empty slot
				return;
			}
		}
	}
	#endregion

	#region move items to specific slot methods
	public void SwapItemsInSlots(int currentSlot, int newSlot)
	{
		InventoryItem itemInCurrentSlot = Items[currentSlot];
		InventoryItem itemInNewSlot = Items[newSlot];

		if (StackedExistingItems(currentSlot, newSlot)) return; //return early if successful

		AddInventoryItemToSlot(newSlot, itemInCurrentSlot);

		if (!ItemExists(itemInNewSlot))
		{
			RemoveInventoryItemFromSlot(currentSlot);
			return;
		}

		AddInventoryItemToSlot(currentSlot, itemInNewSlot);
	}
	#endregion

	#region splitting items
	public void SplitItem(int slot)
	{
		if (ContainerFull())
		{
			Debug.LogWarning("Inventory full cant split item stack");
		}

		InventoryItem item = Items[slot];

		if (!ItemExists(item))
		{
			Debug.LogWarning($"no item in slot {slot}");
			return;
		}

		int originalStack = item.CurrentStack / 2;      // floor division
		int newStack = item.CurrentStack - originalStack; // remainder

		// Reduce original stack
		item.RemoveItemStack(newStack); // remove amount going to new stack
		OnContainerItemChanged?.Invoke(slot, item);

		// Create new item with remaining stack
		AddNewItem(new InventoryItem(item.ItemDefinition, newStack), false);
	}
	#endregion

	#region dropping items (TODO: update so world item is spawned)
	public void DropItem(int slot, bool dropStack)
	{
		if (!SlotExists(slot) || !ItemExists(items[slot]))
		{
			Debug.LogError($"no item exists in slot {slot}");
			return;
		}

		InventoryItem itemToDrop = items[slot];

		if (dropStack)
			RemoveItemsFromSlot(slot, itemToDrop.CurrentStack);
		else
			RemoveItemsFromSlot(slot, 1);

		//spawn world item
	}
	#endregion

	#region removing items
	public void RemoveItemsFromSlot(int slot, int stackToRemove)
	{
		if (!SlotExists(slot) || !ItemExists(items[slot]))
		{
			Debug.LogError($"no item exists in slot {slot}");
			return;
		}
		InventoryItem item = items[slot];

		item.RemoveItemStack(stackToRemove);
		OnContainerItemChanged?.Invoke(slot, item);

		if (item.CurrentStack <= 0)
			RemoveInventoryItemFromSlot(slot);
	}
	#endregion

	#region buying/selling items
	public void BuyItemInSlot(InventoryHandler inventory, InventoryHandler otherInventory, int slot, bool buyingStack)
	{
		if (ContainerFull() || otherInventory.ItemContainer.ContainerFull()) return;
		if (!otherInventory.ItemContainer.SlotExists(slot) || !ItemExists(otherInventory.ItemContainer.Items[slot]))
		{
			Debug.LogError($"no item exists in slot {slot}");
			return;
		}

		InventoryItem item = otherInventory.ItemContainer.Items[slot];
		ProcessTransaction(inventory, otherInventory, item, slot, buyingStack);
	}
	public void SellItemInSlot(InventoryHandler inventory, InventoryHandler otherInventory, int slot, bool sellingStack)
	{
		if (ContainerFull() || otherInventory.ItemContainer.ContainerFull()) return;
		if (!SlotExists(slot) || !ItemExists(items[slot]))
		{
			Debug.LogError($"no item exists in slot {slot}");
			return;
		}

		InventoryItem item = items[slot];
		ProcessTransaction(otherInventory, inventory, item, slot, sellingStack);
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

		if (buyer.ItemContainer.ContainerFull())
		{
			Debug.LogWarning("Buyer inventory full.");
			return;
		}

		//money transfer
		buyer.RemoveMoney(price);
		seller.AddMoney(price);

		//item transfer
		buyer.ItemContainer.AddNewItem(new(item.ItemDefinition, stackCount));
		seller.RemoveItemsFromSlot(slot, stackCount);
	}
	#endregion

	#region adding/removing InventoryItem to/from inventory
	private void AddInventoryItemToSlot(int slot, InventoryItem item)
	{
		if (!SlotExists(slot)) return;
		Items[slot] = item;
		OnContainerItemChanged?.Invoke(slot, item);
	}
	private void RemoveInventoryItemFromSlot(int slot)
	{
		if (!SlotExists(slot)) return;
		Items[slot] = null;
		OnContainerItemChanged?.Invoke(slot, null);
	}
	#endregion

	#region item stacking helpers
	private bool StackedExistingItems(int currentSlot, int newSlot) //bool used to check fail or success
	{
		InventoryItem itemInCurrentSlot = Items[currentSlot];
		InventoryItem itemInNewSlot = Items[newSlot];

		if (!ItemExists(itemInCurrentSlot) || !ItemExists(itemInNewSlot) || !ItemDefinitionMatches(itemInCurrentSlot, itemInNewSlot)) return false;

		itemInCurrentSlot = StackItem(newSlot, itemInNewSlot, itemInCurrentSlot);
		OnContainerItemChanged?.Invoke(newSlot, itemInNewSlot);
		OnContainerItemChanged?.Invoke(currentSlot, itemInCurrentSlot);

		if (itemInCurrentSlot.CurrentStack <= 0)
			RemoveInventoryItemFromSlot(currentSlot);

		return true;
	}
	private InventoryItem TryStackNewItem(InventoryItem itemToStack)
	{
		Debug.Log($"trying to stack new item: {itemToStack.ItemDefinition.ItemName} ({itemToStack.CurrentStack}x)");

		for (int i = 0; i < Items.Length; i++)
		{
			InventoryItem existingItem = Items[i];
			if (!ItemExists(existingItem) || !ItemDefinitionMatches(existingItem, itemToStack)) continue; //filter

			Debug.Log($"existing item: {existingItem.ItemDefinition.ItemName} with stack {existingItem.CurrentStack}");

			if (existingItem.CurrentStack < existingItem.ItemDefinition.StackLimit) //check for valid stack space
				itemToStack = StackItem(i, existingItem, itemToStack); //stack item
		}

		return itemToStack;
	}
	private InventoryItem StackItem(int slot, InventoryItem existingItem, InventoryItem itemToSack)
	{
		int newStackCount = existingItem.CurrentStack + itemToSack.CurrentStack;

		if (newStackCount > existingItem.ItemDefinition.StackLimit) //handle stacking overflow
		{
			existingItem.SetItemStack(existingItem.ItemDefinition.StackLimit); //set max stack limit
			newStackCount -= existingItem.ItemDefinition.StackLimit; //carry overflow
			itemToSack.SetItemStack(newStackCount); //set to overflow
		}
		else
		{
			existingItem.AddItemStack(itemToSack.CurrentStack); //add to stack
			itemToSack.SetItemStack(0); //nothing left to stack
		}

		OnContainerItemChanged?.Invoke(slot, existingItem);
		Debug.Log($"stacked item: {existingItem.ItemDefinition.ItemName} to {existingItem.CurrentStack}");

		return itemToSack;
	}
	#endregion

	#region item unstacking helpers
	private void UnstackItem(int slot, int stackToRemove)
	{
		if (!SlotExists(slot) || !ItemExists(Items[slot]))
		{
			Debug.LogError($"no item exists in slot {slot}");
			return;
		}

		Items[slot].RemoveItemStack(stackToRemove);

		if (Items[slot].CurrentStack <= 0)
			RemoveInventoryItemFromSlot(slot);
		else
		{
			InventoryItem item = Items[slot];
			OnContainerItemChanged?.Invoke(slot, Items[slot]);
			Debug.Log($"unstacked item: {item.ItemDefinition.ItemName} to {item.CurrentStack}");
		}
	}
	#endregion

	#region look for item
	/// <summary>
	/// look for item via definition, optionally include looking for amount
	/// </summary>
	public InventoryItem LookForItem(ItemDefinition itemDef)
	{
		foreach (InventoryItem item in Items)
		{
			if (!ItemDefinitionMatches(item, new(itemDef, 1))) continue;
			return item;
		}
		return null;
	}
	#endregion

	#region reset container
	public void ResetContainer()
	{
		for (int i = 0; i < Items.Length; i++)
			RemoveInventoryItemFromSlot(i);
	}
	#endregion

	#region inventory checks
	public bool ContainerFull()
	{
		int fullSlots = 0;
		foreach (InventoryItem item in Items)
		{
			if (!ItemExists(item)) continue;
			fullSlots++;
		}

		if (Items.Length <= fullSlots)
			return true;
		else
			return false;
	}
	public bool SlotExists(int slot)
	{
		if (slot < 0 || slot >= Items.Length)
		{
			Debug.LogError("slot index out of range");
			return false;
		}
		return true;
	}
	public bool ItemExists(InventoryItem item)
	{
		if (item == null)
			return false;

		if (item.ItemDefinition == null)
			return false;

		return true;
	}
	public bool ItemDefinitionMatches(InventoryItem itemOne, InventoryItem itemTwo)
	{
		if (itemOne.ItemDefinition == itemTwo.ItemDefinition)
			return true;
		else
			return false;
	}
	#endregion
}
