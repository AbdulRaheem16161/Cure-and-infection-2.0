using UnityEngine;

public class InventoryUi : MonoBehaviour
{
	#region inventory ui
	[Header("Inventory Ui")]
	public GameObject inventoryUiPanel;
	public InventorySlotUi[] inventorySlotUis;
	#endregion

	/// <summary>
	/// for player inventory could simply be grabbed from a GameManager or similar
	/// for npc shop inventory, on interact pass interacted obj (npc in this case) and grab InventoryHandler script through some new method
	/// </summary>
	#region inventory ref
	[SerializeField] private InventoryHandler inventory;
	#endregion

	private void Start()
	{
		inventory = TestInventoryManager.Instance.playerObj.GetComponent<InventoryHandler>(); //grab via test manager for now
		inventory.OnInventorySizeChanged += HandleInventorySizeChanges;
		HandleInventorySizeChanges(inventory.InventorySize);
	}
	private void OnDestroy()
	{
		inventory.OnInventorySizeChanged -= HandleInventorySizeChanges;
	}

	#region show/hide inventory (should listen out for player input events + when opening other ui elements except pause screen)
	public void ShowInventory()
	{
		inventoryUiPanel.SetActive(true);
	}
	public void HideInventory()
	{
		inventoryUiPanel.SetActive(false);
	}
	#endregion

	private void HandleInventorySizeChanges(int newSize)
	{
		if (newSize > inventorySlotUis.Length)
		{
			Debug.LogError("New inventroy size bigger then what ui currently supports, resize and add new ui slots " +
				"or edit inventory size + inventory sizes provided by backpacks");
			return;
		}

		for (int i = 0; i < inventorySlotUis.Length; i++)
		{
			if (i < newSize)
				inventorySlotUis[i].EnableSlot(inventory);
			else
				inventorySlotUis[i].DisableSlot();
		}
	}
}
