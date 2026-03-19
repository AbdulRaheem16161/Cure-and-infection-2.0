using System;
using UnityEngine;

[Serializable]
public class InventoryItem
{
	[SerializeField] private ItemDefinition itemDefinition;
	[SerializeField] private int currentStack;

	//read only
	public ItemDefinition ItemDefinition => itemDefinition;
	public int CurrentStack => currentStack;
	public bool StackEmpty => currentStack <= 0;

	public InventoryItem(ItemDefinition itemDefinition, int currentStack)
	{
		this.itemDefinition = itemDefinition;
		this.currentStack = currentStack;
	}

	public void SetItemStack(int newStack)
	{
		currentStack = newStack;
	}
	public void AddItemStack(int stackToAdd)
	{
		currentStack += stackToAdd;
	}
	public void RemoveItemStack(int stackToRemove)
	{
		currentStack -= stackToRemove;
	}
}
