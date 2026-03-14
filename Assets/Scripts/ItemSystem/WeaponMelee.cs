using Game.Core;
using Game.MyNPC;
using Game.MyPlayer;
using UnityEngine;

public class WeaponMelee : Item<WeaponMeleeDefinition>
{
	[SerializeField] private WeaponMeleeDefinition weaponDefinition;

	public WeaponMeleeDefinition WeaponDefinition => weaponDefinition;

	public MeleeWeaponView WeaponView { get; private set; }

	private bool CanSwing => swingCooldownTimer <= 0;
	private bool CurrentlySwinging;

	public float swingTimer;
	public float swingCooldownTimer;

	public override void InitializeItem(WeaponMeleeDefinition definition, int itemStack)
	{
		base.InitializeItem(definition, itemStack);
		weaponDefinition = definition;

		WeaponView = GetComponentInChildren<MeleeWeaponView>();
		WeaponView.DisableHitCollider();

		CurrentlySwinging = false;
		swingTimer = 0;
		swingCooldownTimer = 0;
	}

	private void Update()
	{
		HandleSwingCooldownTimer();
		HandleSwingTimer();
	}

	public void EquipWeapon()
	{
		//hold in hands
	}

	public void UnEquipWeapon()
	{
		//holster
	}

	public void LightAttack()
	{
		if (!CanSwing) return;
		if (CurrentlySwinging) return;

		CurrentlySwinging = true;
		WeaponView.EnableHitCollider(this);

		swingTimer = WeaponDefinition.LightSwingSpeed;
		swingCooldownTimer = swingTimer + WeaponDefinition.LightSwingCooldown;

		///<summery>
		/// swing weapon, if something gets hit damage it and disable hit collider
		/// create relevent sfx and vfx
		///<summery>
	}
	public void HeavyAttack()
	{
		if (!CanSwing) return;
		if (CurrentlySwinging) return;

		CurrentlySwinging = true;
		WeaponView.EnableHitCollider(this);

		swingTimer = WeaponDefinition.HeavySwingSpeed;
		swingCooldownTimer = swingTimer + WeaponDefinition.HeavySwingCooldown;
	}

	public void OnColliderHit(Collider other)
	{
		if (other.TryGetComponent(out IDamageable damageable))
		{
			damageable.RecieveDamage(WeaponDefinition.Damage);
			WeaponView.DisableHitCollider(); //disable hitting once something to hit is found
		}
	}

	#region handle swing timer and auto disable collider if nothing hit
	private void HandleSwingTimer()
	{
		if (swingCooldownTimer > 0)
		{
			swingCooldownTimer -= Time.deltaTime;
			if (swingCooldownTimer <= 0)
				WeaponView.DisableHitCollider();
		}
		if (swingCooldownTimer <= 0f) return;
	}
	#endregion

	#region handle swing cooldown timer
	private void HandleSwingCooldownTimer()
	{
		if (CanSwing) return;

		swingCooldownTimer -= Time.deltaTime;
		if (swingCooldownTimer > 0f) return;
	}
	#endregion
}
