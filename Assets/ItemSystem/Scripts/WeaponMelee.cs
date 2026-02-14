using UnityEngine;

public class WeaponMelee : Item<WeaponMeleeDefinition>
{
	private WeaponMeleeDefinition weaponDefinition;

	private bool CanSwing => swingCooldownTimer <= 0;
	private float swingCooldownTimer;

	public override void InitializeItem(WeaponMeleeDefinition definition)
	{
		base.InitializeItem(definition);
		weaponDefinition = definition;

		// weapon-specific setup here
	}

	public void LightAttack()
	{
		///<summery>
		/// check if can swing, swing weapon
		/// create relevent sfx and vfx
		///<summery>
	}
	public void HeavyAttack()
	{

	}

	public void SwingCooldown()
	{
		//read attackspeed from definition, set swing cooldown to attack speed then tick down to 0
	}
}
