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
			ShowContextPanel(slot);
		else
			HideContextPanel();
	}
	private void ShowContextPanel(InventorySlotUi slot)
	{
		//buttons needed, equip/unequip as max 3, split stack as 1 (stack count > 1), drop 1x as 1, drop stack as 1 (stack count > 1)

		if (slot.IsEquipmentSlot())
		{
			SetUpUnEquipContext(slot);
		}
		else if (slot.IsInventorySlot())
		{
			SetUpEquipContext(slot);
			SetupSplitContext(slot);
		}

		SetupDropContext(slot);

		contextUiPanel.SetActive(true);
	}
	private void HideContextPanel()
	{
		contextUiPanel.SetActive(false);
	}
	#endregion

	#region set up equip item context menu buttons
	private void SetUpEquipContext(InventorySlotUi slot)
	{
		//show button to equip items to slot (include multiple options for items with them)
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

			if (item.AllowedSlots.HasFlag(slotCategory))
				yield return equipmentType;
		}
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
		
	}
	private void UnEquipItem(InventorySlotUi slot)
	{

	}
	private void SplitItem(InventorySlotUi slot)
	{

	}
	private void DropItem(InventorySlotUi slot, bool dropStack)
	{

	}
	#endregion
}
