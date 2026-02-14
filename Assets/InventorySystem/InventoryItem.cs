using System;

[Serializable]
public class InventoryItem
{
	public ItemDefinition ItemDefinition { get; private set; }

	public bool StackEmpty => CurrentStack <= 0;
	public int CurrentStack { get; private set; }

	public InventoryItem(ItemDefinition itemDefinition, int currentStack)
	{
		this.ItemDefinition = itemDefinition;
		this.CurrentStack = currentStack;
	}

	public void SetItemStack(int newStack)
	{
		CurrentStack = newStack;
	}
	public void AddItemStack(int stackToAdd)
	{
		CurrentStack += stackToAdd;
	}
	public void RemoveItemStack(int stackToRemove)
	{
		CurrentStack -= stackToRemove;
	}
}
