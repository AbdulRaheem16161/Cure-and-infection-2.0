using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static EquipmentHandler;

public class InventorySlotUi : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
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

	#region runtime info
	[Header("Runtime Info")]
	[SerializeField] private bool canBeDragged;
	[SerializeField] private bool isBeingDragged;
	[SerializeField] private int slotIndex;

	[SerializeField] private GameObject objectRef;
	[SerializeField] private EquipmentHandler equipment;
	[SerializeField] private ItemContainer itemContainer;
	[SerializeField] private InventoryItem slotItem;
	[SerializeField] private EquipmentType equipmentType;
	#endregion

	#region read only runtime info
	public int SlotIndex => slotIndex;
	public GameObject ObjectRef => objectRef;
    public EquipmentHandler Equipment => equipment;
	public ItemContainer ItemContainer => itemContainer;
	public InventoryItem SlotItem => slotItem;
	public EquipmentType EquipmentTypes => equipmentType;
	#endregion

	public static event Action<InventorySlotUi, Vector2> OnToggleInventoryContextMenu;

	[SerializeField] private GameObject canvasParent; //used for draggable ui

	private void Awake()
	{
		canvasParent = gameObject.transform.parent.transform.parent.transform.parent.transform.parent.gameObject; //grab parent canvas in hierarchy
		slotIndex = transform.GetSiblingIndex();
		UpdateSlotUi(null);
	}

	private void OnDestroy()
	{
		if (ItemContainer != null)
            ItemContainer.OnContainerItemChanged -= HandleItemChanges;

		if (equipment != null)
			equipment.OnEquippedItemChanges -= OnEquippedItemChanges;
	}

	#region enable/disable equipment slot
	public void EnableEquipmentSlot(GameObject gamebject, EquipmentHandler equipment, EquipmentType equipmentType)
	{
		inventorySlotUi.SetActive(true);

		if (equipment != null) //sub to events
		{
            objectRef = gamebject;
            this.equipment = equipment;
			this.equipmentType = equipmentType;
			equipment.OnEquippedItemChanges += OnEquippedItemChanges;
			equipment.OnConsumableUsed += OnConsumableUsed;
		}

        UpdateSlotUi(equipment.GetEquipmentSlot(equipmentType).Item);
        inventorySlotUi.SetActive(true);
	}
	public void DisableEquipmentSlot()
	{
		inventorySlotUi.SetActive(false);

		if (equipment != null) //unsub to events
		{
			equipment.OnEquippedItemChanges -= OnEquippedItemChanges;
			equipment.OnConsumableUsed -= OnConsumableUsed;
		}

		UpdateSlotUi(null);
	}
	#endregion

	#region enable/disable inventory slot
	public void EnableSlot(GameObject gamebject, ItemContainer container)
	{
		if (itemContainer != null) //sub to events
		{
            objectRef = gamebject;
            itemContainer = container;
            itemContainer.OnContainerItemChanged += HandleItemChanges;
		}

        UpdateSlotUi(itemContainer.Items[slotIndex]);
        inventorySlotUi.SetActive(true);
	}
	public void DisableSlot()
	{
		inventorySlotUi.SetActive(false);

		if (itemContainer != null) //unsub to events
            itemContainer.OnContainerItemChanged -= HandleItemChanges;

		UpdateSlotUi(null);
	}
	#endregion

	#region i drag event listeners
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

	#region i drop event listener
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

		HandleItemDropEvent(draggedSlotUi);
	}
	private void HandleItemDropEvent(InventorySlotUi draggedSlotUi)
	{
		if (draggedSlotUi == this) return;

		if (draggedSlotUi.IsInventorySlot() && IsInventorySlot())
			itemContainer.MoveItemToSlot(draggedSlotUi.slotIndex, slotIndex);

		else if (draggedSlotUi.IsEquipmentSlot() && IsEquipmentSlot())
			equipment.EquipItemFromEquipment(draggedSlotUi.equipmentType, equipmentType);

		else if (draggedSlotUi.IsInventorySlot() && IsEquipmentSlot())
			equipment.EquipItemFromInventory(draggedSlotUi.slotIndex, equipmentType);

		else if (draggedSlotUi.IsEquipmentSlot() && IsInventorySlot())
			draggedSlotUi.equipment.EquipItemFromInventory(slotIndex, draggedSlotUi.equipmentType);

		else
			Debug.LogError("drag and drop action not supported");
	}
	#endregion

	#region i pointer click event listener
	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Right && slotItem != null)
			OnToggleInventoryContextMenu?.Invoke(this, eventData.position);
	}
	#endregion

	#region equipment slot listeners
	private void OnEquippedItemChanges(EquipmentSlot slot, bool wasEquipped)
	{
		if (equipment == null) return;
		if (equipmentType != slot.EquipmentType) return; //not correct equipment slot type
		UpdateSlotUi(slot.Item);
	}
	private void OnConsumableUsed(EquipmentSlot slot)
	{
		OnEquippedItemChanges(slot, true);
	}
	#endregion

	#region inventory listener
	private void HandleItemChanges(int slot, InventoryItem item)
	{
		if (itemContainer == null) return; //doesnt have inventory to represent
		if (slotIndex != slot) return; //not correct slot

		UpdateSlotUi(item);
	}
	#endregion

	#region updating ui elements
	private void UpdateSlotUi(InventoryItem item)
	{
		if (item != null && !item.ItemDefinitionNull)
		{
			//while no ui icons for items just change colour to green, when they do uncomment warning log
			//itemInventoryIcon = item.ItemDefinition.ItemUiIcon;

			slotItem = item;
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
			slotItem = null;
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

		OnToggleInventoryContextMenu?.Invoke(null, new(0, 0)); //disable on inventory changes
	}
	#endregion

	#region slot checks
	public bool IsInventorySlot() => itemContainer != null;
	public bool IsEquipmentSlot() => equipment != null;
	#endregion
}
