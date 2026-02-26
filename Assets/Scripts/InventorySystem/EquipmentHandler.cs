using System;
using System.Collections.Generic;
using UnityEngine;
using static EquipmentHandler;
using static ItemDefinition;

public class EquipmentHandler : MonoBehaviour
{
	public CharacterStatsTest CharacterStats { get; private set; }
	public InventoryHandler InventoryHandler { get; private set; }

	#region equipment slots + dictionary lookup
	[Header("Equipment Settings")]
	public Dictionary<EquipmentType, EquipmentSlot> slotLookup = new();
	public List<EquipmentSlot> equipmentSlots = new();
	#endregion

	#region equipped world items
	[Header("Equipped World Items")]
	public GameObject equippedItemsParent;
	public Dictionary<EquipmentType, WeaponRanged> equippedRangedWeapons = new();
	public Dictionary<EquipmentType, WeaponMelee> equippedMeleeWeapon = new();
	public Dictionary<EquipmentType, Armour> equippedArmour = new();
	#endregion

	#region item prefabs
	[Header("Equipment Prefabs")]
	public GameObject WeaponRangedPrefab;
	public GameObject WeaponMeleePrefab;
	public GameObject ArmourPrefab;
	#endregion

	#region debug settings
	[Header("Debug Settings")]
	[HideInInspector] public ItemDefinition itemToEquip;
	[HideInInspector] public int itemToEquipCount;
	[HideInInspector] public EquipmentType slotToEquipItemTo;
	[HideInInspector] public EquipmentType equipmentSlotToUnequip;
	[HideInInspector] public EquipmentType consumableSlotToUse;
	[HideInInspector] public int equipItemFromSlot;
	#endregion

	#region events
	public event Action<EquipmentSlot> OnItemEquip;
	public event Action<EquipmentSlot> OnItemUnEquip;
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

	#region initilize slots + grab script refs on awake
	private void Awake()
	{
		CharacterStats = GetComponent<CharacterStatsTest>();

		if (CharacterStats == null)
		{
			Debug.LogError($"CharacterStatsTest script not found on this gameobject: {gameObject.name}");
			return;
		}

		InventoryHandler = GetComponent<InventoryHandler>();

		if (InventoryHandler == null)
		{
			Debug.LogError($"InventoryHandler script not found on this gameobject: {gameObject.name}");
			return;
		}

		InitilizeEquipmentSlots();
	}
	private void InitilizeEquipmentSlots()
	{
		equipmentSlots = new();
		slotLookup = new();

		foreach (EquipmentType type in Enum.GetValues(typeof(EquipmentType)))
		{
			if (!slotToInventoryType.TryGetValue(type, out var slotType))
			{
				Debug.LogWarning($"No InventorySlotType mapped for {type}, skipping.");
				continue;
			}

			EquipmentSlot newSlot = new(slotType, type, new(null, 0));
			equipmentSlots.Add(newSlot);
			slotLookup.Add(type, newSlot);
		}
	}
	#endregion

	#region equipping item
	/// <summary>
	/// equip item, replacing any existing item, safe to use for npcs
	/// </summary>
	public void EquipItem(ItemDefinition item, int stackCount, EquipmentType equipmentType)
	{
		EquipmentSlot equipmentSlot = GetEquipmentSlot(equipmentType);
		InventoryItem itemToEquip = new(item, stackCount);

		if (!SlotAndItemTypeMatch(equipmentSlot, itemToEquip)) return;

		HandleItemEquipping(itemToEquip, equipmentSlot);
	}

	/// <summary>
	/// equip item from inventory, returning existing item to inventory by default, always use for player 
	/// </summary>
	public void EquipItemFromInventory(int itemSlot, EquipmentType equipmentType, bool returnItem = true)
	{
		EquipmentSlot equipmentSlot = GetEquipmentSlot(equipmentType);
		InventoryItem equippedItem = CheckForEquippedItem(equipmentType);
		InventoryItem itemToEquip = InventoryHandler.InventoryItems[itemSlot];

		if (itemToEquip.ItemDefinition == null && equippedItem != null && returnItem) //return early if no weapon to equip
		{
			HandleItemUnequipping(equipmentSlot);
			InventoryHandler.AddNewItemPickUp(equippedItem);
			return;
		}

		if (!SlotAndItemTypeMatch(equipmentSlot, itemToEquip)) return;

		if (equippedItem != null && returnItem) //return item
		{
			if (InventoryHandler.InventoryFull())
			{
				Debug.LogWarning("inventory full, cannot equip new item and return old one");
				return;
			}

			HandleItemUnequipping(equipmentSlot);
			InventoryHandler.AddNewItemPickUp(equippedItem);
		}

		HandleItemEquipping(itemToEquip, equipmentSlot);
		InventoryHandler.RemoveItemsFromSlot(itemSlot, itemToEquip.CurrentStack);
	}
	/// <summary>
	/// equip item from equipment, swapping item places if slot matches
	/// </summary>
	public void EquipItemFromEquipment(EquipmentType currentSlotType, EquipmentType newSlotType)
	{
		EquipmentSlot currentEquipmentSlot = GetEquipmentSlot(currentSlotType);
		EquipmentSlot newEquipmentSlot = GetEquipmentSlot(newSlotType);

		InventoryItem currentEquippedItem = CheckForEquippedItem(currentSlotType);
		InventoryItem newEquippedItem = CheckForEquippedItem(newSlotType);

		if (!SlotTypesMatch(currentEquipmentSlot, newEquipmentSlot)) return; //cant swap equipped items

		HandleItemEquipping(currentEquippedItem, newEquipmentSlot);

		if (newEquippedItem == null)
		{
			HandleItemUnequipping(currentEquipmentSlot);
			Debug.LogWarning("inventory item null");
			return;
		}
		if (newEquippedItem.ItemDefinition == null) //item def null nothing to swap
		{
			Debug.LogWarning("inventory item definition null");
			return;
		}

		HandleItemEquipping(newEquippedItem, currentEquipmentSlot);
	}
	#endregion

	#region unequipping item
	/// <summary>
	/// unequip item, returning existing item to inventory by default
	/// </summary>
	public void UnequipItem(EquipmentType equipmentType, bool returnItem = true)
	{
		EquipmentSlot equipmentSlot = GetEquipmentSlot(equipmentType);
		InventoryItem equippedItem = CheckForEquippedItem(equipmentType);

		if (equippedItem == null) return; //no equipped item to unequip
		HandleItemUnequipping(equipmentSlot);

		if (returnItem)
		{
			if (!InventoryHandler.InventoryFull()) //return equipped item
				InventoryHandler.AddNewItemPickUp(equippedItem);
			else
				Debug.LogWarning("inventory full, cannot unequip item");
		}
	}
	#endregion

	#region dropping item in equipment
	public void DropItem(EquipmentType equipmentType, bool dropStack)
	{
		EquipmentSlot equipmentSlot = GetEquipmentSlot(equipmentType);
		InventoryItem equippedItem = CheckForEquippedItem(equipmentType);

		HandleItemUnequipping(equipmentSlot);

		//spawn world item
	}
	#endregion

	/// <summary>
	/// will need updating to play any equip/unequip sfxs, linking with any animations and vfxs when equipping weapons, armour and using consumables
	/// </summary>

	#region equipping world items and equipment slots
	private void HandleItemEquipping(InventoryItem item, EquipmentSlot slot)
	{
		Debug.Log($"equipped {item.ItemDefinition.ItemName} to {slot.equipmentType} slot");
		slot.item = new InventoryItem(item.ItemDefinition, item.CurrentStack);
		item.ItemDefinition.OnEquip(this, slot);
		OnItemEquip?.Invoke(slot);
	}
	#endregion

	#region unequipping world items and equipment slots
	private void HandleItemUnequipping(EquipmentSlot slot)
	{
		Debug.Log($"unequipped {slot.item.ItemDefinition.ItemName} from {slot.equipmentType} slot");
		slot.item.ItemDefinition.OnUnequip(this, slot);
		OnItemUnEquip?.Invoke(slot);
		slot.item = null;
	}
	#endregion

	#region equipping/unequipping ranged weapon objects
	public void EquipRangedWeapon(EquipmentSlot slot)
	{
		//get or create the world weapon instance
		if (!equippedRangedWeapons.TryGetValue(slot.equipmentType, out var weaponInstance) || weaponInstance == null)
		{
			weaponInstance = InstantiateRangedWeapon();
			equippedRangedWeapons[slot.equipmentType] = weaponInstance;
		}

		weaponInstance.gameObject.SetActive(true);
		weaponInstance.InitializeItem((WeaponRangedDefinition)slot.item.ItemDefinition, slot.item.CurrentStack);
	}

	public void UnEquipRangedWeapon(EquipmentSlot slot)
	{
		//get weapon instance
		if (!equippedRangedWeapons.TryGetValue(slot.equipmentType, out var weaponInstance) || weaponInstance == null)
			return;

		//destroy or disable game object
		weaponInstance.gameObject.SetActive(false);
	}
	#endregion

	#region equipping/unequipping melee weapon objects
	public void EquipMeleeWeapon(EquipmentSlot slot)
	{
		//get or create the world weapon instance
		if (!equippedMeleeWeapon.TryGetValue(slot.equipmentType, out var weaponInstance) || weaponInstance == null)
		{
			weaponInstance = InstantiateMeleeWeapon();
			equippedMeleeWeapon[slot.equipmentType] = weaponInstance;
		}

		weaponInstance.gameObject.SetActive(true);
		weaponInstance.InitializeItem((WeaponMeleeDefinition)slot.item.ItemDefinition, slot.item.CurrentStack);
	}

	public void UnEquipMeleeWeapon(EquipmentSlot slot)
	{
		//get weapon instance
		if (!equippedMeleeWeapon.TryGetValue(slot.equipmentType, out var weaponInstance) || weaponInstance == null)
			return;

		//destroy or disable game object
		weaponInstance.gameObject.SetActive(false);
	}
	#endregion

	#region equipping/unequipping armour objects
	public void EquipArmour(EquipmentSlot slot)
	{
		//get or create the world armour instance
		if (!equippedArmour.TryGetValue(slot.equipmentType, out var armourInstance) || armourInstance == null)
		{
			armourInstance = InstantiateArmour();
			equippedArmour[slot.equipmentType] = armourInstance;
		}

		armourInstance.gameObject.SetActive(true);
		armourInstance.InitializeItem((ArmourDefinition)slot.item.ItemDefinition, slot.item.CurrentStack);
	}

	public void UnEquipArmour(EquipmentSlot slot)
	{
		//get world armour instance
		if (!equippedArmour.TryGetValue(slot.equipmentType, out var armourInstance) || armourInstance == null)
			return;

		//destroy or disable game object
		armourInstance.gameObject.SetActive(false);
	}
	#endregion

	#region instantiate world objects (weapons/armours)
	private WeaponRanged InstantiateRangedWeapon()
	{
		GameObject weaponGO = Instantiate(WeaponRangedPrefab);
		weaponGO.transform.parent = equippedItemsParent.transform;
		WeaponRanged weapon = weaponGO.GetComponent<WeaponRanged>();
		return weapon;
	}
	private WeaponMelee InstantiateMeleeWeapon()
	{
		GameObject weaponGO = Instantiate(WeaponMeleePrefab);
		weaponGO.transform.parent = equippedItemsParent.transform;
		WeaponMelee weapon = weaponGO.GetComponent<WeaponMelee>();
		return weapon;
	}
	private Armour InstantiateArmour()
	{
		GameObject armourGO = Instantiate(ArmourPrefab);
		armourGO.transform.parent = equippedItemsParent.transform;
		Armour armour = armourGO.GetComponent<Armour>();
		return armour;
	}
	#endregion

	#region using consumables in quick slots
	public void UseConsumable(EquipmentType equipmentType)
	{
		EquipmentSlot slot = GetEquipmentSlot(equipmentType);
		InventoryItem equippedItem = CheckForEquippedItem(equipmentType);

		if (!equippedItem.ItemDefinition.CanEquipTo(equipmentType))
		{
			Debug.LogError($"{equipmentType} cannot use this item.");
			return;
		}

		bool shouldConsume = equippedItem.ItemDefinition.OnUsed(this, slot);
		if (!shouldConsume)
			return;

		equippedItem.RemoveItemStack(1);
		OnConsumableUsed?.Invoke(slot);

		if (equippedItem.CurrentStack <= 0)
			HandleItemUnequipping(slot);
	}
	#endregion

	/// <summary>
	/// need to add logic framework for holstering/unholstering weapons, play sfx/animation
	/// </summary>
	/// 
	#region weapon holstering
	public void HolsterWeapon(GameObject weaponObj)
	{

	}
	public void UnholsterWeapon(GameObject weaponObj)
	{

	}
	#endregion

	#region equipment slot and inventory item checks
	private InventoryItem CheckForEquippedItem(EquipmentType equipmentType)
	{
		EquipmentSlot slot = GetEquipmentSlot(equipmentType);

		if (slot == null)
		{
			Debug.LogError($"no slot found for {equipmentType}");
			return null;
		}

		if (slot.item == null)
		{
			Debug.LogWarning($"slot {equipmentType} item is null");
			return null;
		}

		if (slot.item.ItemDefinition == null)
		{
			Debug.LogWarning($"slot {equipmentType} item definition is null");
			return null;
		}

		return slot.item;
	}
	private EquipmentSlot GetEquipmentSlot(EquipmentType equipmentType)
	{
		if (!slotLookup.TryGetValue(equipmentType, out var slot))
			return null;
		return slot;
	}
	#endregion

	#region slot and item type checks
	private bool SlotTypesMatch(EquipmentSlot slotOne, EquipmentSlot slotTwo)
	{
		InventorySlotType slotTypeOne = slotToInventoryType[slotOne.equipmentType];
		InventorySlotType slotTypeTwo = slotToInventoryType[slotTwo.equipmentType];

		if (slotTypeOne == slotTypeTwo)
			return true;

		Debug.LogWarning($"Slot One ({slotTypeOne}) and Slot Two ({slotTypeTwo}) types do not match");
		return false;
	}
	private bool SlotAndItemTypeMatch(EquipmentSlot slot, InventoryItem item)
	{
		if (!slotToInventoryType.TryGetValue(slot.equipmentType, out var slotCategory))
		{
			Debug.LogWarning($"EquipmentType {slot.equipmentType} has no mapped InventorySlotType");
			return false;
		}

		if (item.ItemDefinition.AllowedSlots.HasFlag(slotCategory))
			return true;

		Debug.LogWarning($"Slot {slot.equipmentType} and item {item.ItemDefinition.ItemName} type do not match");
		return false;
	}
	#endregion
}

[System.Serializable]
public class EquipmentSlot
{
	public InventorySlotType slotType; // stored once
	public EquipmentType equipmentType;
	public InventoryItem item;

	public EquipmentSlot(InventorySlotType slotType, EquipmentType equipmentType, InventoryItem item)
	{
		this.slotType = slotType;
		this.equipmentType = equipmentType;
		this.item = item;
	}
}
