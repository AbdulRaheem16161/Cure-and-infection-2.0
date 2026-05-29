using JetBrains.Annotations;
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
	public void InitializeEquipmentHandler(EntityDefinition definition)
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

		if (definition == null) return; //allows partial component testing

		EquipNpcEquipment(definition);
	}
	#endregion

	#region auto equip npc starting equipment
	private void EquipNpcEquipment(EntityDefinition definition)
	{
		if (definition == null) return;
		if (definition is not HumanoidDefinition humanoid) return;

		if (humanoid.MeleeWeapon != null) //auto equip melee to hands
		{
			HandleItemEquipping(GetEquipmentSlot(EquipmentType.weaponMelee), new(humanoid.MeleeWeapon, humanoid.MeleeWeapon.StackLimit));
			UnholsterWeapon(EquipmentType.weaponMelee);
		}

		if (humanoid.WeaponOne != null) //overwrite melee weapon (allows for melee only npcs)
		{
            HandleItemEquipping(GetEquipmentSlot(EquipmentType.weaponOne), new(humanoid.WeaponOne, humanoid.WeaponOne.StackLimit));
            UnholsterWeapon(EquipmentType.weaponOne);
		}
		if (humanoid.WeaponTwo != null)
            HandleItemEquipping(GetEquipmentSlot(EquipmentType.weaponTwo), new(humanoid.WeaponTwo, humanoid.WeaponTwo.StackLimit));

        if (humanoid.Helmet != null)
            HandleItemEquipping(GetEquipmentSlot(EquipmentType.helmet), new(humanoid.Helmet, humanoid.Helmet.StackLimit));
        if (humanoid.Chest != null)
            HandleItemEquipping(GetEquipmentSlot(EquipmentType.chest), new(humanoid.Chest, humanoid.Chest.StackLimit));
        if (humanoid.Backpack != null)
            HandleItemEquipping(GetEquipmentSlot(EquipmentType.backpack), new(humanoid.Backpack, humanoid.Backpack.StackLimit));

        if (humanoid.ConsumableOne != null)
            HandleItemEquipping(GetEquipmentSlot(EquipmentType.consumableOne), new(humanoid.ConsumableOne, humanoid.ConsumableOne.StackLimit));
        if (humanoid.ConsumableTwo != null)
            HandleItemEquipping(GetEquipmentSlot(EquipmentType.consumableTwo), new(humanoid.ConsumableTwo, humanoid.ConsumableTwo.StackLimit));
        if (humanoid.ConsumableThree != null)
            HandleItemEquipping(GetEquipmentSlot(EquipmentType.consumableThree), new(humanoid.ConsumableThree, humanoid.ConsumableThree.StackLimit));
    }
	#endregion

	/// <summary>
	/// will need updating to play any equip/unequip sfxs, linking with any animations and vfxs when equipping weapons, armour and using consumables
	/// + proper pos/rot setting to visually be on characters back etc..
	/// </summary>

	#region handle item equipping/unequipping
	public void HandleItemEquipping(EquipmentSlot slot, InventoryItem item)
	{
		if (item.ItemDefinitionNull) return;

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
	public void HandleItemUnequipping(EquipmentSlot slot)
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
		if (item is RangedWeaponItem || item is MeleeWeaponItem)
			return equippedWeaponsParent.transform;

		else if (item is ArmourItem armour)
		{
			return armour.TypedDefinition.ArmourSlot switch
			{
				ArmourSlotType.helmet => equippedHelmetParent.transform,
				ArmourSlotType.chest => equippedChestpieceParent.transform,
				ArmourSlotType.backpack => equippedBackpackParent.transform,
				_ => transform
			};
		}

		else if (item is ConsumableItem)
			return null;

		else
		{
			Debug.LogError("no equip slot found, returning Transfrom of EquipmentHandler");
			return transform;
		}
	}
    #endregion

    #region Invoke equip/unequip item changes event
	public void InvokeEquippedItemChanges(EquipmentSlot slot, bool equipped)
	{
        OnEquippedItemChanges?.Invoke(slot, equipped);

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
	/// <summary>
	/// use consumable currently equipped to slot, with optional consumeItem bool (true by default)
	/// </summary>
	public void UseConsumable(EquipmentType equipmentType, bool consumeItem = true)
	{
		EquipmentSlot slot = GetEquipmentSlot(equipmentType);

		//TODO: could add checks like if full health dont consume etc...
		if (slot.Item.ItemDefinition is not ConsumableDefinition)
		{
			Debug.LogError($"{equipmentType} cannot consume this item.");
			return;
		}

		if (consumeItem)
			slot.Item.SetItemStack(slot.Item.CurrentStack - 1);

		OnConsumableUsed?.Invoke(slot);

		if (slot.Item.CurrentStack <= 0)
			HandleItemUnequipping(slot);
	}
	#endregion

	#region handle holstering/unholstering weapons
	public void HolsterWeapon()
	{
		if (itemInHands == null) return;
		itemInHands.HolsterItem(this);
		itemInHands.transform.SetParent(equippedWeaponsParent.transform);
		itemInHands.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		itemInHands = null;
	}
	public void UnholsterWeapon(EquipmentType equipmentType)
	{
		HolsterWeapon(); //holster current weapon if any
		itemInHands = GetEquipmentSlot(equipmentType).WorldItem;
		itemInHands.UnHolsterItem(this);
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
    public bool GetMatchingEquipmentSlot(EquipmentType equipmentType, out EquipmentSlot slot)
    {
        foreach (EquipmentSlot equipmentSlot in equippedItems)
        {
            if (equipmentSlot.EquipmentType == equipmentType)
			{
				slot = equipmentSlot;
                return true;
            }
        }
        Debug.LogError($"Failed to find {typeof(EquipmentSlot)} that matched {equipmentType}, returning first as fallback");

		slot = null;
        return false;
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
	public bool EquipmentSlotMatchesAllowed(EquipmentSlot slot, InventoryItem item)
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