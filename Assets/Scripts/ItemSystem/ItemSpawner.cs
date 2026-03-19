using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
	public static ItemSpawner Instance { get; private set; }

	public Dictionary<ItemDefinition, List<Item>> itemObjectPooling = new();

	[Header("Item Prefab Objects")]
	public GameObject rangedWeaponPrefab;
	public GameObject meleeWeaponPrefab;
	public GameObject armourPrefab;
	public GameObject consumablePrefab;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void OnEnable()
	{
		Item<ItemDefinition>.OnCleanUpItem += CleanUpItemObject;
	}
	private void OnDisable()
	{
		Item<ItemDefinition>.OnCleanUpItem -= CleanUpItemObject;
	}

	#region create world item
	public static Item GetItem<T>(T definition, int stackCount, Transform parent, Vector3 position, Quaternion rotation)
	where T : ItemDefinition
	{
		Item item = Instance.TryGetItemFromObjectPooling(definition, position, rotation);

		item.gameObject.transform.SetParent(parent);
		item.gameObject.SetActive(true);

		if (item is Item<WeaponRangedDefinition> weaponRanged)
			weaponRanged.InitializeItem(definition as WeaponRangedDefinition, stackCount);

		else if (item is Item<WeaponMeleeDefinition> weaponMelee)
			weaponMelee.InitializeItem(definition as WeaponMeleeDefinition, stackCount);

		else if (item is Item<ArmourDefinition> armour)
			armour.InitializeItem(definition as ArmourDefinition, stackCount);

		else if (item is Item<ConsumableDefinition> consumable)
			consumable.InitializeItem(definition as ConsumableDefinition, stackCount);

		else
		{
			Debug.LogError($"Item prefab does not match definition type {typeof(T)}");
		}

		return item;
	}
	#endregion

	#region audio handler object pooling
	private Item TryGetItemFromObjectPooling(ItemDefinition itemDefinition, Vector3 position, Quaternion rotation)
	{
		if (!itemObjectPooling.TryGetValue(itemDefinition, out List<Item> itemList))
		{
			itemList = new List<Item>();
			itemObjectPooling[itemDefinition] = itemList;
		}

		if (itemList.Count > 0)
		{
			Item item = itemList[0];
			itemList.RemoveAt(0);
			return item;
		}
		else
		{
			if (itemDefinition is WeaponRangedDefinition)
				return Instantiate(rangedWeaponPrefab, position, rotation).GetComponent<Item>();

			else if (itemDefinition is WeaponMeleeDefinition)
				return Instantiate(meleeWeaponPrefab, position, rotation).GetComponent<Item>();

			else if (itemDefinition is ArmourDefinition)
				return Instantiate(armourPrefab, position, rotation).GetComponent<Item>();

			else if (itemDefinition is ConsumableDefinition)
				return Instantiate(consumablePrefab, position, rotation).GetComponent<Item>();

			else
			{
				Debug.LogError("ItemDefinition not supported, add logic for it");
				return null;
			}
		}
	}

	public void CleanUpItemObject(Item item)
	{
		if (!itemObjectPooling.TryGetValue(item.ItemDefinition, out List<Item> itemList))
		{
			itemList = new List<Item>();
			itemObjectPooling[item.ItemDefinition] = itemList;
		}

		itemList.Add(item);
		item.gameObject.transform.SetParent(gameObject.transform);
		item.gameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		item.gameObject.SetActive(false);
	}
	#endregion
}
