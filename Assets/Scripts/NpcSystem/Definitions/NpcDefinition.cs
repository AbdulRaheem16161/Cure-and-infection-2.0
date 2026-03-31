using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Npc", menuName = "ScriptableObjects/Npc")]
public class NpcDefinition : ScriptableObject
{
	#region npc info
	[Header("Npc Info")]
	[SerializeField] private string npcName;
	[SerializeField] private bool isPlayer;

	[SerializeField] private LifeState startingLifeState;
	public enum LifeState
	{
		alive, dead, zombified
	}

	[SerializeField] private EntityFlags entityFlags;
	[Flags]
	public enum EntityFlags
	{
		none = 0, canRespawn = 1 << 0, canBecomeZombie = 1 << 1
	}

	[SerializeField] private bool supportsEquipmentModels;
	#endregion

	#region definition prefab to use
	[Header("Definition Prefab To Use")]
	public GameObject gameObjectPrefab;
	#endregion

	#region Npc Stats
	[Header("Npc Stats")]
	[SerializeField] private int maxHealth;
	[SerializeField] private int maxWater;
	[SerializeField] private int maxFood;
	[SerializeField] private int maxStamina;
	#endregion

	#region Npc Movement Behaviour
	[Header("Npc Movement Behaviour")]
	[SerializeField] private float walkSpeed;
	[SerializeField] private float sprintSpeed;
	[SerializeField] private float rotationSpeed;
	[SerializeField] private float fleeDistance;
	[SerializeField] private float minIdleTime;
	[SerializeField] private float maxIdleTime;
	#endregion

	#region NPC Perception Settings
	[Header("NPC Perception Settings")]
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

	#region Npc Equipment
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

	#region read only
	public string NpcName => npcName;
	public bool Player => isPlayer;
	public LifeState StartingLifeState => startingLifeState;
	public EntityFlags Flags => entityFlags;
	public bool SupportsEquipmentModels => supportsEquipmentModels;

	public GameObject GameObjectPrefab => gameObjectPrefab;

	public int MaxHealth => maxHealth;
	public int MaxWater => maxWater;
	public int MaxFood => maxFood;
	public int MaxStamina => maxStamina;

	public float WalkSpeed => walkSpeed;
	public float SprintSpeed => sprintSpeed;
	public float RotationSpeed => rotationSpeed;
	public float FleeDistance => fleeDistance;
	public float MinIdleTime => minIdleTime;
	public float MaxIdleTime => maxIdleTime;

	public float ViewAngle => viewAngle;
	public float ViewDistance => viewDistance;
	public float HighAlertDuration => highAlertDuration;
	public float ViewAngleMultiplier => viewAngleMultiplier;
	public float ViewDistanceMultiplier => viewDistanceMultiplier;

	public float SoundSensitivity => soundSensitivity;

	public WeaponRangedDefinition WeaponOne => weaponOne;
	public WeaponRangedDefinition WeaponTwo => weaponTwo;
	public WeaponMeleeDefinition MeleeWeapon => meleeWeapon;

	public ArmourDefinition Helmet => helmet;
	public ArmourDefinition Chest => chest;
	public ArmourDefinition Backpack => backpack;

	public ConsumableDefinition ConsumableOne => consumableOne;
	public ConsumableDefinition ConsumableTwo => consumableTwo;
	public ConsumableDefinition ConsumableThree => consumableThree;
	#endregion
}
