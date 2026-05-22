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
	public void SetContainerSize(int newSize)
	{
        InventoryItem[] newInventory = new InventoryItem[newSize];

        for (int i = 0; i < Mathf.Min(items.Length, newSize); i++)
            newInventory[i] = items[i];

        items = newInventory;
        OnContainerSizeChanged?.Invoke(items.Length);
    }
	#endregion

	#region ammo counting + checking
	private void RecalculateAmmoCounts(int _, InventoryItem item)
	{
		if (!InventoryItemExists(item)) return;
		if (item.ItemDefinition is not ProjectileDefinition _) return;

		ammoCounts.Clear();

		foreach (InventoryItem inventoryItem in items)
		{
			if (!InventoryItemExists(inventoryItem)) continue;

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

    #region Add New Item
    /// <summary>
    /// add new items to inventory, by default trying to stack them
    /// </summary>
    public void AddNewItem(InventoryItem newItem, bool tryStack = true)
	{
		if (tryStack)
			newItem = TryStackItem(newItem);

		if (ContainerFull() && newItem.CurrentStack > 0) //leave rest of stack (stack should update correctly internally)
		{
			Debug.LogWarning("inventory full and cant stack anymore items");
			return;
		}
		else if (newItem.CurrentStack > 0)
		{
			for (int i = 0; i < items.Length; i++)
			{
				if (!SlotExists(i) || InventoryItemExists(items[i])) continue;

				Debug.Log($"added new item: {newItem.ItemDefinition.ItemName}");
				AddInventoryItemToSlot(i, newItem); //add to first empty slot
				return;
			}
		}
	}
	#endregion

	#region move items to specific slot methods
	public void MoveItemToSlot(int currentSlot, int newSlot)
	{
        if (!SlotExists(currentSlot) || !SlotExists(newSlot)) return;

        (Items[currentSlot], Items[newSlot]) = (Items[newSlot], Items[currentSlot]);

        OnContainerItemChanged?.Invoke(currentSlot, Items[currentSlot]);
        OnContainerItemChanged?.Invoke(newSlot, Items[newSlot]);
    }
	#endregion

	#region splitting items
	public void SplitItem(int slot)
	{
		if (ContainerFull()) { Debug.LogWarning("Inventory full cant split item stack"); return; }

		InventoryItem item = Items[slot];

		if (!InventoryItemExists(item)) { Debug.LogWarning($"No item in slot {slot}"); return; }
		if (item.CurrentStack <= 1) { Debug.LogWarning($"Cant split single item"); return; }

        int newStack = item.CurrentStack / 2;
        int keepStack = item.CurrentStack - newStack;

        item.SetItemStack(keepStack);
        AddNewItem(new InventoryItem(item.ItemDefinition, newStack), false);
        OnContainerItemChanged?.Invoke(slot, item);
	}
	#endregion

	#region removing items
	public void RemoveItemsFromSlot(int slot, int stackToRemove, bool effectsStack)
	{
		if (!SlotExists(slot) || !InventoryItemExists(items[slot]))
		{
			Debug.LogError($"no item exists in slot {slot}");
			return;
		}
		InventoryItem item = items[slot];

		if (effectsStack)
		{
            RemoveInventoryItemFromSlot(slot);
            return;
		}

		item.RemoveItemStack(stackToRemove);
		OnContainerItemChanged?.Invoke(slot, item);

		if (item.CurrentStack <= 0)
			RemoveInventoryItemFromSlot(slot);
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
    private InventoryItem TryStackItem(InventoryItem itemToStack, bool logOutcome = false)
    {
		if (logOutcome) Debug.Log($"trying to stack new item: {itemToStack.ItemDefinition.ItemName} ({itemToStack.CurrentStack}x)");

        for (int i = 0; i < Items.Length; i++)
        {
            InventoryItem existingItem = Items[i];
            if (!InventoryItemExists(existingItem) || !existingItem.CanStackWith(itemToStack)) continue; //filter

            if (logOutcome) Debug.Log($"existing item: {existingItem.ItemDefinition.ItemName} with stack {existingItem.CurrentStack}");

            if (existingItem.CurrentStack < existingItem.ItemDefinition.StackLimit) //check for valid stack space
                itemToStack = StackItem(i, existingItem, itemToStack); //stack item

            if (itemToStack.CurrentStack <= 0)
                return itemToStack;
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
			if (InventoryItemExists(item))
                fullSlots++;
        }

        if (Items.Length <= fullSlots)
			return true;
		else
			return false;
	}
	public bool SlotExists(int slot)
	{
		if (slot < 0 || slot >= Items.Length) { Debug.LogError("slot index out of range"); return false; }
		return true;
	}
	public bool InventoryItemExists(InventoryItem item)
	{
		if (item == null || item.ItemDefinitionNull) 
			return false;
		else 
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
