using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UiManager;

public class PlayerHudUi : MonoBehaviour, IUiPanel
{
    public GameObject hudUi;

    private EquipmentHandler playerEquipment;
    private ItemContainer playerInventory;

    public GameObject hotbarSlotsUi;

    private readonly float hotbarHideDelay = 5f;
    private Coroutine hotbarHideCoroutine;
    public List<InventorySlotUi> hotbarSlotUis = new();

    #region Initialize Ui + Button Listeners
    private void Start()
    {
        InitializeUi();
    }

    private void InitializeUi()
    {

    }
    #endregion

    #region Show/Hide Hotbar Slots + toggle to auto hide (pressing hotbar key input shows bar for x seconds)
    public void ShowHotbarSlots(bool autoHide)
    {
        hotbarSlotsUi.SetActive(true);

        if (autoHide)
        {
            if (hotbarHideCoroutine != null)
            {
                StopCoroutine(hotbarHideCoroutine);
                hotbarHideCoroutine = null;
            }
            hotbarHideCoroutine = StartCoroutine(DelayHotbarHide());
        }
    }
    private IEnumerator DelayHotbarHide()
    {
        yield return new WaitForSeconds(hotbarHideDelay);
        hotbarSlotsUi.SetActive(false);
    }
    public void HideDelayHotbar()
    {
        hotbarSlotsUi.SetActive(false);
    }
    #endregion

    #region Show/Hide Ui Api
    public void ShowUi(UiContext uiContext)
    {
        playerEquipment = uiContext.playerEquipment;
        playerInventory = uiContext.playerContainer;
        hudUi.SetActive(true);
    }
    public void HideUi()
    {
        hudUi.SetActive(false);
    }
    #endregion
}
