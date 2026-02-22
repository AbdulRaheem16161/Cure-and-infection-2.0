using System.Collections;
using UnityEngine;
using static EquipmentHandler;

public class EquipmentUi : MonoBehaviour
{
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
	[SerializeField] private EquipmentHandler equipment;
	#endregion

	private void Start()
	{
		equipment = TestInventoryManager.Instance.playerObj.GetComponent<EquipmentHandler>(); //grab via test manager for now
		SetUpEquipmentSlots();
	}

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
	public void HideQuickSlots()
	{
		quickSlotsUiPanel.SetActive(false);
	}
	#endregion

	private IEnumerator DelayQuickSlotHide()
	{
		yield return new WaitForSeconds(quickSlotHideDelay);
		HideQuickSlots();
	}

	private void SetUpEquipmentSlots()
	{
		weaponOneSlot.EnableEquipmentSlot(equipment, EquipmentType.weaponOne);
		weaponTwoSlot.EnableEquipmentSlot(equipment, EquipmentType.weaponTwo);
		meleeWeaponSlot.EnableEquipmentSlot(equipment, EquipmentType.weaponMelee);
		helmetSlot.EnableEquipmentSlot(equipment, EquipmentType.helmet);
		chestSlot.EnableEquipmentSlot(equipment, EquipmentType.chest);
		backpackSlot.EnableEquipmentSlot(equipment, EquipmentType.backpack);

		quickSlotOne.EnableEquipmentSlot(equipment, EquipmentType.consumableOne);
		quickSlotTwo.EnableEquipmentSlot(equipment, EquipmentType.consumableTwo);
		quickSlotThree.EnableEquipmentSlot(equipment, EquipmentType.consumableThree);
	}
}
