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
	[HideInInspector] public int equipItemFromSlot;
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
		InventoryItem itemToEquip = new(item, stackCount);
		HandleItemEquipping(itemToEquip, equipmentType);
	}

	/// <summary>
	/// equip item from inventory, returning existing item to inventory by default, always use for player 
	/// </summary>
	public void EquipItemFromInventory(int itemSlot, EquipmentType equipmentType, bool returnItem = true)
	{
		InventoryItem equippedItem = CheckForEquippedItem(equipmentType);

		if (equippedItem != null && returnItem) //return item
		{
			if (InventoryHandler.InventoryFull())
			{
				Debug.LogWarning("inventory full, cannot equip new item and return old one");
				return;
			}

			HandleItemUnequipping(equipmentType);
			InventoryHandler.AddNewItemPickUp(equippedItem);
		}

		InventoryItem itemToEquip = InventoryHandler.InventoryItems[itemSlot]; //equip new item

		HandleItemEquipping(itemToEquip, equipmentType);
		InventoryHandler.RemoveItemsFromSlot(itemSlot, itemToEquip.CurrentStack);
	}

	/// <summary>
	/// unequip item, returning existing item to inventory by default
	/// </summary>
	public void UnequipItem(EquipmentType equipmentType, bool returnItem = true)
	{
		InventoryItem equippedItem = CheckForEquippedItem(equipmentType);

		if (equippedItem == null) return; //unequip equipped item
		HandleItemUnequipping(equipmentType);

		if (returnItem && !InventoryHandler.InventoryFull()) //return equipped item
			InventoryHandler.AddNewItemPickUp(equippedItem);
		else
			Debug.LogWarning("inventory full, cannot unequip item");
	}
	#endregion

	/// <summary>
	/// move logic that checks equipment slot type and itemsAllowedSlots into seperate check to fix items beign removed from inventory
	/// but failing to be equipped due to:	Debug.LogWarning("item allowed slot types and equipment slot type dont match");
	/// </summary>

	#region equipping world items and equipment slots
	private void HandleItemEquipping(InventoryItem item, EquipmentType equipmentType)
	{
		if (!slotLookup.TryGetValue(equipmentType, out var slot))
		{
			Debug.LogError($"No slot found for {equipmentType}");
			return;
		}

		InventorySlotType itemsAllowedSlots = item.ItemDefinition.AllowedSlots;

		//equip world items
		if (IsRangedWeaponSlot(slot.equipmentType) && itemsAllowedSlots.HasFlag(InventorySlotType.weaponRanged))
		{
			slot.item = new InventoryItem(item.ItemDefinition, item.CurrentStack);
			EquipRangedWeapon(slot.equipmentType);
		}
		else if (IsMeleeWeaponSlot(slot.equipmentType) && itemsAllowedSlots.HasFlag(InventorySlotType.weaponMelee))
		{
			slot.item = new InventoryItem(item.ItemDefinition, item.CurrentStack);
			EquipMeleeWeapon(slot.equipmentType);
		}

		else if (IsArmourSlot(slot.equipmentType) && itemsAllowedSlots.HasFlag(InventorySlotType.armour))
		{
			slot.item = new InventoryItem(item.ItemDefinition, item.CurrentStack);
			EquipArmour(equipmentType);
		}
		else if (IsConsumableSlot(slot.equipmentType) && itemsAllowedSlots.HasFlag(InventorySlotType.consumable))
		{
			slot.item = new InventoryItem(item.ItemDefinition, item.CurrentStack);
			//no world object to instantiate
		}
		else
			Debug.LogWarning("item allowed slot types and equipment slot type dont match");
	}
	#endregion

	#region unequipping world items and equipment slots
	private void HandleItemUnequipping(EquipmentType equipmentType)
	{
		EquipmentSlot slot = GetEquipmentSlot(equipmentType);

		//unequip world items
		if (IsRangedWeaponSlot(slot.equipmentType))
		{
			UnEquipRangedWeapon(slot.equipmentType);
		}
		else if (IsMeleeWeaponSlot(slot.equipmentType))
		{
			UnEquipMeleeWeapon(slot.equipmentType);
		}
		else if (IsArmourSlot(slot.equipmentType))
		{
			UnEquipArmour(equipmentType);
		}
		else if (IsConsumableSlot(slot.equipmentType))
		{
			//no world object
		}
		else
			Debug.LogWarning("item type and equipment slot type dont match");

		slot.item = null;
	}
	#endregion

	#region equipping/unequipping ranged weapon
	private void EquipRangedWeapon(EquipmentType equipmentType)
	{
		EquipmentSlot slot = GetEquipmentSlot(equipmentType);

		if (slot == null)
		{
			Debug.LogError($"No slot found for {equipmentType}");
			return;
		}

		if (!IsRangedWeaponSlot(equipmentType))
		{
			Debug.LogError($"{equipmentType} is not a ranged weapon slot");
			return;
		}

		//get or create the world weapon instance
		if (!equippedRangedWeapons.TryGetValue(equipmentType, out var weaponInstance) || weaponInstance == null)
		{
			weaponInstance = InstantiateRangedWeapon();
			equippedRangedWeapons[equipmentType] = weaponInstance;
		}

		weaponInstance.gameObject.SetActive(true);
		weaponInstance.InitializeItem((WeaponRangedDefinition)slot.item.ItemDefinition, slot.item.CurrentStack);
	}

	private void UnEquipRangedWeapon(EquipmentType equipmentType)
	{
		if (!slotLookup.TryGetValue(equipmentType, out var slot) || slot.item == null)
		{
			Debug.LogError($"no ranged weapon in slot {equipmentType}");
			return;
		}

		if (!IsRangedWeaponSlot(equipmentType))
		{
			Debug.LogError($"{equipmentType} is not a ranged weapon slot");
			return;
		}

		//get weapon instance
		if (!equippedRangedWeapons.TryGetValue(equipmentType, out var weaponInstance) || weaponInstance == null)
			equippedRangedWeapons[equipmentType] = weaponInstance;

		//destroy or disable game object
		weaponInstance.gameObject.SetActive(false);
	}
	#endregion

	#region equipping/unequipping melee weapon
	private void EquipMeleeWeapon(EquipmentType equipmentType)
	{
		if (!slotLookup.TryGetValue(equipmentType, out var slot) || slot.item == null)
		{
			Debug.LogError($"no melee weapon in slot {equipmentType}");
			return;
		}

		if (!IsMeleeWeaponSlot(equipmentType) && slot.item.ItemDefinition)
		{
			Debug.LogError($"{equipmentType} is not a melee weapon slot");
			return;
		}

		//get or create the world weapon instance
		if (!equippedMeleeWeapon.TryGetValue(equipmentType, out var weaponInstance) || weaponInstance == null)
		{
			weaponInstance = InstantiateMeleeWeapon();
			equippedMeleeWeapon[equipmentType] = weaponInstance;
		}

		weaponInstance.gameObject.SetActive(true);
		weaponInstance.InitializeItem((WeaponMeleeDefinition)slot.item.ItemDefinition, slot.item.CurrentStack);
	}

	private void UnEquipMeleeWeapon(EquipmentType equipmentType)
	{
		if (!slotLookup.TryGetValue(equipmentType, out var slot) || slot.item == null)
		{
			Debug.LogError($"No melee weapon in slot {equipmentType}");
			return;
		}

		if (!IsMeleeWeaponSlot(equipmentType))
		{
			Debug.LogError($"{equipmentType} is not a weapon slot");
			return;
		}

		//get weapon instance
		if (!equippedMeleeWeapon.TryGetValue(equipmentType, out var weaponInstance) || weaponInstance == null)
			equippedMeleeWeapon[equipmentType] = weaponInstance;

		//destroy or disable game object
		weaponInstance.gameObject.SetActive(false);
	}
	#endregion

	#region equipping/unequipping armour and updated stats
	private void EquipArmour(EquipmentType equipmentType)
	{
		if (!slotLookup.TryGetValue(equipmentType, out var slot) || slot.item == null)
		{
			Debug.LogError($"No armour in slot {equipmentType}");
			return;
		}

		if (!IsArmourSlot(equipmentType))
		{
			Debug.LogError($"{equipmentType} is not an armour slot");
			return;
		}

		//get or create the world armour instance
		if (!equippedArmour.TryGetValue(equipmentType, out var armourInstance) || armourInstance == null)
		{
			armourInstance = InstantiateArmour();
			equippedArmour[equipmentType] = armourInstance;
		}

		armourInstance.gameObject.SetActive(true);
		armourInstance.InitializeItem((ArmourDefinition)slot.item.ItemDefinition, slot.item.CurrentStack);
		UpdateArmourStats();
	}

	private void UnEquipArmour(EquipmentType equipmentType)
	{
		if (!slotLookup.TryGetValue(equipmentType, out var slot) || slot.item == null)
		{
			Debug.LogError($"No armour in slot {equipmentType}");
			return;
		}

		if (!IsArmourSlot(equipmentType))
		{
			Debug.LogError($"{equipmentType} is not an armour slot");
			return;
		}

		//get world armour instance
		if (!equippedArmour.TryGetValue(equipmentType, out var armourInstance) || armourInstance == null)
			equippedArmour[equipmentType] = armourInstance;

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

	#region using consumables in quick slots (TODO test + refactor UseItem() method into CharacterStats or similar script and ItemDefinition as argument)
	public void UseConsumable(EquipmentType equipmentType) //called via player inputs or from ai logic
	{
		if (!slotLookup.TryGetValue(equipmentType, out var slot))
		{
			Debug.LogError($"No slot found for {equipmentType}");
			return;
		}

		if (slot.item == null)
		{
			Debug.LogWarning($"No item equipped in {equipmentType}");
			return;
		}

		if (!IsConsumableSlot(equipmentType))
		{
			Debug.LogError($"{equipmentType} is not a consumable slot");
			return;
		}

		slot.item.UseItem();
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
