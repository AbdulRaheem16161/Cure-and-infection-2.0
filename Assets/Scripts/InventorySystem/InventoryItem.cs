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
        if (itemDefinition == null) { Clear(); return; }

        this.itemDefinition = itemDefinition;
        this.currentStack = Mathf.Clamp(currentStack, 0, itemDefinition.StackLimit);
    }

    public void SetItemStack(int value)
    {
        currentStack = Mathf.Clamp(value,0, itemDefinition != null ? itemDefinition.StackLimit : 0);

        if (currentStack == 0)
            Clear();
    }

    private void Clear()
    {
        itemDefinition = null;
        currentStack = 0;
    }
}
