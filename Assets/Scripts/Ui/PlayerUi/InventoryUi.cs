using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class InventoryUi : MonoBehaviour, IUiPanel
{
    private bool isPlayerOwned; //if true, will only listen to player inventory events

    #region inventory ui
    [Header("Inventory Ui")]
    public GameObject inventoryUiPanel;
	private RectTransform inventoryUiRectTransform;
	public InventorySlotUi[] inventorySlotUis;
    #endregion

    #region runtime ref
    [Header("Runtime Ref")]
    [SerializeField] private GameObject objectRef;
    [SerializeField] private EquipmentHandler equipmentHandler;
    [SerializeField] private ItemContainer itemContainer;
    #endregion

    private void Start()
    {
        inventoryUiRectTransform = inventoryUiPanel.GetComponent<RectTransform>();
    }

    #region show/hide inventory
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
    public void UpdateObjectReferences(bool playerOwned, UiContext uiContext)
    {
        itemContainer.OnContainerSizeChanged -= OnInventorySizeChange;

        isPlayerOwned = playerOwned;

        if (isPlayerOwned)
        {
            objectRef = uiContext.playerRef;
            equipmentHandler = uiContext.playerEquipment;
            itemContainer = uiContext.playerContainer;
        }
        else
        {
            objectRef = uiContext.otherRef;
            equipmentHandler = uiContext.otherEquipment;
            itemContainer = uiContext.otherContainer;
        }

        for (int i = 0; i < inventorySlotUis.Length; i++)
            inventorySlotUis[i].InitializeSlotUi(true, isPlayerOwned);

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
