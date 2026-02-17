using UnityEngine;

public class EquipmentHandler : MonoBehaviour
{
	private InventoryHandler inventoryHandler;

	public enum EquipmentType
	{
		weaponOne, weaponTwo, weaponSidearm, weaponMelee, helmet, chest, backpack, consumableOne, consumableTwo, consumableThree
	}

	#region equipment slots
	[SerializeField] private InventoryItem weaponOne;
	[SerializeField] private InventoryItem weaponTwo;
	[SerializeField] private InventoryItem weaponSidearm;
	[SerializeField] private InventoryItem weaponMelee;

	[SerializeField] private InventoryItem helmet;
	[SerializeField] private InventoryItem chest;
	[SerializeField] private InventoryItem backpack;

	[SerializeField] private InventoryItem consumableOne;
	[SerializeField] private InventoryItem consumableTwo;
	[SerializeField] private InventoryItem consumableThree;
	#endregion

	#region equipped world items
	[SerializeField] private WeaponRanged equippedRangedWeapon;
	[SerializeField] private WeaponMelee equippedMeleeWeapon;

	[SerializeField] private Armour equippedHelmet;
	[SerializeField] private Armour equippedChest;
	[SerializeField] private Armour equippedBackpack;
	#endregion

	private void Awake()
	{
		inventoryHandler = GetComponent<InventoryHandler>();

		if (inventoryHandler == null)
		{
			Debug.LogError($"InventoryHandler script not found on this gameobject: {gameObject.name}");
			return;
		}
	}

	//consider adding an overload method to make giving npcs weapons easier by passing InventoryItem and skipping inventory related logic

	#region equipping/unequipping items
	public void EquipItem(int itemSlot, EquipmentType equipmentType, bool returnItem = true)
	{
		if (returnItem)
			HandleItemReturning(equipmentType, returnItem);

		InventoryItem itemToEquip = inventoryHandler.InventoryItems[itemSlot];

		HandleItemEquipping(itemToEquip, equipmentType);
		inventoryHandler.RemoveItemsFromSlot(itemSlot, itemToEquip.CurrentStack);
	}

	public void UnequipItem(EquipmentType equipmentType, bool returnItem = true)
	{
		HandleItemReturning(equipmentType, returnItem);
	}
	#endregion

	//consider ways to refactor switch case into something easier

	#region equippes items to equipment slots (TODO handle spawning armours in world space onto player mode/weapon in players hands/on back)
	private void HandleItemEquipping(InventoryItem item, EquipmentType equipmentType)
	{
		switch (equipmentType)
		{
			case EquipmentType.weaponOne:
			weaponOne = new(item.ItemDefinition, item.CurrentStack);
			break;
			case EquipmentType.weaponTwo:
			weaponTwo = new(item.ItemDefinition, item.CurrentStack);
			break;
			case EquipmentType.weaponSidearm:
			weaponSidearm = new(item.ItemDefinition, item.CurrentStack);
			break;
			case EquipmentType.weaponMelee:
			weaponMelee = new(item.ItemDefinition, item.CurrentStack);
			break;
			case EquipmentType.helmet:
			helmet = new(item.ItemDefinition, item.CurrentStack);
			break;
			case EquipmentType.chest:
			chest = new(item.ItemDefinition, item.CurrentStack);
			break;
			case EquipmentType.backpack:
			backpack = new(item.ItemDefinition, item.CurrentStack);
			break;
			case EquipmentType.consumableOne:
			consumableOne = new(item.ItemDefinition, item.CurrentStack);
			break;
			case EquipmentType.consumableTwo:
			consumableTwo = new(item.ItemDefinition, item.CurrentStack);
			break;
			case EquipmentType.consumableThree:
			consumableThree = new(item.ItemDefinition, item.CurrentStack);
			break;
		}
	}
	#endregion

	#region unequippes items, by default returning them to inventory if not empty (TODO handle destorying world items like equipped helmet/weapon)
	private void HandleItemReturning(EquipmentType equipmentType, bool returnItem = true)
	{
		if (!returnItem) return;

		switch (equipmentType)
		{
			case EquipmentType.weaponOne:
			if (EquipmentSlotEmpty(weaponOne)) return;
			inventoryHandler.AddNewItemPickUp(weaponOne);
			weaponOne = null;
			break;
			case EquipmentType.weaponTwo:
			if (EquipmentSlotEmpty(weaponTwo)) return;
			inventoryHandler.AddNewItemPickUp(weaponTwo);
			weaponTwo = null;
			break;
			case EquipmentType.weaponSidearm:
			if (EquipmentSlotEmpty(weaponSidearm)) return;
			inventoryHandler.AddNewItemPickUp(weaponSidearm);
			weaponSidearm = null;
			break;
			case EquipmentType.weaponMelee:
			if (EquipmentSlotEmpty(weaponMelee)) return;
			inventoryHandler.AddNewItemPickUp(weaponMelee);
			weaponMelee = null;
			break;
			case EquipmentType.helmet:
			if (EquipmentSlotEmpty(helmet)) return;
			inventoryHandler.AddNewItemPickUp(helmet);
			helmet = null;
			break;
			case EquipmentType.chest:
			if (EquipmentSlotEmpty(chest)) return;
			inventoryHandler.AddNewItemPickUp(chest);
			chest = null;
			break;
			case EquipmentType.backpack:
			if (EquipmentSlotEmpty(backpack)) return;
			inventoryHandler.AddNewItemPickUp(backpack);
			backpack = null;
			break;
			case EquipmentType.consumableOne:
			if (EquipmentSlotEmpty(consumableOne)) return;
			inventoryHandler.AddNewItemPickUp(consumableOne);
			consumableOne = null;
			break;
			case EquipmentType.consumableTwo:
			if (EquipmentSlotEmpty(consumableTwo)) return;
			inventoryHandler.AddNewItemPickUp(consumableTwo);
			consumableTwo = null;
			break;
			case EquipmentType.consumableThree:
			if (EquipmentSlotEmpty(consumableThree)) return;
			inventoryHandler.AddNewItemPickUp(consumableThree);
			consumableThree = null;
			break;
		}
	}
	#endregion

	#region using consumables in quick slots
	public void UseConsumable(EquipmentType equipmentType) //called via player inputs or from ai logic
	{
		InventoryItem item = equipmentType switch
		{
			EquipmentType.consumableOne => consumableOne,
			EquipmentType.consumableTwo => consumableTwo,
			EquipmentType.consumableThree => consumableThree,
			_ => null
		};

		if (item == null)
		{
			Debug.LogError("No consumable in that slot, or equipment type doesnt support UseItem");
			return;
		}

		item.UseItem();
	}
	#endregion

	public bool EquipmentSlotEmpty(InventoryItem itemInEquipmentSlot)
	{
		if (itemInEquipmentSlot == null)
			return true;
		else
			return false;
	}
}
