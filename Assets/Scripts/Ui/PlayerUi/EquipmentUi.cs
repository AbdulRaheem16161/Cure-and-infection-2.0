using System.Collections;
using System.Collections.Generic;
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

    public List<InventorySlotUi> equipmentSlotsUi = new();
    #endregion

    #region equipment ref
    [Header("Runtime Ref")]
    [SerializeField] private GameObject objectRef;
    [SerializeField] private EquipmentHandler equipmentHandler;
    [SerializeField] private ItemContainer itemContainer;
    #endregion

    private void Start()
	{
        canvas = GetComponent<Canvas>();
    }

    #region show/hide equipment (should listen out for player input events + when opening other ui elements except pause screen)
    public void ShowUi(UiContext uiContext)
	{
		equipmentUiPanel.SetActive(true);
	}
	public void HideUi()
	{
        equipmentUiPanel.SetActive(false);
	}
    #endregion

    #region Update references + Slots from UiContext
    public void UpdateObjectReferences(bool playerOwned, UiContext uiContext)
    {
        isPlayerOwned = playerOwned;
        isPlayerOwned = playerOwned;
        objectRef = uiContext.playerRef;
        equipmentHandler = uiContext.playerEquipment;
        itemContainer = uiContext.playerContainer;

        foreach (InventorySlotUi slotUi in equipmentSlotsUi)
            slotUi.InitializeSlotUi(canvas, isPlayerOwned);

        equipmentSlotsUi[0].EnableEquipmentSlot(objectRef, equipmentHandler, itemContainer, EquipmentType.weaponOne);
        equipmentSlotsUi[1].EnableEquipmentSlot(objectRef, equipmentHandler, itemContainer, EquipmentType.weaponTwo);
        equipmentSlotsUi[2].EnableEquipmentSlot(objectRef, equipmentHandler, itemContainer, EquipmentType.weaponMelee);

        equipmentSlotsUi[3].EnableEquipmentSlot(objectRef, equipmentHandler, itemContainer, EquipmentType.helmet);
        equipmentSlotsUi[4].EnableEquipmentSlot(objectRef, equipmentHandler, itemContainer, EquipmentType.chest);
        equipmentSlotsUi[5].EnableEquipmentSlot(objectRef, equipmentHandler, itemContainer, EquipmentType.backpack);

        equipmentSlotsUi[6].EnableEquipmentSlot(objectRef, equipmentHandler, itemContainer, EquipmentType.consumableOne);
        equipmentSlotsUi[7].EnableEquipmentSlot(objectRef, equipmentHandler, itemContainer, EquipmentType.consumableTwo);
        equipmentSlotsUi[8].EnableEquipmentSlot(objectRef, equipmentHandler, itemContainer, EquipmentType.consumableThree);
    }
    #endregion
}
