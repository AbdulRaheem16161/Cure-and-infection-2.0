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
		if (!InventoryService.ItemExists(item)) return;
		if (item.ItemDefinition is not ProjectileDefinition _) return;

		ammoCounts.Clear();

		foreach (InventoryItem inventoryItem in items)
		{
			if (!InventoryService.ItemExists(inventoryItem)) continue;

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

    #region Set Item In Slot
    public void SetItemInSlot(InventoryItem item, int slot)
    {
        items[slot] = item;
        if (item != null && item.ItemDefinitionNull)
            item.SetItemStack(0);

        OnContainerItemChanged?.Invoke(slot, item);
    }
    #endregion

    #region Add New Item
    /// <summary>
    /// add new items to first empty inventory slot, stacking if possible, but return unstacked items 
    /// </summary>
    public InventoryItem AddNewItem(InventoryItem newItem)
	{
        newItem = InventoryService.TryStackItem(this, newItem);
		if (newItem.CurrentStack <= 0) return newItem;

        //if we have any left that couldnt be stacked, try to add to first empty slot found
        if (ContainerFull())
		{
			Debug.LogWarning("item container full and cant add item");
			return newItem;
		}

        for (int i = 0; i < items.Length; i++)
        {
            if (!SlotExists(i) || InventoryService.ItemExists(items[i])) continue;

            Debug.Log($"added new item: {newItem.ItemDefinition.ItemName}");
            SetItemInSlot(newItem, i);
			break;
        }
        return newItem;
    }
    #endregion

    #region Remove Item From Slot
    public void RemoveItemsFromSlot(int slot, int stackToRemove, bool effectsStack)
    {
        if (!SlotExists(slot) || !InventoryService.ItemExists(items[slot]))
        {
            Debug.LogError($"no item exists in slot {slot}");
            return;
        }
        InventoryItem item = items[slot];

        if (effectsStack)
        {
            SetItemInSlot(null, slot);
            return;
        }

        item.RemoveItemStack(stackToRemove);
        SetItemInSlot(item, slot);
    }
    #endregion

    #region Split item In Slot
    public void SplitItemInSlot(int slot)
	{
		if (ContainerFull()) { Debug.LogWarning("Inventory full cant split item stack"); return; }

		InventoryItem item = Items[slot];

		if (!InventoryService.ItemExists(item)) { Debug.LogWarning($"No item in slot {slot}"); return; }
		if (item.CurrentStack <= 1) { Debug.LogWarning($"Cant split single item"); return; }

        int newStack = item.CurrentStack / 2;
        int keepStack = item.CurrentStack - newStack;

        item.SetItemStack(keepStack);
        AddNewItem(new InventoryItem(item.ItemDefinition, newStack));
        OnContainerItemChanged?.Invoke(slot, item);
	}
    #endregion

    #region ContainerItemChanged Invoke
	public void InvokeContainerItemChanged(int slot, InventoryItem item)
	{
        OnContainerItemChanged?.Invoke(slot, item);
    }
    #endregion

	#region reset container
	public void ResetContainer()
	{
		for (int i = 0; i < Items.Length; i++)
			SetItemInSlot(null, i);
	}
	#endregion

	#region inventory checks
	public bool ContainerFull()
	{
        int fullSlots = 0;
        foreach (InventoryItem item in Items)
        {
			if (InventoryService.ItemExists(item))
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
	#endregion
}
