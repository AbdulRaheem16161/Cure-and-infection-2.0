using UnityEngine;
using static DamageContext;

[RequireComponent(typeof(Collider))]
public class HitCollider : MonoBehaviour
{
	private IDamageable damageable;
	private Collider hitCollider;

	public BodyPart bodyPart;
	public enum BodyPart
	{
		Head, body
	}

	private void Awake()
	{
		damageable = GetComponentInParent<IDamageable>();
		hitCollider = GetComponent<Collider>();

		if (damageable == null)
			Debug.LogError($"no {typeof(IDamageable)} component found in parent object.");

		if (damageable is StatsHandler stats)
		{
			stats.OnInitialize += EnableHitCollider;
			stats.OnDeath += DisableHitCollider;
		}
	}
	private void OnDestroy()
	{
		if (damageable is StatsHandler stats)
		{
			stats.OnInitialize -= EnableHitCollider;
			stats.OnDeath -= DisableHitCollider;
		}
	}

	private void EnableHitCollider()
	{
		hitCollider.enabled = true;
	}
	private void DisableHitCollider()
	{
		hitCollider.enabled = false;
	}

	public void OnHit(float damage, HitImpact impactType, GameObject attacker)
	{
		if (!hitCollider.enabled) return;

		DamageContext damageContext = new(damage, bodyPart, impactType, attacker);
		damageable.RecieveDamage(damageContext);
	}
}
