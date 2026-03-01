using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static EquipmentHandler;
using static ItemDefinition;

public class InventoryContextUi : MonoBehaviour
{
	[Header("Context Ui Panel")]
	public GameObject contextUiPanel;

	[Header("Button Actions")]
	public Button[] equipItemButtons;
	public Button unEquipItem;
	public Button splitItem;
	public Button dropItem;
	public Button dropItemStack;

	private bool ContextMenuActive => contextUiPanel.activeInHierarchy;

	#region event sub/unsub
	private void Awake()
	{
		InventorySlotUi.OnToggleInventoryContextMenu += ToggleInventoryContextMenu;
		HideContextPanel();
	}
	private void OnDestroy()
	{
		InventorySlotUi.OnToggleInventoryContextMenu -= ToggleInventoryContextMenu;
	}
	#endregion

	/// <summary>
	/// hide menu on right click if active will need redoing when merging to main and updating to player inputs
	/// </summary>
	private void Update()
	{
		if (ContextMenuActive && Input.GetKeyDown(KeyCode.Mouse1))
			ToggleInventoryContextMenu(null, new(0, 0));
	}

	#region toggle + show/hide context menu
	private void ToggleInventoryContextMenu(InventorySlotUi slot, Vector2 position)
	{
		if (slot != null)
			ShowContextPanel(slot, position);
		else
			HideContextPanel();
	}
	private void ShowContextPanel(InventorySlotUi slot, Vector2 position)
	{
		SetUpEquipContext(slot);
		SetUpUnEquipContext(slot);
		SetupSplitContext(slot);
		SetupDropContext(slot);

		contextUiPanel.SetActive(true);
		SetMenuPosition(position);
	}
	private void HideContextPanel()
	{
		contextUiPanel.SetActive(false);

		foreach (Button button in equipItemButtons)
			button.gameObject.SetActive(false);

		unEquipItem.gameObject.SetActive(false);
		splitItem.gameObject.SetActive(false);
		dropItem.gameObject.SetActive(false);
		dropItemStack.gameObject.SetActive(false);
	}
	#endregion

	#region set context menu position and size
	private void SetMenuPosition(Vector2 position)
	{
		RectTransform rect = contextUiPanel.GetComponent<RectTransform>();
		rect.sizeDelta = SetMenuSize();

		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			rect.parent as RectTransform, position, null, out Vector2 anchoredPos);

		Vector2 offset = new(rect.rect.width / 2f, -rect.rect.height / 2f);
		rect.anchoredPosition = anchoredPos + offset;
	}
	private Vector2 SetMenuSize()
	{
		int baseWidth = 4;
		int baseHeight = 4;
		int buttonWidth = 160;
		int buttonHeight = 45;
		int buttonCount = 0;

		// Count active buttons
		foreach (Button button in equipItemButtons)
			if (button.gameObject.activeInHierarchy)
				buttonCount++;

		if (unEquipItem.gameObject.activeInHierarchy) buttonCount++;
		if (splitItem.gameObject.activeInHierarchy) buttonCount++;
		if (dropItem.gameObject.activeInHierarchy) buttonCount++;
		if (dropItemStack.gameObject.activeInHierarchy) buttonCount++;

		int width = baseWidth + buttonWidth; // width is fixed
		int height = baseHeight + (buttonHeight * buttonCount);

		return new Vector2(width, height);
	}
	#endregion

	#region set up equip item context menu buttons
	private void SetUpEquipContext(InventorySlotUi slot)
	{
		if (slot.IsEquipmentSlot()) return;

		ItemDefinition item = slot.SlotItem.ItemDefinition;
		var validSlots = GetValidEquipmentSlotsForItem(item).ToList();

		for (int i = 0; i < equipItemButtons.Length; i++)
		{
			Button btn = equipItemButtons[i];
			btn.onClick.RemoveAllListeners();

			if (i < validSlots.Count)
			{
				EquipmentType equipmentType = validSlots[i];
				btn.onClick.AddListener(() => EquipItem(slot, equipmentType));
				btn.GetComponentInChildren<TMP_Text>().text = $"Equip To {equipmentType}";
				btn.gameObject.SetActive(true);
			}
			else
				btn.gameObject.SetActive(false);
		}

		if (!validSlots.Any())
			Debug.LogWarning($"No valid equipment slots found for {item.ItemName}");
	}
	#endregion

	#region set up un equip item context menu buttons
	private void SetUpUnEquipContext(InventorySlotUi slot)
	{
		if (slot.IsInventorySlot()) return;

		unEquipItem.onClick.RemoveAllListeners();
		unEquipItem.onClick.AddListener(() => UnEquipItem(slot));
		unEquipItem.GetComponentInChildren<TMP_Text>().text = $"Unequip {slot.SlotItem.ItemDefinition.ItemName}";
		unEquipItem.gameObject.SetActive(true);
	}
	#endregion

	#region set up split item context menu buttons
	private void SetupSplitContext(InventorySlotUi slot)
	{
		splitItem.onClick.RemoveAllListeners();
		bool stackMoreThenOne = slot.SlotItem.CurrentStack > 1;

		if (!stackMoreThenOne)
		{
			splitItem.gameObject.SetActive(false);
			return;
		}

		splitItem.onClick.AddListener(() => SplitItem(slot));
		splitItem.GetComponentInChildren<TMP_Text>().text = $"Split {slot.SlotItem.ItemDefinition.ItemName}";
		splitItem.gameObject.SetActive(true);
	}
	#endregion

	#region set up drop item context menu buttons
	private void SetupDropContext(InventorySlotUi slot)
	{
		dropItem.onClick.RemoveAllListeners();
		dropItem.onClick.AddListener(() => DropItem(slot, false));
		dropItem.GetComponentInChildren<TMP_Text>().text = $"Drop {slot.SlotItem.ItemDefinition.ItemName}";
		dropItem.gameObject.SetActive(true);

		dropItemStack.onClick.RemoveAllListeners();
		bool stackMoreThenOne = slot.SlotItem.CurrentStack > 1;

		if (!stackMoreThenOne)
		{
			dropItemStack.gameObject.SetActive(false);
			return;
		}

		dropItemStack.onClick.AddListener(() => DropItem(slot, true));
		dropItemStack.GetComponentInChildren<TMP_Text>().text = $"Drop all {slot.SlotItem.ItemDefinition.ItemName}";
		dropItemStack.gameObject.SetActive(true);
	}
	#endregion

	#region get valid slots for items
	private IEnumerable<EquipmentType> GetValidEquipmentSlotsForItem(ItemDefinition item)
	{
		foreach (var kvp in slotToInventoryType)
		{
			EquipmentType equipmentType = kvp.Key;
			InventorySlotType slotCategory = kvp.Value;

			if (item.AllowedSlots.HasFlag(slotCategory) && EquipmentTypeMatch(equipmentType, item))
				yield return equipmentType;
		}
	}
	private bool EquipmentTypeMatch(EquipmentType equipmentType, ItemDefinition item)
	{
		return (equipmentType & item.AllowedEquipmentSlots) != 0;
	}
	#endregion

	/// <summary>
	/// while no shop ui or npc shop interaction exists leave for now
	/// </summary>
	public void SetUpBuyContext()
	{

	}
	public void SetUpSellContext()
	{

	}

	#region button actions
	private void EquipItem(InventorySlotUi slot, EquipmentType equipmentType)
	{
		slot.InventoryRef.EquipmentHandler.EquipItemFromInventory(slot.SlotIndex, equipmentType);
		HideContextPanel();
	}
	private void UnEquipItem(InventorySlotUi slot)
	{
		slot.EquipmentRef.UnequipItem(slot.SlotEquipmentType);
		HideContextPanel();
	}
	private void SplitItem(InventorySlotUi slot)
	{
		slot.InventoryRef.SplitItem(slot.SlotIndex);
		HideContextPanel();
	}
	private void DropItem(InventorySlotUi slot, bool dropStack)
	{
		if (slot.EquipmentRef != null)
		{
			slot.EquipmentRef.DropItem(slot.SlotEquipmentType, dropStack);
		}
		else if (slot.InventoryRef != null)
		{
			slot.InventoryRef.DropItem(slot.SlotIndex, dropStack);
		}
		HideContextPanel();
	}
	#endregion
}
