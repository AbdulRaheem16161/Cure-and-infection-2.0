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
	#endregion

	#region inventory refs (passed from InventoryUi, should hopefully be reusable for something like ShopUi as it uses an inventory)
	[SerializeField] private InventoryHandler inventoryToRepresent;
	#endregion

	private void Awake()
	{
		slotIndex = transform.GetSiblingIndex();
		UpdateSlotUi(null);
	}

	private void OnDestroy()
	{
		if (inventoryToRepresent != null)
			inventoryToRepresent.OnInventoryItemChanged -= HandleItemChanges;
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

	private void HandleItemChanges(int slot, InventoryItem item)
	{
		if (inventoryToRepresent == null) return; //doesnt have inventory to represent
		if (slotIndex != slot) return; //not correct slot

		UpdateSlotUi(item);
	}

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
}
