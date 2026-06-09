using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Canvas))]
public class InventoryUi : MonoBehaviour, IUiPanel
{
    private bool isPlayerOwned; //if true, will only listen to player inventory events

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
    [SerializeField] private EquipmentHandler equipmentHandler;
    [SerializeField] private ItemContainer itemContainer;
    #endregion

    private void Start()
    {
        canvas = GetComponent<Canvas>();
        inventoryUiRectTransform = inventoryUiPanel.GetComponent<RectTransform>();
    }

    #region show/hide inventory (TODO link to and listen out for player input events + when opening other ui elements except pause screen)
    public void ShowUi(UiContext uiContext)
	{
        inventoryUiPanel.SetActive(true);
	}
	public void HideUi()
	{
        inventoryUiPanel.SetActive(false);
	}
    public void SetUiAnchorPosition(bool equipmentExists)
    {
        inventoryUiRectTransform.anchoredPosition = equipmentExists ? new(300, 0) : new(720, 0);
    }
    #endregion

    #region Update references + Slots from UiContext
    public void UpdateObjectReferences(bool playerOwned, GameObject obj, EquipmentHandler equipment, ItemContainer container)
    {
        itemContainer.OnContainerSizeChanged -= OnInventorySizeChange;

        isPlayerOwned = playerOwned;
        objectRef = obj;
        equipmentHandler = equipment;
        itemContainer = container;

        for (int i = 0; i < inventorySlotUis.Length; i++)
            inventorySlotUis[i].InitializeSlotUi(canvas, isPlayerOwned);

        itemContainer.OnContainerSizeChanged += OnInventorySizeChange;
        OnInventorySizeChange(itemContainer.ContainerSize);
    }
    #endregion

    #region Event Subscriptions
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
                inventorySlotUis[i].EnableSlot(objectRef, equipmentHandler, itemContainer);
            else
                inventorySlotUis[i].DisableSlot();
        }
    }
    #endregion
}
