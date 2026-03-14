using UnityEngine;

[CreateAssetMenu(fileName = "Npc", menuName = "ScriptableObjects/Npc")]
public class NpcDefinition : ScriptableObject
{
	#region npc info
	[Header("Npc Info")]
	[SerializeField] private string npcName;
	[SerializeField] private bool isZombie;
	[SerializeField] private bool supportsEquipmentModels;
	#endregion

	#region npc stats
	[Header("Npc Stats")]
	[SerializeField] private int maxHealth;
	[SerializeField] private int maxWater;
	[SerializeField] private int maxFood;
	[SerializeField] private int maxStamina;
	#endregion

	#region npc behaviour stats
	[Header("Npc Behaviour")]
	[SerializeField] private float patrolSpeed;
	[SerializeField] private float chaseSpeed;
	[SerializeField] private float rotationSpeed;
	#endregion

	#region npc equipment
	[Header("Npc Equipment")]
	/// <summary>
	/// for zombie ranged attacks we can create a unique WeaponRangedDefinition ZombieSpit as they should work fine if set up like a single shot gun
	/// for zombie melee attacks we can create a unique WeaponMeleeDefinition that will do the same as above
	/// </summary>
	[SerializeField] private WeaponRangedDefinition weaponOne;
	[SerializeField] private WeaponRangedDefinition weaponTwo;
	[SerializeField] private WeaponMeleeDefinition meleeWeapon;

	[SerializeField] private ArmourDefinition helmet;
	[SerializeField] private ArmourDefinition chest;
	[SerializeField] private ArmourDefinition backpack;

	[SerializeField] private ConsumableDefinition consumableOne;
	[SerializeField] private ConsumableDefinition consumableTwo;
	[SerializeField] private ConsumableDefinition consumableThree;
	#endregion

	#region npc vision detection
	[Header("NPC Vision Detection")]
	[SerializeField] private float viewAngle = 45f;
	[SerializeField] private float viewDistance = 5f;
	[SerializeField] private float highAlertDuration = 5f;
	[SerializeField] private float viewAngleMultiplier = 1.5f;
	[SerializeField] private float viewDistanceMultiplier = 2f;
	#endregion

	#region npc sound detection
	[Header("NPC Sound Detection")]
	[SerializeField] private float soundSensitivity; //percentage chance divided by distance to sound source or something
	#endregion

	#region definition prefab to use
	[Header("Definition Prefab To Use")]
	public GameObject gameObjectPrefab;
	#endregion

	#region read only
	public string NpcName => npcName;
	public bool IsZombie => isZombie;
	public bool SupportsEquipmentModels => supportsEquipmentModels;

	public int MaxHealth => maxHealth;
	public int MaxWater => maxWater;
	public int MaxFood => maxFood;
	public int MaxStamina => maxStamina;

	public float PatrolSpeed => patrolSpeed;
	public float ChaseSpeed => chaseSpeed;
	public float RotationSpeed => rotationSpeed;

	public WeaponRangedDefinition WeaponOne => weaponOne;
	public WeaponRangedDefinition WeaponTwo => weaponTwo;
	public WeaponMeleeDefinition MeleeWeapon => meleeWeapon;

	public ArmourDefinition Helmet => helmet;
	public ArmourDefinition Chest => chest;
	public ArmourDefinition Backpack => backpack;

	public ConsumableDefinition ConsumableOne => consumableOne;
	public ConsumableDefinition ConsumableTwo => consumableTwo;
	public ConsumableDefinition ConsumableThree => consumableThree;

	public float ViewAngle => viewAngle;
	public float ViewDistance => viewDistance;
	public float HighAlertDuration => highAlertDuration;
	public float ViewAngleMultiplier => viewAngleMultiplier;
	public float ViewDistanceMultiplier => viewDistanceMultiplier;

	public float SoundSensitivity => soundSensitivity;

	public GameObject GameObjectPrefab => gameObjectPrefab;
	#endregion
}
