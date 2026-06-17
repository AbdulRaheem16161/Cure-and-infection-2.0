using System.Collections.Generic;
using UnityEngine;
using static EquipmentHandler;

public class EquipmentUi : MonoBehaviour, IUiPanel
{
	private bool isPlayerOwned; //if true, will only listen to player inventory events

    #region equipment ui
    [Header("Equipment Ui")]
	public GameObject equipmentUiPanel;
    public List<InventorySlotUi> equipmentSlotsUi = new();
    #endregion

    #region runtime ref
    [Header("Runtime Ref")]
    [SerializeField] private GameObject objectRef;
    [SerializeField] private EquipmentHandler equipmentHandler;
    [SerializeField] private ItemContainer itemContainer;
    #endregion

    #region show/hide equipment
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

        foreach (InventorySlotUi slotUi in equipmentSlotsUi)
            slotUi.InitializeSlotUi(true, isPlayerOwned);

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
