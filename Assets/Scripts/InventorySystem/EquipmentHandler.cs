using System;
using System.Collections.Generic;
using UnityEngine;
using static ArmourDefinition;
using static EquipmentHandler;
using static ItemDefinition;

[RequireComponent(typeof(StatsHandler))]
[RequireComponent(typeof(InventoryHandler))]
public class EquipmentHandler : MonoBehaviour
{
	public StatsHandler StatsHandler { get; private set; }
	public InventoryHandler InventoryHandler { get; private set; }
	private bool _Initialized = false;

	#region Equipment Item Prefabs
	[Header("Equipment Item Prefabs")]
	public GameObject WeaponRangedPrefab;
	public GameObject WeaponMeleePrefab;
	public GameObject ArmourPrefab;
	#endregion

	#region Equipped Items Parent Slots
	[Header("Equipped Items Parent Slots")]
	public GameObject equippedHelmetParent;
	public GameObject equippedChestpieceParent;
	public GameObject equippedBackpackParent;
	public GameObject equippedWeaponsParent;
	public GameObject ItemsInHandsParent;
	#endregion

	#region Equipped Items Data Settings
	[Header("Equipped Items Data Settings")]
	public List<EquipmentSlot> equippedItems = new();
	#endregion

	#region item in hands
	[Header("Weapon Item In Hands")]
	public Item itemInHands;
	public bool HasItemInHands => itemInHands != null;
	#endregion

	#region debug settings
	[Header("Debug Settings")]
	[HideInInspector] public ItemDefinition itemToEquip;
	[HideInInspector] public int itemToEquipCount;
	[HideInInspector] public EquipmentType slotToEquipItemTo;
	[HideInInspector] public EquipmentType equipmentSlotToUnequip;
	[HideInInspector] public EquipmentType consumableSlotToUse;
	[HideInInspector] public int equipItemFromInventorySlot;
	[HideInInspector] public EquipmentType unHolsterItem;
	#endregion

	#region events
	public event Action<EquipmentSlot, bool> OnEquippedItemChanges;
	public event Action<EquipmentSlot> OnConsumableUsed;
	#endregion

	#region equipment types
	[Flags]
	public enum EquipmentType
	{
		none = 0,
		weaponOne = 1 << 0,
		weaponTwo = 1 << 1,
		weaponMelee = 1 << 2, 
		helmet = 1 << 3, 
		chest = 1 << 4, 
		backpack = 1 << 5, 
		consumableOne = 1 << 6, 
		consumableTwo = 1 << 7, 
		consumableThree = 1 << 8
	}
	#endregion

	#region equipment type to inventory type mapping
	public static readonly Dictionary<EquipmentType, InventorySlotType> slotToInventoryType = new()
	{
		{ EquipmentType.weaponOne, InventorySlotType.weaponRanged },
		{ EquipmentType.weaponTwo, InventorySlotType.weaponRanged },
		{ EquipmentType.weaponMelee, InventorySlotType.weaponMelee },
		{ EquipmentType.helmet, InventorySlotType.armour },
		{ EquipmentType.chest, InventorySlotType.armour },
		{ EquipmentType.backpack,InventorySlotType.armour },
		{ EquipmentType.consumableOne, InventorySlotType.consumable },
		{ EquipmentType.consumableTwo, InventorySlotType.consumable },
		{ EquipmentType.consumableThree, InventorySlotType.consumable }
	};
	#endregion

	#region awake + initialize equipment handler method
	private void Awake()
	{
		StatsHandler = GetComponent<StatsHandler>();
		InventoryHandler = GetComponent<InventoryHandler>();

		if (!_Initialized)
			InitializeEquipmentHandler(null);
	}
	public void InitializeEquipmentHandler(NpcDefinition npcDefinition)
	{
		_Initialized = true;

		equippedItems.Clear();

		foreach (EquipmentType type in Enum.GetValues(typeof(EquipmentType)))
		{
			if (!slotToInventoryType.TryGetValue(type, out var slotType))
			{
				if (slotType == InventorySlotType.none) continue;
				Debug.LogWarning($"No InventorySlotType mapped for {type}, skipping.");
				continue;
			}

			EquipmentSlot equipmentSlot = new(slotType, type, new(null, 0), null);
			equippedItems.Add(equipmentSlot);
		}

		if (npcDefinition == null) return; //allows partial component testing

		EquipNpcEquipment(npcDefinition);
	}
	#endregion

	#region auto equip npc starting equipment
	private void EquipNpcEquipment(NpcDefinition npcDefinition)
	{
		if (npcDefinition == null) return;

		if (npcDefinition.MeleeWeapon != null) //auto equip melee to hands
		{
			EquipItem(npcDefinition.MeleeWeapon, npcDefinition.MeleeWeapon.StackLimit, EquipmentType.weaponMelee);
			UnholsterWeapon(EquipmentType.weaponMelee);
		}

		if (npcDefinition.WeaponOne != null) //overwrite melee weapon (allows for melee only npcs)
		{
			EquipItem(npcDefinition.WeaponOne, npcDefinition.WeaponOne.StackLimit, EquipmentType.weaponOne);
			UnholsterWeapon(EquipmentType.weaponOne);
		}
		if (npcDefinition.WeaponTwo != null)
			EquipItem(npcDefinition.WeaponTwo, npcDefinition.WeaponTwo.StackLimit, EquipmentType.weaponTwo);

		if (npcDefinition.Helmet != null)
			EquipItem(npcDefinition.Helmet, npcDefinition.Helmet.StackLimit, EquipmentType.helmet);
		if (npcDefinition.Chest != null)
			EquipItem(npcDefinition.Chest, npcDefinition.Chest.StackLimit, EquipmentType.chest);
		if (npcDefinition.Backpack != null)
			EquipItem(npcDefinition.Backpack, npcDefinition.Backpack.StackLimit, EquipmentType.backpack);

		if (npcDefinition.ConsumableOne != null)
			EquipItem(npcDefinition.ConsumableOne, npcDefinition.ConsumableOne.StackLimit, EquipmentType.consumableOne);
		if (npcDefinition.ConsumableTwo != null)
			EquipItem(npcDefinition.ConsumableTwo, npcDefinition.ConsumableTwo.StackLimit, EquipmentType.consumableTwo);
		if (npcDefinition.ConsumableThree != null)
			EquipItem(npcDefinition.ConsumableThree, npcDefinition.ConsumableThree.StackLimit, EquipmentType.consumableThree);
	}
	#endregion

	#region equipping item methods
	/// <summary>
	/// equip item, replacing any existing item, safe to use for npcs
	/// </summary>
	public void EquipItem(ItemDefinition item, int stackCount, EquipmentType equipmentType)
	{
		EquipmentSlot equipmentSlot = GetEquipmentSlot(equipmentType);
		InventoryItem itemToEquip = new(item, stackCount);

		if (!EquipmentSlotsMatch(equipmentSlot, itemToEquip)) return;

		HandleItemEquipping(itemToEquip, equipmentSlot);
	}

	/// <summary>
	/// equip item from inventory, returning existing item to inventory by default, always use for player 
	/// </summary>
	public void EquipItemFromInventory(int itemSlot, EquipmentType equipmentType, bool returnItem = true)
	{
		EquipmentSlot slot = GetEquipmentSlot(equipmentType);
		InventoryItem itemToEquip = InventoryHandler.ItemContainer.Items[itemSlot];


		if (itemToEquip.ItemDefinitionNull && !slot.ItemDefinitionNull && returnItem) //return early if no item to equip
		{
			HandleItemUnequipping(slot);
			InventoryHandler.ItemContainer.AddNewItem(slot.Item);
			return;
		}

		if (!EquipmentSlotsMatch(slot, itemToEquip)) return;

		if (!slot.ItemDefinitionNull && returnItem) //return item
		{
			if (InventoryHandler.ItemContainer.ContainerFull())
			{
				Debug.LogWarning("inventory full, cannot equip new item and return old one");
				return;
			}

			HandleItemUnequipping(slot);
			InventoryHandler.ItemContainer.AddNewItem(slot.Item);
		}

		HandleItemEquipping(itemToEquip, slot);
		InventoryHandler.RemoveItemsFromSlot(itemSlot, itemToEquip.CurrentStack);
	}
	/// <summary>
	/// equip item from equipment, swapping item places if slot matches
	/// </summary>
	public void EquipItemFromEquipment(EquipmentType currentSlotType, EquipmentType newSlotType)
	{
		EquipmentSlot currentSlot = GetEquipmentSlot(currentSlotType);
		EquipmentSlot newSlot = GetEquipmentSlot(newSlotType);

		if (!SlotTypesMatch(currentSlot, newSlot)) return; //cant swap equipped items
		if (!EquipmentSlotsMatch(newSlot, currentSlot.Item)) return;

		HandleItemEquipping(currentSlot.Item, newSlot);

		if (currentSlot.Item == null)
		{
			HandleItemUnequipping(currentSlot);
			Debug.LogWarning("inventory item null");
			return;
		}
		if (newSlot.Item.ItemDefinition == null) //item def null nothing to swap
		{
			Debug.LogWarning("inventory item definition null");
			return;
		}

		HandleItemEquipping(newSlot.Item, currentSlot);
	}
	#endregion

	#region unequipping item method
	/// <summary>
	/// unequip item, returning existing item to inventory by default
	/// </summary>
	public void UnequipItem(EquipmentType equipmentType, bool returnItem = true)
	{
		EquipmentSlot slot = GetEquipmentSlot(equipmentType);

		if (slot.ItemNull) return; //no equipped item to unequip
		HandleItemUnequipping(slot);

		if (returnItem)
		{
			if (!InventoryHandler.ItemContainer.ContainerFull()) //return equipped item
				InventoryHandler.ItemContainer.AddNewItem(slot.Item);
			else
				Debug.LogWarning("inventory full, cannot unequip item");
		}
	}
	#endregion

	#region drop item method on ground (TODO add spawning of item on ground logic)
	public void DropItem(EquipmentType equipmentType, bool dropStack)
	{
		//TODO: instantiate item in world at characters feet
		EquipmentSlot slot = GetEquipmentSlot(equipmentType);

		if (slot.ItemNull) return;

		HandleItemUnequipping(slot);

		//spawn world item
	}
	#endregion

	/// <summary>
	/// will need updating to play any equip/unequip sfxs, linking with any animations and vfxs when equipping weapons, armour and using consumables
	/// + proper pos/rot setting to visually be on characters back etc..
	/// </summary>

	#region handle item equipping/unequipping
	private void HandleItemEquipping(InventoryItem item, EquipmentSlot slot)
	{
		if (item.ItemDefinitionNull)
		{
			Debug.LogError("items, itemDefinition is null when reference is expected");
			return;
		}

		Debug.Log($"equipped {item.ItemDefinition.ItemName} to {slot.EquipmentType} slot");
		slot.SetInventoryItem(new(item.ItemDefinition, item.CurrentStack));

		if (item.ItemDefinition is not ConsumableDefinition) //doesnt need world item
		{
			Item spawnedItem = GetOrCreateItemInstance(slot);
			spawnedItem.EquipItem(this, GetParentForSlot(spawnedItem));
			slot.SetWorldItem(spawnedItem);
		}
		OnEquippedItemChanges?.Invoke(slot, true);
	}
	private void HandleItemUnequipping(EquipmentSlot slot)
	{
		if (slot.ItemDefinitionNull) return; //no item to unequip can be expected
		Debug.Log($"unequipped {slot.Item.ItemDefinition.ItemName} from {slot.EquipmentType} slot");

		if (slot.Item.ItemDefinition is not ConsumableDefinition) //has no world item
		{
			Item spawnedItem = GetOrCreateItemInstance(slot);
			spawnedItem.UnEquipItem(this);
			slot.SetWorldItem(spawnedItem);
		}

		OnEquippedItemChanges?.Invoke(slot, false);
		slot.SetInventoryItem(null);
	}
	private Transform GetParentForSlot(Item item)
	{
		if (item is WeaponRanged || item is WeaponMelee)
			return equippedWeaponsParent.transform;

		else if (item is Armour armour)
		{
			return armour.TypedDefinition.ArmourSlot switch
			{
				ArmourSlotType.helmet => equippedHelmetParent.transform,
				ArmourSlotType.chest => equippedChestpieceParent.transform,
				ArmourSlotType.backpack => equippedBackpackParent.transform,
				_ => transform
			};
		}

		else if (item is Consumable)
			return null;

		else
		{
			Debug.LogError("no equip slot found, returning Transfrom of EquipmentHandler");
			return transform;
		}
	}
	#endregion

	#region get equipped item instance
	public Item GetOrCreateItemInstance(EquipmentSlot slot)
	{
		Item itemInstance;

		if (slot.WorldItemNull)
			itemInstance = ItemSpawner.GetItem(slot.Item.ItemDefinition, slot.Item.CurrentStack, null, Vector3.zero, Quaternion.identity);
		else
			itemInstance = slot.WorldItem;

		return itemInstance;
	}
	#endregion

	#region handle using consumables
	public void UseConsumable(EquipmentType equipmentType)
	{
		EquipmentSlot slot = GetEquipmentSlot(equipmentType);

		//TODO: could add checks like if full health dont consume etc...
		if (slot.Item.ItemDefinition is not ConsumableDefinition)
		{
			Debug.LogError($"{equipmentType} cannot consume this item.");
			return;
		}

		slot.Item.RemoveItemStack(1);
		OnConsumableUsed?.Invoke(slot);

		if (slot.Item.CurrentStack <= 0)
			HandleItemUnequipping(slot);
	}
	#endregion

	#region handle holstering/unholstering weapons
	public void HolsterWeapon()
	{
		if (itemInHands == null) return;
		itemInHands.HolsterItem();
		itemInHands.transform.SetParent(equippedWeaponsParent.transform);
		itemInHands.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		itemInHands = null;
	}
	public void UnholsterWeapon(EquipmentType equipmentType)
	{
		HolsterWeapon(); //holster current weapon if any
		itemInHands = GetEquipmentSlot(equipmentType).WorldItem;
		itemInHands.UnHolsterItem();
		itemInHands.transform.SetParent(ItemsInHandsParent.transform);
		itemInHands.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
	}
	#endregion

	#region equipment slot and inventory item checks
	public EquipmentSlot GetEquipmentSlot(EquipmentType equipmentType)
	{
		foreach (EquipmentSlot equipmentSlot in equippedItems)
		{
			if (equipmentSlot.EquipmentType == equipmentType)
				return equipmentSlot;
		}
		Debug.LogError($"Failed to find {typeof(EquipmentSlot)} that matched {equipmentType}, returning first as fallback");

		return equippedItems[0];
	}
	#endregion

	#region slot and item type checks
	private bool SlotTypesMatch(EquipmentSlot slotOne, EquipmentSlot slotTwo)
	{
		InventorySlotType slotTypeOne = slotToInventoryType[slotOne.EquipmentType];
		InventorySlotType slotTypeTwo = slotToInventoryType[slotTwo.EquipmentType];

		if (slotTypeOne == slotTypeTwo)
			return true;

		Debug.LogWarning($"Slot One ({slotTypeOne}) and Slot Two ({slotTypeTwo}) types do not match");
		return false;
	}
	private bool EquipmentSlotsMatch(EquipmentSlot slot, InventoryItem item)
	{
		return (slot.EquipmentType & item.ItemDefinition.AllowedEquipmentSlots) != 0;
	}
	#endregion
}

[System.Serializable]
public class EquipmentSlot
{
	[SerializeField] private InventorySlotType slotType;
	[SerializeField] private EquipmentType equipmentType;
	[SerializeField] private InventoryItem item;
	[SerializeField] private Item worldItem;

	public InventorySlotType SlotType => slotType;
	public EquipmentType EquipmentType => equipmentType;
	public InventoryItem Item => item;
	public Item WorldItem => worldItem;

	public bool ItemNull => item == null;
	public bool ItemDefinitionNull => item == null || item.ItemDefinition == null;
	public bool WorldItemNull => worldItem == null;

	public EquipmentSlot(InventorySlotType slotType, EquipmentType equipmentType, InventoryItem item, Item worldItem)
	{
		this.slotType = slotType;
		this.equipmentType = equipmentType;
		this.item = item;
		this.worldItem = worldItem;
	}

	public void SetInventoryItem(InventoryItem item)
	{
		this.item = item;
	}
	public void SetWorldItem(Item worldItem)
	{
		this.worldItem = worldItem;
	}
}
