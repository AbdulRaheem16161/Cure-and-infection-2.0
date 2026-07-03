using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityDefinition : ScriptableObject
{
	#region Entity Info
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
		loot = 1 << 10,
		move = 1 << 11
	};

	[SerializeField] private EntityFlags entityFlags;
	[Flags]
	public enum EntityFlags
	{
		none = 0, canBecomeZombie = 1 << 0, canUtilizeCover = 1 << 1
	}
	#endregion

	#region Definition Prefab To Use
	[Header("Definition Prefab To Use")]
	public GameObject gameObjectPrefab;
	public List<GameObject> allowedCharacterModels = new();
	#endregion

	#region Items Dropped On Death
	[Header("Items Dropped On Death")]
	[SerializeField] private List<ItemDropData> itemsDroppedOnDeath = new();
	#endregion

	#region Entity Stats
	[Header("Entity Stats")]
	[SerializeField] private int maxHealth = 100;
	[SerializeField] private int maxWater = 100;
	[SerializeField] private int maxFood = 100;
	[SerializeField] private int maxStamina = 100;

	[Header("Entity Stats Drain")]
	[SerializeField] private float waterDrainSeconds = 15;
	[SerializeField] private float waterDrainAmount = 1;
	[SerializeField] private float foodDrainSeconds = 25;
	[SerializeField] private float foodDrainAmount = 1;

	[Header("Entity Stamina Drain")]
	[SerializeField] private float staminaDrainSeconds = 0.4f;
	[SerializeField] private float staminaDrainAmount = 1;
	[SerializeField] private float staminaRegenSeconds = 1f;
	[SerializeField] private float staminaRegenAmount = 1;
	[SerializeField] private float exhaustToSprintThreshold = 0.15f;
	#endregion

	#region Entity Movement Behaviour
	[Header("Entity Movement Behaviour")]
	[SerializeField] private float walkSpeed = 2.5f;
	[SerializeField] private float sprintSpeed = 5.5f;
	[SerializeField] private float rotationSpeed = 240;
	[SerializeField] private float acceleration = 10;
	[SerializeField] private float stoppingDistance = 1;
	[SerializeField] private float fleeDistance = 20;
	[SerializeField] private float minIdleTime = 1;
	[SerializeField] private float maxIdleTime = 3;
	#endregion

	#region Entity Perception Settings
	[Header("Entity Perception Settings")]
	[SerializeField] private float viewAngle = 120f;
	[SerializeField] private float viewDistance = 30f;
	[SerializeField] private float highAlertDuration = 10f;
	[SerializeField] private float viewAngleMultiplier = 1.5f;
	[SerializeField] private float viewDistanceMultiplier = 1.5f;
	#endregion

	#region Entity Sound Detection
	[Header("Entity Sound Detection")]
	[SerializeField] private float soundSensitivity; //percentage chance divided by distance to sound source or something
	#endregion

	#region read only
	public string Name => entityName;
	public bool Player => isPlayer;
	public LifeState StartingLifeState => startingLifeState;
	public Capability Capabilities => capabilities;
	public EntityFlags Flags => entityFlags;

	public GameObject GameObjectPrefab => gameObjectPrefab;

	public List<ItemDropData> ItemsDroppedOnDeath => itemsDroppedOnDeath;

	public int MaxHealth => maxHealth;
	public int MaxWater => maxWater;
	public int MaxFood => maxFood;
	public int MaxStamina => maxStamina;

	public float WaterDrainSeconds => waterDrainSeconds;
	public float WaterDrainAmount => waterDrainAmount;
	public float FoodDrainSeconds => foodDrainSeconds;
	public float FoodDrainAmount => foodDrainAmount;

	public float StaminaDrainSeconds => staminaDrainSeconds;
	public float StaminaDrainAmount => staminaDrainAmount;
	public float StaminaRegenSeconds => staminaRegenSeconds;
	public float StaminaRegenAmount => staminaRegenAmount;
	public float ExhaustToSprintThreshold => exhaustToSprintThreshold;

	public float WalkSpeed => walkSpeed;
	public float SprintSpeed => sprintSpeed;
	public float RotationSpeed => rotationSpeed;
	public float Acceleration => acceleration;
	public float StoppingDistance => stoppingDistance;
	public float FleeDistance => fleeDistance;
	public float FleeSqrDistance => fleeDistance * fleeDistance;
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

[Serializable]
public class ItemDropData
{
	public ItemDefinition Item;
	public int MinDropAmount;
	public int MaxDropAmount;
}
