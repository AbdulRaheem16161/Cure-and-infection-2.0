using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EquipmentHandler : MonoBehaviour
{
	private CharacterStatsTest characterStats;
	private InventoryHandler inventoryHandler;

	[Header("Equipment Settings")]
	#region equipment slots + dictionary lookup
	public Dictionary<EquipmentType, EquipmentSlot> slotLookup = new();
	public List<EquipmentSlot> equipmentSlots = new();
	#endregion

	#region equipped world items
	public Dictionary<EquipmentType, WeaponRanged> equippedWeapons = new();
	public Dictionary<EquipmentType, Armour> equippedArmour = new();
	#endregion

	//add fields for prefab objects to instantiate so i can properly test equipment handler

	#region debug settings
	[Header("Debug Settings")]
	[HideInInspector] public ItemDefinition itemToEquip;
	[HideInInspector] public int itemToEquipCount;
	[HideInInspector] public EquipmentType slotToEquipItemTo;
	[HideInInspector] public EquipmentType equipmentSlotToUnequip;
	#endregion

	public enum EquipmentType
	{
		weaponOne, weaponTwo, weaponSidearm, weaponMelee, helmet, chest, backpack, consumableOne, consumableTwo, consumableThree
	}

	private void Awake()
	{
		characterStats = GetComponent<CharacterStatsTest>();

		if (characterStats == null)
		{
			Debug.LogError($"CharacterStatsTest script not found on this gameobject: {gameObject.name}");
			return;
		}

		inventoryHandler = GetComponent<InventoryHandler>();

		if (inventoryHandler == null)
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

	#region equipping/unequipping items
	/// <summary>
	/// equip item, safe to use for npcs
	/// </summary>
	public void EquipItem(ItemDefinition item, int stackCount, EquipmentType equipmentType)
	{
		InventoryItem itemToEquip = new(item, stackCount);
		HandleItemEquipping(itemToEquip, equipmentType);
	}

	/// <summary>
	/// equip item from inventory, always use for player 
	/// </summary>
	public void EquipItemFromInventory(int itemSlot, EquipmentType equipmentType, bool returnItem = true)
	{
		if (returnItem)
			HandleItemUnequipping(equipmentType, returnItem);

		InventoryItem itemToEquip = inventoryHandler.InventoryItems[itemSlot];

		HandleItemEquipping(itemToEquip, equipmentType);
		inventoryHandler.RemoveItemsFromSlot(itemSlot, itemToEquip.CurrentStack);
	}

	public void UnequipItem(EquipmentType equipmentType, bool returnItem = true)
	{
		HandleItemUnequipping(equipmentType, returnItem);
	}
	#endregion

	#region equippes items to equipment slots (TODO handle spawning armours in world space onto player mode/weapon in players hands/on back)
	private void HandleItemEquipping(InventoryItem item, EquipmentType equipmentType)
	{
		if (!slotLookup.TryGetValue(equipmentType, out var slot))
		{
			Debug.LogError($"No slot found for {equipmentType}");
			return;
		}

		if (IsWeaponSlot(slot.equipmentType) && item.ItemDefinition.AllowedSlots.HasFlag(ItemDefinition.InventorySlotType.weapon))
		{
			slot.item = new InventoryItem(item.ItemDefinition, item.CurrentStack);
			//equip weapon
		}
		else if (IsArmourSlot(slot.equipmentType) && item.ItemDefinition.AllowedSlots.HasFlag(ItemDefinition.InventorySlotType.armour))
		{
			slot.item = new InventoryItem(item.ItemDefinition, item.CurrentStack);
			EquipArmour(equipmentType);
		}
		else if (IsConsumableSlot(slot.equipmentType) && item.ItemDefinition.AllowedSlots.HasFlag(ItemDefinition.InventorySlotType.consumable))
		{
			slot.item = new InventoryItem(item.ItemDefinition, item.CurrentStack);
		}
		else
		{
			Debug.LogWarning("item type and equipment slot type dont match");
		}
	}
	#endregion

	#region unequippes items, by default returning them to inventory if not empty (TODO test + handle destorying world items like equipped helmet/weapon)
	private void HandleItemUnequipping(EquipmentType equipmentType, bool returnItem = true)
	{
		if (!returnItem) return;

		if (!slotLookup.TryGetValue(equipmentType, out var slot))
		{
			Debug.LogError($"No slot found for {equipmentType}");
			return;
		}

		if (EquipmentSlotEmpty(slot)) return;
		inventoryHandler.AddNewItemPickUp(slot.item);
		slot.item = null;
	}
	#endregion

	#region equipping/unequipping armour and updated stats (TODO test + handle destroying/disabling game object + instantiating it)
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

		// Get or create the world armour instance
		if (!equippedArmour.TryGetValue(equipmentType, out var armourInstance) || armourInstance == null)
		{
			//armourInstance = InstantiateArmour(slot.item);
			equippedArmour[equipmentType] = armourInstance;
		}

		armourInstance.InitializeItem((ArmourDefinition)slot.item.ItemDefinition, slot.item.CurrentStack);
		UpdateArmourStats();
	}

	public void UnEquipArmour(EquipmentType equipmentType)
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

		//destroy or disable game object
		UpdateArmourStats();
	}
	#endregion

	#region instantiate armour (TODO needs proper implementation)
	//instantiate prefab, attach to player model etc...
	/*
	private Armour InstantiateArmour(InventoryItem item)
	{
		GameObject armourGO = Instantiate();
		Armour armour = armourGO.GetComponent<Armour>();
		return armour;
	}
	*/
	#endregion

	#region armour helpers (TODO test)
	private void UpdateArmourStats()
	{
		var armourPieces = equippedArmour.Values
			.Where(a => a != null) //ignore null armours
			.ToArray();

		characterStats.RecalculateArmourProtectionStats(armourPieces);
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

	#region slot type fliters
	private bool EquipmentSlotEmpty(EquipmentSlot slot)
	{
		if (slot.item == null)
			return true;
		else
			return false;
	}
	private bool IsWeaponSlot(EquipmentType type)
	{
		return type == EquipmentType.weaponOne
			|| type == EquipmentType.weaponTwo
			|| type == EquipmentType.weaponSidearm
			|| type == EquipmentType.weaponMelee;
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
	public EquipmentHandler.EquipmentType equipmentType;
	public InventoryItem item;

	public EquipmentSlot(EquipmentHandler.EquipmentType type, InventoryItem item)
	{
		this.equipmentType = type;
		this.item = item;
	}
}
