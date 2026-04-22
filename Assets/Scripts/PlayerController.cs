using UnityEngine;

/// <summary>
/// example script, comment out if needed
/// </summary>

public class PlayerController : MonoBehaviour
{
	private EquipmentHandler EquipmentHandler;
	private InventoryHandler InventoryHandler;

	private string[] HitableTags;

	private void Awake()
	{
		EquipmentHandler = GetComponent<EquipmentHandler>();
		InventoryHandler = GetComponent<InventoryHandler>();
	}

	public void PlayerMouseLeftClick()
	{
		if (EquipmentHandler.itemInHands is RangedWeaponItem weaponRanged)
		{
			weaponRanged.Shoot();
		}
		else if (EquipmentHandler.itemInHands is MeleeWeaponItem weaponMelee)
		{
			weaponMelee.LightAttack();
		}
	}

	/// <summary>
	/// inputKey would be passed in via PlayerInputs for example F to interact is base, but looking at a ranged weapon with multiple destinations
	/// will also give the option of pressing f/e 
	/// </summary>
	public void ItemPickUpInteract<T>(Item<T> item, int inputKey) where T : ItemDefinition
	{
		if (item is RangedWeaponItem ranged)
		{
			if (inputKey == 0) // 0 = input key f for example
				EquipmentHandler.EquipItem(ranged.TypedDefinition, 1, EquipmentHandler.EquipmentType.weaponOne); //equip directly
			else if (inputKey == 1) // 1 = input key e for example
				EquipmentHandler.EquipItem(ranged.TypedDefinition, 1, EquipmentHandler.EquipmentType.weaponTwo); //equip directly
		}
		else if (item is MeleeWeaponItem melee)
		{
			EquipmentHandler.EquipItem(melee.TypedDefinition, 1, EquipmentHandler.EquipmentType.weaponMelee); //equip directly
		}
		else
			item.PickUp(InventoryHandler);
	}

	//holsters weapon on key press if same weapon already equipped. if not unholsters that weapon (internal logic should handle things)

	public void PlayerHotkeyPressOne()
	{
		if (EquipmentHandler.HasItemInHands)
			EquipmentHandler.HolsterWeapon();
		else
			EquipmentHandler.UnholsterWeapon(EquipmentHandler.EquipmentType.weaponOne);
	}

	public void PlayerHotkeyPressTwo()
	{
		if (EquipmentHandler.HasItemInHands)
			EquipmentHandler.HolsterWeapon();
		else
			EquipmentHandler.UnholsterWeapon(EquipmentHandler.EquipmentType.weaponTwo);
	}

	public void PlayerHotkeyPressThree()
	{
		if (EquipmentHandler.HasItemInHands)
			EquipmentHandler.HolsterWeapon();
		else
			EquipmentHandler.UnholsterWeapon(EquipmentHandler.EquipmentType.weaponMelee);
	}

	//use consumables on hotkey presses

	public void PlayerHotkeyPressQ()
	{
		EquipmentHandler.UseConsumable(EquipmentHandler.EquipmentType.consumableOne);
	}
	public void PlayerHotkeyPressE()
	{
		EquipmentHandler.UseConsumable(EquipmentHandler.EquipmentType.consumableTwo);
	}
}
