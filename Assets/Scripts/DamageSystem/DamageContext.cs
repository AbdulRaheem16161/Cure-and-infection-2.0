using UnityEngine;
using static HitCollider;

public class DamageContext
{
	//core info
	public float Damage { get; private set; }
	public BodyPart BodyPartHit { get; private set; }

	//source info
	public GameObject Attacker { get; private set; }

	public DamageContext(float damage, BodyPart bodyPartHit, GameObject attacker)
	{
		Damage = damage;
		BodyPartHit = bodyPartHit;
		Attacker = attacker;
	}

	public void UpdateBodyPartHit(BodyPart bodyPartHit)
	{
		BodyPartHit = bodyPartHit;
	}
}
