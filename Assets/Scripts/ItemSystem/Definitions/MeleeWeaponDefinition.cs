using System;
using UnityEngine;
using static DamageContext;

[CreateAssetMenu(fileName = "WeaponMelee", menuName = "ScriptableObjects/Item/WeaponMelee")]
public class WeaponMeleeDefinition : ItemDefinition
{
	#region weapon characteristics
	[Header("Weapon Characteristics")]
	[SerializeField] private int damage;
	[SerializeField] private HitImpact impactType;

	[Header("Swing Behaviour")]
	[Tooltip("How quick the swing is")]
	[SerializeField] private float lightSwingSpeed;
	[Tooltip("How long till you can swing again after swingSpeed")]
	[SerializeField] private float lightSwingCooldown;

	[Tooltip("How quick the swing is")]
	[SerializeField] private float heavySwingSpeed;
	[Tooltip("How long till you can swing again after swingSpeed")]
	[SerializeField] private float heavySwingCooldown;

    [SerializeField] private WeaponFlag weaponFlags;
    [Flags]
    public enum WeaponFlag
    {
        None = 0,
        Improvised = 1 << 0,
        Civilian = 1 << 1,
        Police = 1 << 2,
        Military = 1 << 3,
        Industrial = 1 << 4,
        Sporting = 1 << 5
    }
    #endregion

    #region readonly properties
    public HitImpact ImpactType => impactType;
	public int Damage => damage;

	//swing behaviour
	public float LightSwingSpeed => lightSwingSpeed;
	public float LightSwingCooldown => lightSwingCooldown;

	public float HeavySwingSpeed => heavySwingSpeed;
	public float HeavySwingCooldown => heavySwingCooldown;

    public WeaponFlag WeaponFlags => weaponFlags;
    #endregion
}
