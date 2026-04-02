using UnityEngine;

public class WeaponMelee : Item<WeaponMeleeDefinition>
{
	public MeleeWeaponView WeaponView { get; private set; }

	private bool CanSwing => swingCooldownTimer <= 0;
	private bool CurrentlySwinging;

	public float swingTimer;
	public float swingCooldownTimer;

	public override void InitializeItem(WeaponMeleeDefinition definition, GameObject itemModel, int itemStack)
	{
		base.InitializeItem(definition, itemModel, itemStack);

		//weapon-specific setup here
		if (ModelReference == null)
		{
			Debug.LogError($"{TypedDefinition.ItemName} lacks a needed (or temporary) model with component: {nameof(MeleeWeaponView)}");
			return;
		}

		WeaponView = ModelReference.GetComponent<MeleeWeaponView>();

		if (WeaponView == null)
		{
			Debug.LogError($"{TypedDefinition.ItemName} missing component {nameof(MeleeWeaponView)} in its ModelReference.\n");
			return;
		}

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

	public void LightAttack()
	{
		if (!CanSwing) return;
		if (CurrentlySwinging) return;

		CurrentlySwinging = true;
		WeaponView.EnableHitCollider(this);

		swingTimer = TypedDefinition.LightSwingSpeed;
		swingCooldownTimer = swingTimer + TypedDefinition.LightSwingCooldown;

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

		swingTimer = TypedDefinition.HeavySwingSpeed;
		swingCooldownTimer = swingTimer + TypedDefinition.HeavySwingCooldown;
	}

	public void OnColliderHit(Collider other)
	{
		if (other.TryGetComponent(out HitCollider hitCollider))
		{
			hitCollider.OnHit(TypedDefinition.Damage, CurrentOwner);
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
