using UnityEngine;

public class InventoryUi : MonoBehaviour
{
    public bool isPlayerInventory; //if true, will only listen to player inventory events

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
	[Header("Runtime Ref")]
    [SerializeField] private GameObject objectRef;
    [SerializeField] private ItemContainer itemContainer;
    [SerializeField] private EquipmentHandler equipment;
    #endregion

    private void Start()
    {
        if (isPlayerInventory)
            UpdateObjectReferences(TestInventoryManager.Instance.playerObj); //grab via test manager for now)

        SubToEvents();
	}
	private void OnDestroy()
	{
        UnSubToEvents();
	}

    public void UpdateObjectReferences(GameObject newRef)
    {
        itemContainer.OnContainerSizeChanged -= OnInventorySizeChange;

        objectRef = newRef;
        itemContainer = objectRef.GetComponent<InventoryHandler>().ItemContainer;
        itemContainer.OnContainerSizeChanged += OnInventorySizeChange;
        equipment = objectRef.GetComponent<EquipmentHandler>();

        OnInventorySizeChange(itemContainer.ContainerSize);
    }

    #region Event Subscriptions
    private void SubToEvents()
    {
        itemContainer.OnContainerSizeChanged += OnInventorySizeChange;
        TestInventoryManager.PlayerInventoryVisibleEvent += OnPlayerInventoryVisible;
        TestInventoryManager.LootableInventoryVisibleEvent += OnLootableInventoryVisible;
    }
    private void UnSubToEvents()
    {
        TestInventoryManager.PlayerInventoryVisibleEvent -= OnPlayerInventoryVisible;
        TestInventoryManager.LootableInventoryVisibleEvent -= OnLootableInventoryVisible;
    }
    private void OnInventorySizeChange(int newSize)
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
                inventorySlotUis[i].EnableSlot(objectRef, itemContainer, equipment);
            else
                inventorySlotUis[i].DisableSlot();
        }
    }
    private void OnPlayerInventoryVisible(bool isVisible)
    {
        if (objectRef != TestInventoryManager.Instance.playerObj)
            return;

        if (isVisible) ShowInventory();
        else HideInventory();
    }
    private void OnLootableInventoryVisible(GameObject lootable, bool isVisible)
    {
        if (objectRef == TestInventoryManager.Instance.playerObj)
            return;

        UpdateObjectReferences(lootable);

        if (isVisible) ShowInventory();
        else  HideInventory();
    }
    #endregion

    #region show/hide inventory (TODO link to and listen out for player input events + when opening other ui elements except pause screen)
    public void ShowInventory()
	{
        Debug.LogError("Show Inventory: true");
        inventoryUiPanel.SetActive(true);
	}
	public void HideInventory()
	{
        Debug.LogError("Show Inventory: false");
        inventoryUiPanel.SetActive(false);
	}
	#endregion
}
