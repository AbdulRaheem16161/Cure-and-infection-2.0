using System;
using System.Collections.Generic;
using System.Linq;
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
	public static event Action<EquipmentSlot> OnItemEquip;
	public static event Action<EquipmentSlot> OnItemUnEquip;
	public static event Action<EquipmentSlot> OnConsumableUsed;
	#endregion

	public enum EquipmentType
	{
		weaponOne, weaponTwo, weaponMelee, helmet, chest, backpack, consumableOne, consumableTwo, consumableThree
	}

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

		foreach (EquipmentType type in System.Enum.GetValues(typeof(EquipmentType)))
		{
			EquipmentSlot newSlot = new(type, null);

			equipmentSlots.Add(newSlot);
			slotLookup.Add(type, newSlot);
		}
	}
	#endregion

	#region equipping/unequipping item method calls
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
	/// unequip item, returning existing item to inventory by default
	/// </summary>
	public void UnequipItem(EquipmentType equipmentType, bool returnItem = true)
	{
		EquipmentSlot equipmentSlot = GetEquipmentSlot(equipmentType);
		InventoryItem equippedItem = CheckForEquippedItem(equipmentType);

		if (equippedItem == null) return; //no equipped item to unequip
		HandleItemUnequipping(equipmentSlot);

		if (returnItem && !InventoryHandler.InventoryFull()) //return equipped item
			InventoryHandler.AddNewItemPickUp(equippedItem);
		else
			Debug.LogWarning("inventory full, cannot unequip item");
	}
	#endregion

	/// <summary>
	/// will need updating to play any equip/unequip sfxs, linking with any animations and vfxs when equipping weapons, armour and using consumables
	/// </summary>

	#region equipping world items and equipment slots
	private void HandleItemEquipping(InventoryItem item, EquipmentSlot slot)
	{
		slot.item = new InventoryItem(item.ItemDefinition, item.CurrentStack);

		//equip world items
		if (IsRangedWeaponSlot(slot.equipmentType))
		{
			EquipRangedWeapon(slot);
		}
		else if (IsMeleeWeaponSlot(slot.equipmentType))
		{
			EquipMeleeWeapon(slot);
		}
		else if (IsArmourSlot(slot.equipmentType))
		{
			EquipArmour(slot);
		}
		else if (IsConsumableSlot(slot.equipmentType))
		{
			//no world object to instantiate
		}

		Debug.Log($"equipped {item.ItemDefinition.ItemName} to {slot.equipmentType} slot");
	}
	#endregion

	#region unequipping world items and equipment slots
	private void HandleItemUnequipping(EquipmentSlot slot)
	{
		//unequip world items
		if (IsRangedWeaponSlot(slot.equipmentType))
		{
			UnEquipRangedWeapon(slot);
		}
		else if (IsMeleeWeaponSlot(slot.equipmentType))
		{
			UnEquipMeleeWeapon(slot);
		}
		else if (IsArmourSlot(slot.equipmentType))
		{
			UnEquipArmour(slot);
		}
		else if (IsConsumableSlot(slot.equipmentType))
		{
			//no world object
		}

		Debug.Log($"unequipped {slot.item.ItemDefinition.ItemName} from {slot.equipmentType} slot");
		slot.item = null;
	}
	#endregion

	#region equipping/unequipping ranged weapon
	private void EquipRangedWeapon(EquipmentSlot slot)
	{
		if (!IsRangedWeaponSlot(slot.equipmentType))
		{
			Debug.LogError($"{slot.equipmentType} is not a ranged weapon slot");
			return;
		}

		//get or create the world weapon instance
		if (!equippedRangedWeapons.TryGetValue(slot.equipmentType, out var weaponInstance) || weaponInstance == null)
		{
			weaponInstance = InstantiateRangedWeapon();
			equippedRangedWeapons[slot.equipmentType] = weaponInstance;
		}

		weaponInstance.gameObject.SetActive(true);
		weaponInstance.InitializeItem((WeaponRangedDefinition)slot.item.ItemDefinition, slot.item.CurrentStack);
	}

	private void UnEquipRangedWeapon(EquipmentSlot slot)
	{
		if (!IsRangedWeaponSlot(slot.equipmentType))
		{
			Debug.LogError($"{slot.equipmentType} is not a ranged weapon slot");
			return;
		}

		//get weapon instance
		if (!equippedRangedWeapons.TryGetValue(slot.equipmentType, out var weaponInstance) || weaponInstance == null)
			equippedRangedWeapons[slot.equipmentType] = weaponInstance;

		//destroy or disable game object
		weaponInstance.gameObject.SetActive(false);
	}
	#endregion

	#region equipping/unequipping melee weapon
	private void EquipMeleeWeapon(EquipmentSlot slot)
	{
		if (!IsMeleeWeaponSlot(slot.equipmentType) && slot.item.ItemDefinition)
		{
			Debug.LogError($"{slot.equipmentType} is not a melee weapon slot");
			return;
		}

		//get or create the world weapon instance
		if (!equippedMeleeWeapon.TryGetValue(slot.equipmentType, out var weaponInstance) || weaponInstance == null)
		{
			weaponInstance = InstantiateMeleeWeapon();
			equippedMeleeWeapon[slot.equipmentType] = weaponInstance;
		}

		weaponInstance.gameObject.SetActive(true);
		weaponInstance.InitializeItem((WeaponMeleeDefinition)slot.item.ItemDefinition, slot.item.CurrentStack);
	}

	private void UnEquipMeleeWeapon(EquipmentSlot slot)
	{
		if (!IsMeleeWeaponSlot(slot.equipmentType))
		{
			Debug.LogError($"{slot.equipmentType} is not a weapon slot");
			return;
		}

		//get weapon instance
		if (!equippedMeleeWeapon.TryGetValue(slot.equipmentType, out var weaponInstance) || weaponInstance == null)
			equippedMeleeWeapon[slot.equipmentType] = weaponInstance;

		//destroy or disable game object
		weaponInstance.gameObject.SetActive(false);
	}
	#endregion

	#region equipping/unequipping armour and updated stats
	private void EquipArmour(EquipmentSlot slot)
	{
		if (!IsArmourSlot(slot.equipmentType))
		{
			Debug.LogError($"{slot.equipmentType} is not an armour slot");
			return;
		}

		//get or create the world armour instance
		if (!equippedArmour.TryGetValue(slot.equipmentType, out var armourInstance) || armourInstance == null)
		{
			armourInstance = InstantiateArmour();
			equippedArmour[slot.equipmentType] = armourInstance;
		}

		armourInstance.gameObject.SetActive(true);
		armourInstance.InitializeItem((ArmourDefinition)slot.item.ItemDefinition, slot.item.CurrentStack);
		UpdateArmourStats();
	}

	private void UnEquipArmour(EquipmentSlot slot)
	{
		if (!IsArmourSlot(slot.equipmentType))
		{
			Debug.LogError($"{slot.equipmentType} is not an armour slot");
			return;
		}

		//get world armour instance
		if (!equippedArmour.TryGetValue(slot.equipmentType, out var armourInstance) || armourInstance == null)
			equippedArmour[slot.equipmentType] = armourInstance;

		//destroy or disable game object
		UpdateArmourStats();
		armourInstance.gameObject.SetActive(false);
	}

	private void UpdateArmourStats()
	{
		var armourPieces = equippedArmour.Values
			.Where(a => a != null) //ignore null armours
			.ToArray();

		CharacterStats.RecalculateArmourProtectionStats(armourPieces);
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
	public void UseConsumable(EquipmentType equipmentType) //called via player inputs or from ai logic
	{
		EquipmentSlot equipmentSlot = GetEquipmentSlot(equipmentType);
		InventoryItem equippedItem = CheckForEquippedItem(equipmentType);

		if (equippedItem == null) return;

		if (!IsConsumableSlot(equipmentType))
		{
			Debug.LogError($"{equipmentType} is not a consumable slot");
			return;
		}

		Debug.Log($"used consumable {equippedItem.ItemDefinition.ItemName} in {equipmentType} slot");

		CharacterStats.UseConsumable((ConsumableDefinition)equippedItem.ItemDefinition);
		equippedItem.RemoveItemStack(1);

		if (equippedItem.CurrentStack <= 0)
		{
			Debug.Log($"no more consumables left in {equipmentType} slot, deleting item");
			HandleItemUnequipping(equipmentSlot);
		}
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
			Debug.LogWarning($"slot {equipmentType} has an item but definition is null");
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

	#region slot-item type match
	private bool SlotAndItemTypeMatch(EquipmentSlot slot, InventoryItem item)
	{
		InventorySlotType itemAllowedInSlots = item.ItemDefinition.AllowedSlots;

		//equip world items
		if (IsRangedWeaponSlot(slot.equipmentType) && itemAllowedInSlots.HasFlag(InventorySlotType.weaponRanged))
			return true;
		else if (IsMeleeWeaponSlot(slot.equipmentType) && itemAllowedInSlots.HasFlag(InventorySlotType.weaponMelee))
			return true;
		else if (IsArmourSlot(slot.equipmentType) && itemAllowedInSlots.HasFlag(InventorySlotType.armour))
			return true;
		else if (IsConsumableSlot(slot.equipmentType) && itemAllowedInSlots.HasFlag(InventorySlotType.consumable))
			return true;
		else
		{
			Debug.LogWarning("slot type and items allowed slots dont match");
			return false;
		}
	}
	#endregion

	#region slot type fliters
	private bool IsRangedWeaponSlot(EquipmentType type)
	{
		return type == EquipmentType.weaponOne
			|| type == EquipmentType.weaponTwo;
	}
	private bool IsMeleeWeaponSlot(EquipmentType type)
	{
		return type == EquipmentType.weaponMelee;
	}
	private bool IsArmourSlot(EquipmentType type)
	{
		return type == EquipmentType.helmet
			|| type == EquipmentType.chest
			|| type == EquipmentType.backpack;
	}
	private bool IsConsumableSlot(EquipmentType type)
	{
		return type == EquipmentType.consumableOne
			|| type == EquipmentType.consumableTwo
			|| type == EquipmentType.consumableThree;
	}
	#endregion
}

[System.Serializable]
public class EquipmentSlot
{
	public EquipmentType equipmentType;
	public InventoryItem item;

	public EquipmentSlot(EquipmentType type, InventoryItem item)
	{
		this.equipmentType = type;
		this.item = item;
	}
}
