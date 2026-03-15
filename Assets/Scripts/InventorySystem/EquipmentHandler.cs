using System;
using System.Collections.Generic;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using static EquipmentHandler;
using static ItemDefinition;

public class EquipmentHandler : MonoBehaviour
{
	/// <summary>
	/// item equipping/unequipping logic could do with a refactor to cut out duplicate logic
	/// will consider it more 
	/// </summary>

	public InventoryHandler InventoryHandler { get; private set; }
	private bool _Initialized = false;

	#region equipment slots + dictionary lookup
	[Header("Equipment Settings")]
	public Dictionary<EquipmentType, EquipmentSlot> slotLookup = new();
	public List<EquipmentSlot> equipmentSlots = new();
	#endregion

	/// <summary>
	/// currently uses empty game objects on character model to parent items to, can be changed to better setup
	/// or refined as atm its done quick and dirty whilst item and character models are lacking.
	/// </summary>

	#region equipped world items
	[Header("Equipped World Items")]
	[Header("Items Not In Hands")]
	public GameObject equippedHelmetParent;
	public GameObject equippedChestpieceParent;
	public GameObject equippedBackpackParent;
	public GameObject equippedWeaponsParent;
	public GameObject ItemsInHandsParent;
	public Dictionary<EquipmentType, WeaponRanged> equippedRangedWeapons = new();
	public Dictionary<EquipmentType, WeaponMelee> equippedMeleeWeapon = new();
	public Dictionary<EquipmentType, Armour> equippedArmour = new();
	#endregion

	#region item in hands
	public bool HasRangedWeaponInHands {  get; private set; }
	public bool HasMeleeWeaponInHands {  get; private set; }
	[ReadOnly] public WeaponRanged rangedWeaponInHands;
	[ReadOnly] public WeaponMelee meleeWeaponInHands;
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

	#region initialize equipment
	private void Awake()
	{
		#region back up initilze for testing
		if (!_Initialized)
			InitializeEquipmentHandler(GetComponent<InventoryHandler>(), null);
		#endregion
	}
	public void InitializeEquipmentHandler(InventoryHandler inventoryHandler, NpcDefinition npcDefinition)
	{
		#region check for required components
		_Initialized = true;
		InventoryHandler = inventoryHandler;

		if (InventoryHandler == null)
		{
			Debug.LogError($"InventoryHandler script not found on this gameobject: {gameObject.name}");
			return;
		}
		#endregion

		#region set up equipment lookup
		equipmentSlots = new();
		slotLookup = new();

		foreach (EquipmentType type in Enum.GetValues(typeof(EquipmentType)))
		{
			if (!slotToInventoryType.TryGetValue(type, out var slotType))
			{
				if (slotType == InventorySlotType.none) continue;
				Debug.LogWarning($"No InventorySlotType mapped for {type}, skipping.");
				continue;
			}

			EquipmentSlot newSlot = new(slotType, type, new(null, 0));
			equipmentSlots.Add(newSlot);
			slotLookup.Add(type, newSlot);
		}
		#endregion

		EquipNpcEquipment(npcDefinition);
	}
	#endregion

	private void EquipNpcEquipment(NpcDefinition npcDefinition)
	{
		#region auto equip npc items as long as they are not null, + unholster weapons
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
		#endregion
	}

	#region equipping item
	/// <summary>
	/// equip item, replacing any existing item, safe to use for npcs
	/// </summary>
	public void EquipItem(ItemDefinition item, int stackCount, EquipmentType equipmentType)
	{
		#region copy item as InventoryItem, check slot types match and handle equipping item
		EquipmentSlot equipmentSlot = GetEquipmentSlot(equipmentType);
		InventoryItem itemToEquip = new(item, stackCount);

		if (!EquipmentSlotsMatch(equipmentSlot, itemToEquip)) return;

		HandleItemEquipping(itemToEquip, equipmentSlot);
		#endregion
	}

	/// <summary>
	/// equip item from inventory, returning existing item to inventory by default, always use for player 
	/// </summary>
	public void EquipItemFromInventory(int itemSlot, EquipmentType equipmentType, bool returnItem = true)
	{
		#region check for item to equip
		EquipmentSlot equipmentSlot = GetEquipmentSlot(equipmentType);
		InventoryItem equippedItem = CheckForEquippedItem(equipmentType);
		InventoryItem itemToEquip = InventoryHandler.ItemContainer.Items[itemSlot];
		#endregion

		#region unequip and return early if no new item to equip + handle unequiping current item
		if (itemToEquip.ItemDefinition == null && equippedItem != null && returnItem) //return early if no weapon to equip
		{
			HandleItemUnequipping(equipmentSlot);
			InventoryHandler.ItemContainer.AddNewItem(equippedItem);
			return;
		}
		#endregion

		#region check equipment slot type and inventory full, return early if fail, handle unequipping existing item
		if (!EquipmentSlotsMatch(equipmentSlot, itemToEquip)) return;

		if (equippedItem != null && returnItem) //return item
		{
			if (InventoryHandler.ItemContainer.ContainerFull())
			{
				Debug.LogWarning("inventory full, cannot equip new item and return old one");
				return;
			}

			HandleItemUnequipping(equipmentSlot);
			InventoryHandler.ItemContainer.AddNewItem(equippedItem);
		}
		#endregion

		#region handle equipping new item and removing from inventory
		HandleItemEquipping(itemToEquip, equipmentSlot);
		InventoryHandler.RemoveItemsFromSlot(itemSlot, itemToEquip.CurrentStack);
		#endregion
	}
	/// <summary>
	/// equip item from equipment, swapping item places if slot matches
	/// </summary>
	public void EquipItemFromEquipment(EquipmentType currentSlotType, EquipmentType newSlotType)
	{
		#region check for existing items in both equipment slots
		EquipmentSlot currentEquipmentSlot = GetEquipmentSlot(currentSlotType);
		EquipmentSlot newEquipmentSlot = GetEquipmentSlot(newSlotType);

		InventoryItem currentEquippedItem = CheckForEquippedItem(currentSlotType);
		InventoryItem newEquippedItem = CheckForEquippedItem(newSlotType);
		#endregion

		#region check types match, return early if fail
		if (!SlotTypesMatch(currentEquipmentSlot, newEquipmentSlot)) return; //cant swap equipped items
		if (!EquipmentSlotsMatch(newEquipmentSlot, currentEquippedItem)) return;
		#endregion

		#region handle re-equipping both items, if new slot item was empty unequip item after swapping and return early
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
		#endregion
	}
	#endregion

	/// <summary>
	/// unequip item, returning existing item to inventory by default
	/// </summary>
	public void UnequipItem(EquipmentType equipmentType, bool returnItem = true)
	{
		#region check item exists, handle unequipping it
		EquipmentSlot equipmentSlot = GetEquipmentSlot(equipmentType);
		InventoryItem equippedItem = CheckForEquippedItem(equipmentType);

		if (equippedItem == null) return; //no equipped item to unequip
		HandleItemUnequipping(equipmentSlot);
		#endregion

		#region handle returning item to inventory
		if (returnItem)
		{
			if (!InventoryHandler.ItemContainer.ContainerFull()) //return equipped item
				InventoryHandler.ItemContainer.AddNewItem(equippedItem);
			else
				Debug.LogWarning("inventory full, cannot unequip item");
		}
		#endregion
	}

	public void DropItem(EquipmentType equipmentType, bool dropStack)
	{
		#region check item exists, unequip and spawn in world (TODO: instantiate item in world at characters feet)
		EquipmentSlot equipmentSlot = GetEquipmentSlot(equipmentType);
		InventoryItem equippedItem = CheckForEquippedItem(equipmentType);

		if (equippedItem == null) return;

		HandleItemUnequipping(equipmentSlot);

		//spawn world item
		#endregion
	}

	/// <summary>
	/// will need updating to play any equip/unequip sfxs, linking with any animations and vfxs when equipping weapons, armour and using consumables
	/// + proper pos/rot setting to visually be on characters back etc..
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

	//(TODO prob need to give player ammo back in magazine when unequipping)
	public void EquipRangedWeapon(EquipmentSlot slot)
	{
		#region get or create the world weapon instance and initialize
		if (!equippedRangedWeapons.TryGetValue(slot.equipmentType, out var weaponInstance) || weaponInstance == null)
		{
			weaponInstance = InstantiateRangedWeapon();
			equippedRangedWeapons[slot.equipmentType] = weaponInstance;
		}

		weaponInstance.InitializeItem((WeaponRangedDefinition)slot.item.ItemDefinition, slot.item.CurrentStack);
		#endregion

		#region set position on character model and enable
		weaponInstance.transform.SetParent(equippedWeaponsParent.transform);
		weaponInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		weaponInstance.gameObject.SetActive(true);
		#endregion
	}

	public void UnEquipRangedWeapon(EquipmentSlot slot)
	{
		#region get weapon instance and disable                      
		if (!equippedRangedWeapons.TryGetValue(slot.equipmentType, out var weaponInstance) || weaponInstance == null)
			return;

		weaponInstance.gameObject.SetActive(false);
		#endregion
	}

	public void EquipMeleeWeapon(EquipmentSlot slot)
	{
		#region get or create the world weapon instance and initialize
		if (!equippedMeleeWeapon.TryGetValue(slot.equipmentType, out var weaponInstance) || weaponInstance == null)
		{
			weaponInstance = InstantiateMeleeWeapon();
			equippedMeleeWeapon[slot.equipmentType] = weaponInstance;
		}

		weaponInstance.InitializeItem((WeaponMeleeDefinition)slot.item.ItemDefinition, slot.item.CurrentStack);
		#endregion

		#region set position on character model and enable
		weaponInstance.transform.SetParent(equippedWeaponsParent.transform);
		weaponInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		weaponInstance.gameObject.SetActive(true);
		#endregion
	}


	public void UnEquipMeleeWeapon(EquipmentSlot slot)
	{
		#region get weapon instance and disable
		if (!equippedMeleeWeapon.TryGetValue(slot.equipmentType, out var weaponInstance) || weaponInstance == null)
			return;

		//destroy or disable game object
		weaponInstance.gameObject.SetActive(false);
		#endregion
	}

	public void EquipArmour(EquipmentSlot slot)
	{
		#region get or create the world armour instance and initialize
		//get or create the world armour instance
		if (!equippedArmour.TryGetValue(slot.equipmentType, out var armourInstance) || armourInstance == null)
		{
			armourInstance = InstantiateArmour();
			equippedArmour[slot.equipmentType] = armourInstance;
		}

		armourInstance.InitializeItem((ArmourDefinition)slot.item.ItemDefinition, slot.item.CurrentStack);
		#endregion

		#region set position on character model and enable
		if (armourInstance.ArmourDefinition.ArmourSlot == ArmourDefinition.ArmourSlotType.helmet)
			armourInstance.transform.SetParent(equippedHelmetParent.transform);
		else if (armourInstance.ArmourDefinition.ArmourSlot == ArmourDefinition.ArmourSlotType.chest)
			armourInstance.transform.SetParent(equippedChestpieceParent.transform);
		else if (armourInstance.ArmourDefinition.ArmourSlot == ArmourDefinition.ArmourSlotType.backpack)
			armourInstance.transform.SetParent(equippedBackpackParent.transform);

		armourInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		armourInstance.gameObject.SetActive(true);
		#endregion
	}

	public void UnEquipArmour(EquipmentSlot slot)
	{
		#region get world armour instance and disable
		if (!equippedArmour.TryGetValue(slot.equipmentType, out var armourInstance) || armourInstance == null)
			return;

		armourInstance.gameObject.SetActive(false);
		#endregion
	}

	#region instantiate world objects (weapons/armours)
	private WeaponRanged InstantiateRangedWeapon()
	{
		return Instantiate(WeaponRangedPrefab).GetComponent<WeaponRanged>();
	}
	private WeaponMelee InstantiateMeleeWeapon()
	{
		return Instantiate(WeaponMeleePrefab).GetComponent<WeaponMelee>();
	}
	private Armour InstantiateArmour()
	{
		return Instantiate(ArmourPrefab).GetComponent<Armour>();
	}
	#endregion

	public void UseConsumable(EquipmentType equipmentType)
	{
		#region check for consumable to use
		EquipmentSlot slot = GetEquipmentSlot(equipmentType);
		InventoryItem equippedItem = CheckForEquippedItem(equipmentType);

		if (!equippedItem.ItemDefinition.CanEquipTo(equipmentType))
		{
			Debug.LogError($"{equipmentType} cannot use this item.");
			return;
		}
		#endregion

		#region check if can be consumed (TODO: could add checks like if full health dont consume etc...)
		bool shouldConsume = equippedItem.ItemDefinition.OnUsed(this, slot);
		if (!shouldConsume)
			return;
		#endregion

		#region call event + update stack count and check if count = 0
		equippedItem.RemoveItemStack(1);
		OnConsumableUsed?.Invoke(slot);

		if (equippedItem.CurrentStack <= 0)
			HandleItemUnequipping(slot);
		#endregion
	}

	public void HolsterWeapon()
	{
		#region holster equipped weapon if exists
		GameObject weaponObject = null;
		if (rangedWeaponInHands)
		{
			HasRangedWeaponInHands = false;
			weaponObject = rangedWeaponInHands.gameObject;
			rangedWeaponInHands.UnEquipWeapon();
			rangedWeaponInHands = null;
		}
		if (meleeWeaponInHands)
		{
			HasMeleeWeaponInHands = false;
			weaponObject = meleeWeaponInHands.gameObject;
			meleeWeaponInHands.UnEquipWeapon();
			meleeWeaponInHands = null;
		}

		if (weaponObject == null) //no weapon obj to move
			return;
		#endregion

		#region set parent and transfrom
		weaponObject.transform.SetParent(equippedWeaponsParent.transform);
		weaponObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		#endregion
	}
	public void UnholsterWeapon(EquipmentType equipmentType)
	{
		#region check for equipped weapon on back etc.. and unholster
		HolsterWeapon(); //holster current weapon if any

		GameObject weaponObject;
		if (equipmentType == EquipmentType.weaponOne || equipmentType == EquipmentType.weaponTwo)
		{
			if (equippedRangedWeapons.TryGetValue(equipmentType, out var weapon))
			{
				rangedWeaponInHands = weapon;
				rangedWeaponInHands.EquipWeapon();
				HasRangedWeaponInHands = true;
				weaponObject = rangedWeaponInHands.gameObject;
			}
			else
				return;
		}
		else if (equipmentType == EquipmentType.weaponMelee)
		{
			if (equippedMeleeWeapon.TryGetValue(equipmentType, out var weapon))
			{
				meleeWeaponInHands = weapon;
				meleeWeaponInHands.EquipWeapon();
				HasMeleeWeaponInHands = true;
				weaponObject = meleeWeaponInHands.gameObject;
			}
			else
				return;
		}
		else
			return; //wrong equipment type
		#endregion

		#region set parent and transform
		weaponObject.transform.SetParent(ItemsInHandsParent.transform);
		weaponObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		#endregion
	}

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
	private bool EquipmentSlotsMatch(EquipmentSlot slot, InventoryItem item)
	{
		return (slot.equipmentType & item.ItemDefinition.AllowedEquipmentSlots) != 0;
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
