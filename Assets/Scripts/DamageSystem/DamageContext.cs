using UnityEngine;
using static HitCollider;

public class DamageContext
{
	//core info
	public float Damage { get; private set; }
	public BodyPart BodyPartHit { get; private set; }

	//flinch/stun info
	public HitImpact ImpactType { get; private set; }
	public enum HitImpact
	{
		none, flinch, knockback
	}


	//source info
	public GameObject Attacker { get; private set; }

	public DamageContext(float damage, BodyPart bodyPartHit, HitImpact impactType, GameObject attacker)
	{
		Damage = damage;
		BodyPartHit = bodyPartHit;
		ImpactType = impactType;
		Attacker = attacker;
	}

	public void UpdateBodyPartHit(BodyPart bodyPartHit)
	{
		BodyPartHit = bodyPartHit;
	}
}
