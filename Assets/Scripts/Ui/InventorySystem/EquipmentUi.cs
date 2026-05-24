using System.Collections;
using UnityEngine;
using static EquipmentHandler;

public class EquipmentUi : MonoBehaviour
{
	public bool isPlayerEquipment; //if true, will only listen to player inventory events

    #region equipment ui
    [Header("Equipment Ui")]
	public GameObject equipmentUiPanel;
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
    [SerializeField] private EquipmentHandler equipment;
	#endregion

	private void Start()
	{
		if (isPlayerEquipment)
			UpdateObjectReferences(TestInventoryManager.Instance.playerObj); //grab via test manager for now)

        SubToEvents();
    }
    private void OnDestroy()
    {
		UnSubToEvents();
    }

	public void UpdateObjectReferences(GameObject newRef)
    {
        objectRef = newRef;
        equipment = objectRef.GetComponent<EquipmentHandler>();
		SetUpEquipmentUiSlots();
    }

    private void SetUpEquipmentUiSlots()
    {
        weaponOneSlot.EnableEquipmentSlot(objectRef, equipment, EquipmentType.weaponOne);
        weaponTwoSlot.EnableEquipmentSlot(objectRef, equipment, EquipmentType.weaponTwo);
        meleeWeaponSlot.EnableEquipmentSlot(objectRef, equipment, EquipmentType.weaponMelee);
        helmetSlot.EnableEquipmentSlot(objectRef, equipment, EquipmentType.helmet);
        chestSlot.EnableEquipmentSlot(objectRef, equipment, EquipmentType.chest);
        backpackSlot.EnableEquipmentSlot(objectRef, equipment, EquipmentType.backpack);

        quickSlotOne.EnableEquipmentSlot(objectRef, equipment, EquipmentType.consumableOne);
        quickSlotTwo.EnableEquipmentSlot(objectRef, equipment, EquipmentType.consumableTwo);
        quickSlotThree.EnableEquipmentSlot(objectRef, equipment, EquipmentType.consumableThree);
    }

    #region Event Subscriptions
    private void SubToEvents()
	{
        TestInventoryManager.PlayerInventoryVisibleEvent += OnPlayerInventoryVisible;
		TestInventoryManager.LootableInventoryVisibleEvent += OnLootableInventoryVisible;
    }
	private void UnSubToEvents()
	{
        TestInventoryManager.PlayerInventoryVisibleEvent -= OnPlayerInventoryVisible;
        TestInventoryManager.LootableInventoryVisibleEvent -= OnLootableInventoryVisible;
    }
    private void OnPlayerInventoryVisible(bool isVisible)
    {
        if (objectRef != TestInventoryManager.Instance.playerObj)
            return;

        if (isVisible) ShowEquipment();
        else HideEquipment();
    }
    private void OnLootableInventoryVisible(GameObject lootable, bool isVisible)
    {
        if (objectRef == TestInventoryManager.Instance.playerObj)
            return;

        UpdateObjectReferences(lootable);

        if (isVisible && equipment != null)
            ShowEquipment();
        else
            HideEquipment();
    }
    #endregion

    #region show/hide equipment (should listen out for player input events + when opening other ui elements except pause screen)
    public void ShowEquipment()
	{
		equipmentUiPanel.SetActive(true);
	}
	public void HideEquipment()
	{
        equipmentUiPanel.SetActive(false);
	}
	#endregion

	#region show/hide quickSlots + toggle to auto hide (using consumable shows bar for 5 seconds)
	public void ShowQuickSLots(bool autoHide)
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
    public void HideQuickSlots()
	{
		quickSlotsUiPanel.SetActive(false);
	}
	#endregion
}
