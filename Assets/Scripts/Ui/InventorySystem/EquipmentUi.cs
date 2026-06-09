using System.Collections;
using UnityEngine;
using static EquipmentHandler;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Canvas))]
public class EquipmentUi : MonoBehaviour, IUiPanel
{
	private bool isPlayerOwned; //if true, will only listen to player inventory events

    #region equipment ui
    [Header("Equipment Ui")]
    private Canvas canvas;
	public GameObject equipmentUiPanel;
    private RectTransform equipmentUiRectTransform;
    public GameObject quickSlotsUiPanel; //could hide them and only show them when using consumable for 5 seconds + when also showing equipment
	#endregion

	public float quickSlotHideDelay = 5f;
	private Coroutine currentCoroutine;

	#region equipment ui slots
	[Header("Equipment Slots")]
	public InventorySlotUi weaponOneSlot;
	public InventorySlotUi weaponTwoSlot;
	public InventorySlotUi meleeWeaponSlot;
	public InventorySlotUi helmetSlot;
	public InventorySlotUi chestSlot;
	public InventorySlotUi backpackSlot;

	public InventorySlotUi quickSlotOne;
	public InventorySlotUi quickSlotTwo;
	public InventorySlotUi quickSlotThree;
    #endregion

    /// <summary>
    /// for player equipment could simply be grabbed from a GameManager or similar
    /// npcs probably dont need one unless u want to be able to exchange or give npcs equipment 
    /// </summary>
    #region equipment ref
    [Header("Runtime Ref")]
    [SerializeField] private GameObject objectRef;
    [SerializeField] private EquipmentHandler equipmentHandler;
    [SerializeField] private ItemContainer itemContainer;
    #endregion

    private void Start()
	{
        canvas = GetComponent<Canvas>();
        equipmentUiRectTransform = equipmentUiPanel.GetComponent<RectTransform>();
    }

    #region show/hide equipment (should listen out for player input events + when opening other ui elements except pause screen)
    public void ShowUi(UiContext uiContext)
	{
		equipmentUiPanel.SetActive(true);
        ShowQuickSlots(false);
	}
	public void HideUi()
	{
        equipmentUiPanel.SetActive(false);
        HideQuickSlots();
	}
    #endregion

    #region Update references + Slots from UiContext
    public void UpdateObjectReferences(bool playerOwned, GameObject obj, EquipmentHandler equipment, ItemContainer container)
    {
        isPlayerOwned = playerOwned;
        objectRef = obj;
        equipmentHandler = equipment;
        itemContainer = container;

        weaponOneSlot.InitializeSlotUi(canvas, isPlayerOwned);
        weaponTwoSlot.InitializeSlotUi(canvas, isPlayerOwned);
        meleeWeaponSlot.InitializeSlotUi(canvas, isPlayerOwned);

        helmetSlot.InitializeSlotUi(canvas, isPlayerOwned);
        chestSlot.InitializeSlotUi(canvas, isPlayerOwned);
        backpackSlot.InitializeSlotUi(canvas, isPlayerOwned);

        quickSlotOne.InitializeSlotUi(canvas, isPlayerOwned);
        quickSlotTwo.InitializeSlotUi(canvas, isPlayerOwned);
        quickSlotThree.InitializeSlotUi(canvas, isPlayerOwned);

        weaponOneSlot.EnableEquipmentSlot(objectRef, equipmentHandler, itemContainer, EquipmentType.weaponOne);
        weaponTwoSlot.EnableEquipmentSlot(objectRef, equipmentHandler, itemContainer, EquipmentType.weaponTwo);
        meleeWeaponSlot.EnableEquipmentSlot(objectRef, equipmentHandler, itemContainer, EquipmentType.weaponMelee);

        helmetSlot.EnableEquipmentSlot(objectRef, equipmentHandler, itemContainer, EquipmentType.helmet);
        chestSlot.EnableEquipmentSlot(objectRef, equipmentHandler, itemContainer, EquipmentType.chest);
        backpackSlot.EnableEquipmentSlot(objectRef, equipmentHandler, itemContainer, EquipmentType.backpack);

        quickSlotOne.EnableEquipmentSlot(objectRef, equipmentHandler, itemContainer, EquipmentType.consumableOne);
        quickSlotTwo.EnableEquipmentSlot(objectRef, equipmentHandler, itemContainer, EquipmentType.consumableTwo);
        quickSlotThree.EnableEquipmentSlot(objectRef, equipmentHandler, itemContainer, EquipmentType.consumableThree);
    }
    #endregion

    #region show/hide quickSlots + toggle to auto hide (using consumable shows bar for 5 seconds)
    private void ShowQuickSlots(bool autoHide)
	{
		quickSlotsUiPanel.SetActive(true);

		if (autoHide)
		{
			if (currentCoroutine != null)
			{
				StopCoroutine(currentCoroutine);
				currentCoroutine = null;
			}
			currentCoroutine = StartCoroutine(DelayQuickSlotHide());
		}
	}
    private IEnumerator DelayQuickSlotHide()
    {
        yield return new WaitForSeconds(quickSlotHideDelay);
        HideQuickSlots();
    }
    private void HideQuickSlots()
	{
		quickSlotsUiPanel.SetActive(false);
	}
	#endregion
}
