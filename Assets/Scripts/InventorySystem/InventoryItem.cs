using System;
using UnityEngine;

[Serializable]
public class InventoryItem
{
	[SerializeField] private ItemDefinition itemDefinition;
	[SerializeField] private int currentStack;

	//read only
	public bool ItemDefinitionNull => itemDefinition == null;
	public ItemDefinition ItemDefinition => itemDefinition;
	public int CurrentStack => currentStack;
	public bool StackEmpty => currentStack <= 0;

	public InventoryItem(ItemDefinition itemDefinition, int currentStack)
	{
		this.itemDefinition = itemDefinition;
        this.currentStack = Mathf.Max(0, currentStack);
    }

    public void SetItemStack(int newStack)
	{
        currentStack = Mathf.Max(0, newStack);
    }
	public void AddItemStack(int stackToAdd)
	{
        currentStack += Mathf.Max(0, stackToAdd);
    }
	public void RemoveItemStack(int stackToRemove)
	{
		currentStack -= stackToRemove;
	}

	public bool CanStackWith(InventoryItem otherItem)
    {
        if (ItemDefinitionNull || otherItem == null || otherItem.ItemDefinitionNull && otherItem.currentStack <= 0) return false;
        return ItemDefinitionMatches(otherItem);
    }
    public bool ItemDefinitionMatches(InventoryItem otherItem)
    {
        if (ItemDefinition == otherItem.ItemDefinition)
            return true;
        else
            return false;
    }
}
