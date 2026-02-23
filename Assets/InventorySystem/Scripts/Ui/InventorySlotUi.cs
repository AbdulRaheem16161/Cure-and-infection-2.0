using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUi : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
	#region inventory slot ui
	[Header("Inventory Slot Ui")]
	public GameObject inventorySlotUi;
	public Image itemInventoryIcon;
	public TMP_Text itemNameText;
	public TMP_Text itemCountText;
	#endregion

	#region draggable ui
	[Header("Draggable Ui")]
	public GameObject draggableUi;
	public Image draggableIcon;
	public TMP_Text draggableNameText;
	public TMP_Text draggableCountText;
	#endregion

	#region inventory slot settings
	[Header("Inventory Slot Settings")]
	[SerializeField] private bool canBeDragged;
	[SerializeField] private bool isBeingDragged;
	[SerializeField] private int slotIndex;
	[SerializeField] private EquipmentHandler.EquipmentType slotEquipmentType;
	#endregion

	#region inventory refs (passed from InventoryUi, should hopefully be reusable for something like ShopUi as it uses an inventory)
	[SerializeField] private InventoryHandler inventoryToRepresent;
	[SerializeField] private EquipmentHandler equipmentToRepresent;
	#endregion

	[SerializeField] private GameObject canvasParent; //used for draggable ui

	private void Awake()
	{
		canvasParent = gameObject.transform.parent.transform.parent.transform.parent.transform.parent.gameObject; //grab parent canvas in hierarchy
		slotIndex = transform.GetSiblingIndex();
		UpdateSlotUi(null);
	}

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

	#region I drag event listeners
	public void OnBeginDrag(PointerEventData eventData)
	{
		if (!canBeDragged) return;
		Debug.LogWarning("begin drag");
		draggableUi.transform.SetParent(canvasParent.transform);
		draggableUi.SetActive(true);
		isBeingDragged = true;
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (!isBeingDragged) return;
		draggableUi.transform.position = eventData.position;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (!isBeingDragged) return;
		Debug.LogWarning("end drag");
		draggableUi.SetActive(false);
		draggableUi.transform.SetParent(gameObject.transform);
		isBeingDragged = false;
	}
	#endregion

	/// <summary>
	/// consider if its worth switching to events so ui isnt directly calling game logic
	/// fine for now but when swapping inventory items to equipment slots (+ equipment slots to inventory items) i use add new item which finds the
	/// first empty slot. instead of adding it to the slot player dragged item into or from. 
	/// </summary>

	#region I drop event listener
	public void OnDrop(PointerEventData eventData)
	{
		Debug.LogWarning("dropped");
		GameObject draggedObject = eventData.pointerDrag;
		if (draggedObject == null)
		{
			Debug.LogError("dragged item null");
			return;
		}

		if (!draggedObject.TryGetComponent<InventorySlotUi>(out var draggedSlotUi))
		{
			Debug.LogError("dragged item has no InventorySlotUi component");
			return;
		}

		if (draggedSlotUi == this) return;

		if (IsInventorySlot(draggedSlotUi) && IsInventorySlot(this))
			inventoryToRepresent.SwapItemsInSlots(draggedSlotUi.slotIndex, slotIndex);

		else if (IsEquipmentSlot(draggedSlotUi) && IsEquipmentSlot(this))
			equipmentToRepresent.EquipItemFromEquipment(draggedSlotUi.slotEquipmentType, slotEquipmentType);

		else if (IsInventorySlot(draggedSlotUi) && IsEquipmentSlot(this))
			equipmentToRepresent.EquipItemFromInventory(draggedSlotUi.slotIndex, slotEquipmentType);

		else if (IsEquipmentSlot(draggedSlotUi) && IsInventorySlot(this))
			draggedSlotUi.equipmentToRepresent.EquipItemFromInventory(slotIndex, draggedSlotUi.slotEquipmentType);

		else
			Debug.LogError("drag and drop action not supported");
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
			canBeDragged = true;

			if (item.ItemDefinition.ItemUiIcon == null)
			{
				itemInventoryIcon.color = new(0, 0.5882353f, 0);
				draggableIcon.color = new(0, 0.5882353f, 0);
				//Debug.LogWarning("item has no ui icon, add one"); 
			}
			else
			{
				itemInventoryIcon.sprite = item.ItemDefinition.ItemUiIcon;
				draggableIcon.sprite = item.ItemDefinition.ItemUiIcon;
			}

			itemNameText.text = item.ItemDefinition.ItemName;
			draggableNameText.text = item.ItemDefinition.ItemName;

			itemCountText.text = item.CurrentStack.ToString();
			draggableCountText.text = item.CurrentStack.ToString();
		}
		else
		{
			canBeDragged = false;

			itemInventoryIcon.sprite = null;
			itemInventoryIcon.color = Color.white; //reset colour
			draggableIcon.sprite = null;
			draggableIcon.color = Color.white;

			itemNameText.text = "";
			draggableNameText.text = "";

			itemCountText.text = "";
			draggableCountText.text = "";
		}
	}
	#endregion

	#region slot checks
	private bool IsInventorySlot(InventorySlotUi inventorySlot)
	{
		if (inventorySlot.inventoryToRepresent != null)
			return true;
		else 
			return false;
	}
	private bool IsEquipmentSlot(InventorySlotUi inventorySlot)
	{
		if (inventorySlot.equipmentToRepresent != null)
			return true;
		else
			return false;
	}
	#endregion
}
