using Mono.Cecil;
using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class ItemSpawner : MonoBehaviour
{
	public static ItemSpawner Instance { get; private set; }

	public Dictionary<ItemDefinition, List<Item>> itemObjectPooling = new();
	public Dictionary<ItemDefinition, List<GameObject>> itemModelPooling = new();

	[Header("Item Prefab Objects")]
	public GameObject rangedWeaponPrefab;
	public GameObject meleeWeaponPrefab;
	public GameObject armourPrefab;
	public GameObject consumablePrefab;

	//Debug editor controls
	[HideInInspector] public ItemDefinition itemToSpawn;
	[HideInInspector] public int itemCountToSpawn;
	[HideInInspector] public Vector3 locationToSpawnItem;

	[HideInInspector] public Item worldItemToCleanUp;

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
		Item.OnCleanUpItem += CleanUpItemObjectAndPool;
		//Item.OnCleanUpItemModel += DetachModelAndPool;
	}
	private void OnDisable()
	{
		Item.OnCleanUpItem -= CleanUpItemObjectAndPool;
		//Item.OnCleanUpItemModel -= DetachModelAndPool;
	}

	#region create world item
	public static Item GetItem<T>(T definition, int stackCount, Transform parent, Vector3 position, Quaternion rotation)
	where T : ItemDefinition
	{
		Item item = Instance.TryGetItemFromObjectPooling(definition, position, rotation);

		item.gameObject.transform.SetParent(parent);
		item.gameObject.SetActive(true);

		GameObject modelReference = item.ModelReference;

		if (modelReference == null && definition.ModelPrefab != null) //logs handled in items
		{
			if (definition.ModelPrefab == null)
				Debug.LogWarning($"{definition.ItemName} has no model assigned in definition (maybe intentional or not yet created).");
			else
				modelReference = Instantiate(definition.ModelPrefab);
		}

		item.InitializeItem(definition, modelReference, stackCount);

		if (item is Item<WeaponRangedDefinition> weaponRanged)
			weaponRanged.InitializeItem(definition, modelReference,  stackCount);

		else if (item is Item<WeaponMeleeDefinition> weaponMelee)
			weaponMelee.InitializeItem(definition, modelReference, stackCount);

		else if (item is Item<ArmourDefinition> armour)
			armour.InitializeItem(definition, modelReference, stackCount);

		else if (item is Item<ConsumableDefinition> consumable)
			consumable.InitializeItem(definition, modelReference, stackCount);

		else
		{
			Debug.LogError($"Item prefab does not match definition type {typeof(T)}");
		}

		return item;
	}
	#endregion

	#region item object pooling
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
	private void CleanUpItemObjectAndPool(Item item)
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

	/// <summary>
	/// Separate model pooling kept for potential future use but not required with current 1:1 items planned.
	/// </summary>

	#region model object pooling
	private GameObject TryGetItemModelFromObjectPooling(ItemDefinition itemDefinition)
	{
		if (!itemModelPooling.TryGetValue(itemDefinition, out List<GameObject> modelList))
		{
			modelList = new List<GameObject>();
			itemModelPooling[itemDefinition] = modelList;
		}

		if (modelList.Count > 0)
		{
			GameObject itemModel = modelList[0];
			modelList.RemoveAt(0);
			return itemModel;
		}
		else
		{
			if (itemDefinition.ModelPrefab == null)
				return null;
			else 
				return Instantiate(itemDefinition.ModelPrefab, transform);
		}
	}
	private void DetachModelAndPool(ItemDefinition itemDefinition, GameObject model)
	{
		if (!itemModelPooling.TryGetValue(itemDefinition, out List<GameObject> modelList))
		{
			modelList = new List<GameObject>();
			itemModelPooling[itemDefinition] = modelList;
		}

		modelList.Add(model);
		model.transform.SetParent(gameObject.transform);
		model.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		model.SetActive(false);
	}
	#endregion
}
