using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Canvas))]
public class InventoryUi : MonoBehaviour
{
    public bool isPlayerInventory; //if true, will only listen to player inventory events

    #region inventory ui
    [Header("Inventory Ui")]
    private Canvas canvas;
    public GameObject inventoryUiPanel;
	private RectTransform inventoryUiRectTransform;
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
        canvas = GetComponent<Canvas>();
        inventoryUiRectTransform = inventoryUiPanel.GetComponent<RectTransform>();

        for (int i = 0; i < inventorySlotUis.Length; i++)
            inventorySlotUis[i].InitializeSlotUi(canvas.gameObject, isPlayerInventory);

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

        if (objectRef.TryGetComponent(out ILootContainer lootContainer))
            itemContainer = lootContainer.ItemContainer;
        else
            { Debug.LogError($"Passed {objectRef} doesnt have {typeof(ILootContainer)} interface"); return; }

        if (objectRef.TryGetComponent(out EquipmentHandler equipmentHandler))
            equipment = equipmentHandler;

        itemContainer.OnContainerSizeChanged += OnInventorySizeChange;
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
    private void OnPlayerInventoryVisible(bool isVisible, bool centerUi)
    {
        if (objectRef != TestInventoryManager.Instance.playerObj)
            return;

        SetUiAnchorPosition(centerUi, true, equipment != null);

        if (isVisible) ShowInventory();
        else HideInventory();
    }
    private void OnLootableInventoryVisible(GameObject lootable, bool isVisible)
    {
        if (objectRef == TestInventoryManager.Instance.playerObj)
            return;

        SetUiAnchorPosition(false, false, equipment != null);
        UpdateObjectReferences(lootable);

        if (isVisible) ShowInventory();
        else  HideInventory();
    }
    #endregion

    #region show/hide inventory (TODO link to and listen out for player input events + when opening other ui elements except pause screen)
    public void ShowInventory()
	{
        inventoryUiPanel.SetActive(true);
	}
	public void HideInventory()
	{
        inventoryUiPanel.SetActive(false);
	}
    private void SetUiAnchorPosition(bool centerUi, bool pushLeft, bool equipmentExists)
    {
        Debug.LogError($"Inventory is Player: {isPlayerInventory} | centerUI: {centerUi} | pushLeft: {pushLeft}");

        if (centerUi)
            inventoryUiRectTransform.anchoredPosition = new Vector2(250, 0);
        else
        {
            float offset = equipmentExists ? 300 : 720;
            inventoryUiRectTransform.anchoredPosition = pushLeft ? new(-offset, 0) : new(offset, 0);
        }
    }
    #endregion
}
