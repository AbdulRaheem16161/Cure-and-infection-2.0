using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class InventorySlotUi : MonoBehaviour
{
	#region inventory slot ui
	[Header("Inventory Slot Ui")]
	public GameObject inventorySlotUi;
	public Image itemInventoryIcon;
	public TMP_Text itemNameText;
	public TMP_Text itemCountText;
	#endregion

	#region inventory slot settings
	[Header("Inventory Slot Settings")]
	[SerializeField] private int slotIndex;
	[SerializeField] private EquipmentHandler.EquipmentType slotEquipmentType;
	#endregion

	#region inventory refs (passed from InventoryUi, should hopefully be reusable for something like ShopUi as it uses an inventory)
	[SerializeField] private InventoryHandler inventoryToRepresent;
	[SerializeField] private EquipmentHandler equipmentToRepresent;
	#endregion

	private void Awake()
	{
		slotIndex = transform.GetSiblingIndex();
		UpdateSlotUi(null);
	}

	/// <summary>
	/// will probably need consumable uses event listener for EquipmentHandler for quick consumable slots
	/// </summary>

	private void OnDestroy()
	{
		if (inventoryToRepresent != null)
			inventoryToRepresent.OnInventoryItemChanged -= HandleItemChanges;

		if (equipmentToRepresent != null)
		{
			equipmentToRepresent.OnItemEquip -= HandleEquippingItem;
			equipmentToRepresent.OnItemUnEquip -= HandleUnequippingItem;
			equipmentToRepresent.OnConsumableUsed -= HandleEquippingItem;
		}
	}

	#region enable/disable inventory slot
	public void EnableSlot(InventoryHandler inventoryToRepresent)
	{
		if (inventoryToRepresent != null) //sub to events
		{
			this.inventoryToRepresent = inventoryToRepresent;
			inventoryToRepresent.OnInventoryItemChanged += HandleItemChanges;
		}

		inventorySlotUi.SetActive(true);
	}
	public void DisableSlot()
	{
		inventorySlotUi.SetActive(false);

		if (inventoryToRepresent != null) //unsub to events
			inventoryToRepresent.OnInventoryItemChanged -= HandleItemChanges;

		UpdateSlotUi(null);
	}
	#endregion

	#region enable/disable equipment slot
	public void EnableEquipmentSlot(EquipmentHandler equipmentToRepresent, EquipmentHandler.EquipmentType equipmentType)
	{
		inventorySlotUi.SetActive(true);

		if (equipmentToRepresent != null) //sub to events
		{
			this.equipmentToRepresent = equipmentToRepresent;
			this.slotEquipmentType = equipmentType;
			equipmentToRepresent.OnItemEquip += HandleEquippingItem;
			equipmentToRepresent.OnItemUnEquip += HandleUnequippingItem;
			equipmentToRepresent.OnConsumableUsed += HandleEquippingItem;
		}

		inventorySlotUi.SetActive(true);
	}
	public void DisableEquipmentSlot()
	{
		inventorySlotUi.SetActive(false);

		if (equipmentToRepresent != null) //unsub to events
		{
			equipmentToRepresent.OnItemEquip -= HandleEquippingItem;
			equipmentToRepresent.OnItemUnEquip -= HandleUnequippingItem;
			equipmentToRepresent.OnConsumableUsed -= HandleEquippingItem;
		}

		UpdateSlotUi(null);
	}
	#endregion

	#region equipment slot listeners
	private void HandleEquippingItem(EquipmentSlot slot)
	{
		if (equipmentToRepresent == null) return;
		if (slotEquipmentType != slot.equipmentType) return; //not correct equipment slot type

		UpdateSlotUi(slot.item);
	}
	private void HandleUnequippingItem(EquipmentSlot slot)
	{
		if (equipmentToRepresent == null) return;
		if (slotEquipmentType != slot.equipmentType) return; //not correct equipment slot type

		UpdateSlotUi(null);
	}
	#endregion

	#region inventory listener
	private void HandleItemChanges(int slot, InventoryItem item)
	{
		if (inventoryToRepresent == null) return; //doesnt have inventory to represent
		if (slotIndex != slot) return; //not correct slot

		UpdateSlotUi(item);
	}
	#endregion

	#region updating ui elements
	private void UpdateSlotUi(InventoryItem item)
	{
		if (item != null)
		{
			//while no ui icons for items just change colour to green, when they do uncomment warning log
			//itemInventoryIcon = item.ItemDefinition.ItemUiIcon;

			if (item.ItemDefinition.ItemUiIcon == null)
			{
				itemInventoryIcon.color = new(0, 0.5882353f, 0);
				//Debug.LogWarning("item has no ui icon, add one"); 
			}
			else
				itemInventoryIcon.sprite = item.ItemDefinition.ItemUiIcon;

			itemNameText.text = item.ItemDefinition.ItemName;
			itemCountText.text = item.CurrentStack.ToString();
		}
		else
		{
			itemInventoryIcon.sprite = null;
			itemInventoryIcon.color = Color.white; //reset colour
			itemNameText.text = "";
			itemCountText.text = "";
		}
	}
	#endregion
}
