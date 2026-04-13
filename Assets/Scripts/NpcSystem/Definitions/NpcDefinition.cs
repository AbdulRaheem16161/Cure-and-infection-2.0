using System;
using UnityEngine;

public class EntityDefinition : ScriptableObject
{
	#region npc info
	[Header("Entity Info")]
	[SerializeField] private string entityName;
	[SerializeField] private bool isPlayer;

	[SerializeField] private LifeState startingLifeState;
	public enum LifeState
	{
		alive, dead, zombified
	}

	[SerializeField] private Capability capabilities;
	[Flags]
	public enum Capability
	{
		none = 0,
		stunnable = 1 << 0,
		heal = 1 << 1,
		flee = 1 << 2,
		rangedAttack = 1 << 3,
		meleeAttack = 1 << 4,
		chase = 1 << 5,
		eatCorpse = 1 << 6,
		investigate = 1 << 7,
		drink = 1 << 8,
		eat = 1 << 9,
		move = 1 << 10
	};

	[SerializeField] private EntityFlags entityFlags;
	[Flags]
	public enum EntityFlags
	{
		none = 0, canBecomeZombie = 1 << 0
	}
	#endregion

	#region definition prefab to use
	[Header("Definition Prefab To Use")]
	public GameObject gameObjectPrefab;
	#endregion

	#region Npc Stats
	[Header("Npc Stats")]
	[SerializeField] private int maxHealth = 100;
	[SerializeField] private int maxWater = 100;
	[SerializeField] private int maxFood = 100;
	[SerializeField] private int maxStamina = 100;
	#endregion

	#region Npc Movement Behaviour
	[Header("Npc Movement Behaviour")]
	[SerializeField] private float walkSpeed = 3;
	[SerializeField] private float sprintSpeed = 5;
	[SerializeField] private float rotationSpeed = 240;
	[SerializeField] private float fleeDistance = 20;
	[SerializeField] private float minIdleTime = 1;
	[SerializeField] private float maxIdleTime = 3;
	#endregion

	#region NPC Perception Settings
	[Header("NPC Perception Settings")]
	[SerializeField] private float viewAngle = 120f;
	[SerializeField] private float viewDistance = 30f;
	[SerializeField] private float highAlertDuration = 10f;
	[SerializeField] private float viewAngleMultiplier = 1.5f;
	[SerializeField] private float viewDistanceMultiplier = 1.5f;
	#endregion

	#region npc sound detection
	[Header("NPC Sound Detection")]
	[SerializeField] private float soundSensitivity; //percentage chance divided by distance to sound source or something
	#endregion

	#region read only
	public string Name => entityName;
	public bool Player => isPlayer;
	public LifeState StartingLifeState => startingLifeState;
	public Capability Capabilities => capabilities;
	public EntityFlags Flags => entityFlags;

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
	#endregion
}
