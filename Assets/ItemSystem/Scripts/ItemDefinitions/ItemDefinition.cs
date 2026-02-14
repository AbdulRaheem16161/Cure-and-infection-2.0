using System;
using UnityEngine;

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
		basic = 1, weapon = 2, armour = 4, consumable = 8
	}
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
	public int StackLimit => stackLimit;
	public float ItemWeight => itemWeight;

	public GameObject ItemPrefab => itemPrefab;
	public Sprite ItemUiIcon => itemUiIcon;
	#endregion

	#region common item behaviour methods
	public void DropItem()
	{
		//instantiate object in world space at feet
		Debug.Log($"dropped item: {ItemName}");
	}
	public void BuyItem(InventoryHandler inventory, int buyPirce)
	{
		inventory.RemoveMoney(buyPirce);
	}
	public void SellItem(InventoryHandler inventory, int sellPirce)
	{
		inventory.AddMoney(sellPirce);
	}
	public virtual void UseItem()
	{
		Debug.LogError("UseItem method not implemented for this item type");
	}
	public virtual void EquipItem()
	{
		Debug.LogError("EquipItem method not implemented for this item type");
	}
	public virtual void UnEquipItem()
	{
		Debug.LogError("UnEquipItem method not implemented for this item type");
	}
	public virtual void Holster()
	{
		Debug.LogError("Holster method not implemented for this item type");
	}
	public virtual void UnHolster()
	{
		Debug.LogError("UnHolster method not implemented for this item type");
	}
	#endregion
}
