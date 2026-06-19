using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static EquipmentHandler;

public class InventorySlotUi : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    private static WaitForSeconds _waitForSeconds0_25 = new(0.25f);
    private static WaitForSeconds _waitForSeconds0_75 = new(0.75f);

    public bool IsPlayerOwnedSlot { get; private set; } //set from inventory/equipment ui when initializing slot

    #region inventory slot ui
    [Header("Inventory Slot Ui")]
	public GameObject inventorySlotUi;

    public Image flashInventoryIcon;
    private Coroutine flashCouroutine;
	private Color black = new(0.1960784f, 0.1960784f, 0.1960784f);
	private Color white = new(0.7843137f, 0.7843137f, 0.7843137f);

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
    [SerializeField] private GameObject draggableUiParent;
    private int slotIndex;

	public bool Interactable { get; private set; }
    public bool EquipmentSlot { get; private set; }
    private bool canBeDragged;
    private bool isBeingDragged;

    [SerializeField] private GameObject objectRef;
	[SerializeField] private EquipmentHandler equipment;
	[SerializeField] private ItemContainer itemContainer;
	[SerializeField] private InventoryItem slotItem;
	private EquipmentType equipmentType;
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

	#region Initialize Slot (called from parent)
	public void InitializeSlotUi(bool interactable, bool isPlayerOwnedSlot)
	{
		Interactable = interactable;
		IsPlayerOwnedSlot = isPlayerOwnedSlot;
        draggableUiParent = UiManager.Instance.gameObject;
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
    #endregion

    #region enable/disable equipment slot
    public void EnableEquipmentSlot(GameObject objectRef, EquipmentHandler equipment, ItemContainer itemContainer, EquipmentType equipmentType)
	{
		EquipmentSlot = true;

		if (equipment == null) return;

		this.objectRef = objectRef;
        this.equipment = equipment;
		this.itemContainer = itemContainer;
        this.equipmentType = equipmentType;

        equipment.OnEquippedItemChanges += OnEquippedItemChanges;
		equipment.OnConsumableUsed += OnConsumableUsed;

        UpdateSlotUi(equipment.GetEquipmentSlot(equipmentType).Item);
        inventorySlotUi.SetActive(true);
	}
	public void DisableEquipmentSlot()
	{
		inventorySlotUi.SetActive(false);

		if (equipment == null) return; //unsub to events

        equipment.OnEquippedItemChanges -= OnEquippedItemChanges;
        equipment.OnConsumableUsed -= OnConsumableUsed;

        UpdateSlotUi(null);
        equipment = null;
		itemContainer = null;
    }
	#endregion

	#region enable/disable inventory slot
	public void EnableSlot(GameObject objectRef, EquipmentHandler equipment, ItemContainer itemContainer)
	{
        EquipmentSlot = false;

        this.objectRef = objectRef;
        this.equipment = equipment;
        this.itemContainer = itemContainer;
        equipmentType = EquipmentType.none;

        itemContainer.OnContainerItemChanged += HandleItemChanges;

        UpdateSlotUi(itemContainer.Items[slotIndex]);
        inventorySlotUi.SetActive(true);
	}
	public void DisableSlot()
	{
		inventorySlotUi.SetActive(false);

		if (itemContainer != null) //unsub to events
            itemContainer.OnContainerItemChanged -= HandleItemChanges;

		UpdateSlotUi(null);
		equipment = null;
		itemContainer = null;
	}
    #endregion

    #region flash slot ui (atm just used when selecting hotbar item)
	public void StartFlashingSlot()
	{
		StopFlashingSlot();
		flashCouroutine = StartCoroutine(FlashUiIcon());
	}
    public void StopFlashingSlot()
    {
        if (flashCouroutine != null)
            StopCoroutine(flashCouroutine);
    }
    private IEnumerator FlashUiIcon()
	{
		flashInventoryIcon.color = black;
		yield return _waitForSeconds0_25;

		flashInventoryIcon.color = white;
		yield return _waitForSeconds0_75;

        flashInventoryIcon.color = black;
        yield return _waitForSeconds0_25;

        flashInventoryIcon.color = white;
        yield return _waitForSeconds0_75;

        flashInventoryIcon.color = black;
        yield return _waitForSeconds0_25;

        flashInventoryIcon.color = white;
    }
    #endregion

    #region i drag event listeners
    public void OnBeginDrag(PointerEventData eventData)
	{
		if (!Interactable || !canBeDragged) return;
		Debug.LogWarning("begin drag");
		draggableUi.transform.SetParent(draggableUiParent.transform);
		draggableUi.SetActive(true);
		isBeingDragged = true;
	}

	public void OnDrag(PointerEventData eventData)
	{
        if (!Interactable || !canBeDragged) return;
        draggableUi.transform.position = eventData.position;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (!Interactable || !isBeingDragged) return;
		Debug.LogWarning("end drag");
		draggableUi.SetActive(false);
		draggableUi.transform.SetParent(gameObject.transform);
		isBeingDragged = false;
	}
	#endregion

	#region i drop event listener
	public void OnDrop(PointerEventData eventData)
	{
		if (!Interactable) return;

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

		if (!draggedSlotUi.EquipmentSlot && !EquipmentSlot)
			InventoryService.TryResolveSlotInteraction(draggedSlotUi.itemContainer, draggedSlotUi.slotIndex, ItemContainer, slotIndex, true);

		else if (draggedSlotUi.EquipmentSlot && EquipmentSlot)
			InventoryService.TryResolveSlotEquipping(draggedSlotUi.equipment, draggedSlotUi.EquipmentTypes, itemContainer, slotIndex, true);

		else if (!draggedSlotUi.EquipmentSlot && EquipmentSlot)
            InventoryService.TryResolveSlotEquipping(equipment, EquipmentTypes, draggedSlotUi.itemContainer, draggedSlotUi.slotIndex, true);

        else if (draggedSlotUi.EquipmentSlot && !EquipmentSlot)
            InventoryService.TryResolveSlotEquipping(draggedSlotUi.equipment, draggedSlotUi.EquipmentTypes, itemContainer, slotIndex, true);

        else
			Debug.LogError("drag and drop action not supported");
	}
	#endregion

	#region i pointer click event listener
	public void OnPointerClick(PointerEventData eventData)
    {
        if (!Interactable) return;

        if (eventData.button == PointerEventData.InputButton.Right && slotItem != null)
			OnToggleInventoryContextMenu?.Invoke(this, eventData.position);
	}
	#endregion

	#region equipment slot listeners
	private void OnEquippedItemChanges(EquipmentSlot slot, bool wasEquipped)
	{
		if (equipment == null) return;
		if (equipmentType != slot.EquipmentType) return; //not correct equipment slot type

		if (wasEquipped)
			UpdateSlotUi(slot.Item);
		else
            UpdateSlotUi(null);
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
		if (InventoryService.ItemExists(item) && item.CurrentStack != 0)
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
}
