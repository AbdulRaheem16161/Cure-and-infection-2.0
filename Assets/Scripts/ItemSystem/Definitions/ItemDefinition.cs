using System;
using UnityEngine;
using static EquipmentHandler;

[Serializable]
public class ItemDefinition : ScriptableObject
{
	#region core item info
	[Header("Item Info")]
	[SerializeField] private string itemId;
	[SerializeField] private string itemName;
	[SerializeField] private string itemDescription;
	[SerializeField] private int itemPrice;
	#endregion

	#region inventory properties
	[SerializeField] private bool tradable;
	[Header("Inventory Properties")]
	[SerializeField] private InventorySlotType allowedSlots;
	[Flags]
	public enum InventorySlotType //move into inventory system at some point
	{
		none = 0,
		basic = 1 << 0,
		weaponRanged = 1 << 1,
		weaponMelee = 1 << 2,
		armour = 1 << 3,
		consumable = 1 << 4
	}

	[SerializeField] private EquipmentType allowedEquipmentSlots;
	[SerializeField] private int stackLimit;
	[SerializeField] private float itemWeight;
	#endregion

	//add fields for ui icons, 3d prefab models etc, sfx/vfx etc...
	#region common item model, vfx, sfx
	[Header("Item Model and Ui")]
	[SerializeField] private GameObject itemPrefab;
	[SerializeField] private Sprite itemUiIcon;
	#endregion

	#region readonly properties
	public string ItemId => itemId;
	public string ItemName => itemName;
	public string ItemDescription => itemDescription;
	public int ItemPrice => itemPrice;

	public bool Tradable => tradable;
	public InventorySlotType AllowedSlots => allowedSlots;
	public EquipmentType AllowedEquipmentSlots => allowedEquipmentSlots;
	public int StackLimit => stackLimit;
	public float ItemWeight => itemWeight;

	public GameObject ItemPrefab => itemPrefab;
	public Sprite ItemUiIcon => itemUiIcon;
	#endregion

	public bool CanEquipTo(EquipmentType slot)
	{
		return (allowedEquipmentSlots & slot) != 0;
	}

	public virtual void OnEquip(EquipmentHandler handler, EquipmentSlot slot) { }
	public virtual void OnUnequip(EquipmentHandler handler, EquipmentSlot slot) { }
	public virtual bool OnUsed(EquipmentHandler handler, EquipmentSlot slot)
	{
		return false; // return true if item was consumed
	}
}
