using JetBrains.Annotations;
using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static EquipmentHandler;
using static UiManager;

public class PlayerHudUi : MonoBehaviour, IUiPanel
{
    public GameObject hudUi;

    [Header("Stats Info Ui")]
    public ProgressBarUi healthStatUi;
    public ProgressBarUi staminaStatUi;
    public ProgressBarUi waterStatUi;
    public ProgressBarUi foodStatUi;

    #region stat info property getters
    private int health => player.StatsHandler.health;
    private int stamina => player.StatsHandler.stamina;
    private int water => player.StatsHandler.water;
    private int food => player.StatsHandler.food;

    private int maxHealth => player.Definition.MaxHealth;
    private int maxStamina => player.Definition.MaxStamina;
    private int maxWater => player.Definition.MaxWater;
    private int maxFood => player.Definition.MaxFood;

    private float healthPercentage => Mathf.Clamp01((float)health / maxHealth);
    private float staminaPercentage => Mathf.Clamp01((float)stamina / maxStamina);
    private float waterPercentage => Mathf.Clamp01((float)water / maxWater);
    private float foodPercentage => Mathf.Clamp01((float)food / maxFood);
    #endregion

    [Header("Weapon Info Ui")]
    public GameObject weaponInfoUi;
    public TMP_Text weaponfiremodeText;
    public TMP_Text weaponMagCounterText;
    public TMP_Text weaponReserveCounterText;

    public RangedWeaponItem equippedWeapon;

    [Header("Weapon Reticle Ui")]
    public GameObject weaponReticleUi;
    public RectTransform downReticleBar;
    public RectTransform rightReticleBar;
    public RectTransform upReticleBar;
    public RectTransform leftReticleBar;

    [Header("Hotbar Slots Ui")]
    public GameObject hotbarSlotsUi;

    private readonly float hotbarHideDelay = 3f;
    private Coroutine hotbarHideCoroutine;
    public List<InventorySlotUi> hotbarSlotUis = new();
    public int previousHotbarPressed;

    [Header("Interact Popup Ui")]
    public GameObject interactPopupUi;
    public TMP_Text interactPopupText;

    #region runtime refs
    [Header("Runtime Ref")]
    [SerializeField] private GameObject objectRef;
    [SerializeField] private PlayerController player;
    [SerializeField] private EquipmentHandler playerEquipment;
    [SerializeField] private ItemContainer playerInventory;
    #endregion

    #region Initialize Ui + Button Listeners
    private void InitializeUi(UiContext uiContext)
    {
        HideInteractPopup();
        HideHotbarSlots();

        if (player != null)
        {
            playerEquipment.OnItemInHandsChange -= ToggleWeaponInfo;
            player.Interactor.OnInteractChanged -= OnPlayerInteractableChanged;
            player.Interactor.OnInteractCompleted -= UpdateInteractableText;
        }

        objectRef = uiContext.playerRef;
        player = objectRef.GetComponent<PlayerController>();
        playerEquipment = uiContext.playerEquipment;
        playerInventory = uiContext.playerContainer;

        playerEquipment.OnItemInHandsChange += ToggleWeaponInfo;
        player.Interactor.OnInteractChanged += OnPlayerInteractableChanged;
        player.Interactor.OnInteractCompleted += UpdateInteractableText;

        foreach (InventorySlotUi slotUi in hotbarSlotUis)
            slotUi.InitializeSlotUi(false, true);

        hotbarSlotUis[0].EnableEquipmentSlot(objectRef, playerEquipment, playerInventory, EquipmentType.weaponOne);
        hotbarSlotUis[1].EnableEquipmentSlot(objectRef, playerEquipment, playerInventory, EquipmentType.weaponTwo);
        hotbarSlotUis[2].EnableEquipmentSlot(objectRef, playerEquipment, playerInventory, EquipmentType.weaponMelee);

        hotbarSlotUis[3].EnableEquipmentSlot(objectRef, playerEquipment, playerInventory, EquipmentType.consumableOne);
        hotbarSlotUis[4].EnableEquipmentSlot(objectRef, playerEquipment, playerInventory, EquipmentType.consumableTwo);
        hotbarSlotUis[5].EnableEquipmentSlot(objectRef, playerEquipment, playerInventory, EquipmentType.consumableThree);

        UpdateStats();
    }
    #endregion

    private void Update()
    {
        if (player != null)
            UpdateStats();

        if (InputManager.Instance.AnyHotbarPressed(out int hotbarPressed))
        {
            ShowHotbarSlots();
            hotbarSlotUis[previousHotbarPressed].StopFlashingSlot();
            hotbarSlotUis[hotbarPressed].StartFlashingSlot();
            previousHotbarPressed = hotbarPressed;
        }
    }

    #region Update Stats
    private void UpdateStats()
    {
        healthStatUi.UpdateBarPercentage(ProgressBarUi.ScaleAxis.x, $"{health}/{maxHealth}", healthPercentage);
        staminaStatUi.UpdateBarPercentage(ProgressBarUi.ScaleAxis.x, $"{stamina}/{maxStamina}", staminaPercentage);
        waterStatUi.UpdateBarPercentage(ProgressBarUi.ScaleAxis.x, $"{water}/{maxWater}", waterPercentage);
        foodStatUi.UpdateBarPercentage(ProgressBarUi.ScaleAxis.x, $"{food}/{maxFood}", foodPercentage);
    }
    #endregion

    #region Show/Hide Weapon Info + Sub/Unsub To Events
    private void ToggleWeaponInfo(Item itemInHands)
    {
        if (itemInHands != null && itemInHands is RangedWeaponItem rangedWeapon)
        {
            equippedWeapon = rangedWeapon;
            SubToWeaponEvents();
            UpdateFireMode(rangedWeapon.CurrentFireMode);
            UpdateMagCounter(rangedWeapon.currentMagazineAmmo);
            UpdateReserveAmmoCounter();
            weaponInfoUi.SetActive(true);
        }
        else
        {
            UnsubToWeaponEvents();
            weaponInfoUi.SetActive(false);
        }
    }
    private void SubToWeaponEvents()
    {
        playerInventory.OnAmmoCountsChange += UpdateReserveAmmoCounter;
        equippedWeapon.OnMagazineCountChange += UpdateMagCounter;
        equippedWeapon.OnFireModeChange += UpdateFireMode;
        equippedWeapon.OnAccuracyModifierChange += UpdateReticleUi;
        equippedWeapon.OnReloadTimeRemaining += OnReloadWeaponEvents;
    }
    private void UnsubToWeaponEvents()
    {
        if (equippedWeapon == null) return;

        playerInventory.OnAmmoCountsChange -= UpdateReserveAmmoCounter;
        equippedWeapon.OnMagazineCountChange -= UpdateMagCounter;
        equippedWeapon.OnFireModeChange -= UpdateFireMode;
        equippedWeapon.OnAccuracyModifierChange -= UpdateReticleUi;
        equippedWeapon.OnReloadTimeRemaining -= OnReloadWeaponEvents;
    }
    #endregion

    #region Update Weapon Info Ui Elements
    private void UpdateFireMode(WeaponRangedDefinition.FireModeType fireModeType)
    {
        switch (fireModeType)
        {
            case WeaponRangedDefinition.FireModeType.pumpAction:
                weaponfiremodeText.text = "Pump Action";
                break;
            case WeaponRangedDefinition.FireModeType.fullAuto:
                weaponfiremodeText.text = "Full Auto";
                break;
            case WeaponRangedDefinition.FireModeType.semiAuto:
                weaponfiremodeText.text = "Semi Auto";
                break;
            case WeaponRangedDefinition.FireModeType.boltAction:
                weaponfiremodeText.text = "Bolt Action";
                break;
        }
    }

    private void OnReloadWeaponEvents(float timeRemaning)
    {
        if (timeRemaning == -1) //reload start
        {
            weaponfiremodeText.text = "Reloading";
            weaponMagCounterText.text = $"{timeRemaning}s";
        }
        else if (timeRemaning == 0) //reload end
        {
            UpdateFireMode(equippedWeapon.CurrentFireMode);
            weaponMagCounterText.text = $"{timeRemaning}s";
        }
        else
            weaponMagCounterText.text = $"{timeRemaning}s";
    }

    private void UpdateMagCounter(int magazineCount)
    {
        weaponMagCounterText.text = $"{magazineCount} /";
    }
    private void UpdateReserveAmmoCounter()
    {
        foreach (var kvp in playerInventory.AmmoCounts)
        {
            if (kvp.Key != equippedWeapon.TypedDefinition.AmmoType) continue;

            weaponReserveCounterText.text = $"{kvp.Value}";
            return;
        }
        weaponReserveCounterText.text = $"0";
    }
    #endregion

    #region Update Weapon Reticle
    private void UpdateReticleUi(float accuracyModfier)
    {
        downReticleBar.anchoredPosition = new Vector2(0, -accuracyModfier * 1000);
        rightReticleBar.anchoredPosition = new Vector2(accuracyModfier * 1000, 0);
        upReticleBar.anchoredPosition = new Vector2(0, accuracyModfier * 1000);
        leftReticleBar.anchoredPosition = new Vector2(-accuracyModfier * 1000, 0);
    }
    #endregion

    #region Show/Hide Hotbar Slots + toggle to auto hide (pressing hotbar key input shows bar for x seconds)
    private void ShowHotbarSlots(bool autoHide = true)
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
        HideHotbarSlots();
    }
    private void HideHotbarSlots()
    {
        hotbarSlotsUi.SetActive(false);
    }
    #endregion

    #region Show/Hide Interact Popup + Update Ui Context
    private void OnPlayerInteractableChanged(IInteractable interactable)
    {
        if (interactable == null)
            HideInteractPopup();
        else
        {
            UpdateInteractableText(interactable);
            ShowInteractPopup();
        }
    }
    private void UpdateInteractableText(IInteractable interactable)
    {
        interactPopupText.text = $"{InputManager.Instance.interactAction.GetBindingDisplayString()} {interactable.InteractableName}";
    }
    private void ShowInteractPopup()
    {
        interactPopupUi.SetActive(true);
    }
    private void HideInteractPopup()
    {
        HideTopScreen();
        interactPopupUi.SetActive(false);
    }
    #endregion

    #region Show/Hide Ui Api
    public void ShowUi(UiContext uiContext)
    {
        InitializeUi(uiContext);
        hudUi.SetActive(true);
    }
    public void HideUi()
    {
        hudUi.SetActive(false);
    }
    #endregion
}
