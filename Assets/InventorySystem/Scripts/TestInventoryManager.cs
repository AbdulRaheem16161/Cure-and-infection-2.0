using System.Collections.Generic;
using UnityEngine;

public class TestInventoryManager : MonoBehaviour
{
	public static TestInventoryManager Instance;

	public InventoryHandler playerInventory;

	public InventoryHandler npcInventory;

	static System.Random systemRandom = new();

	public List<ItemDefinition> itemList = new();

	private void Awake()
	{
		Instance = this;
	}

	public static InventoryItem GenerateRandomInventoryItem()
	{
		ItemDefinition itemDefinition = Instance.itemList[systemRandom.Next(Instance.itemList.Count)];
		int stackCount = systemRandom.Next(1, itemDefinition.StackLimit + 1);

		return new(itemDefinition, stackCount);
	}
	public static InventoryItem GenerateSpecificInventoryItem(ItemDefinition item, int stackCount)
	{
		if (stackCount <= 0)
		{
			Debug.LogWarning($"stack count smaller then 1, setting to 1");
			stackCount = 1;
		}
		if (stackCount > item.StackLimit)
		{
			Debug.LogWarning($"stack count bigger then items stack limit {item.StackLimit}, setting to max");
			stackCount = item.StackLimit;
		}

		return new(item, stackCount);
	}
}
