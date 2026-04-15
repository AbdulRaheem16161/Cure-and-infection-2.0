using UnityEngine;
using System;
using static EntityDefinition;
using UnityEngine.AI;
using Game.MyNPC;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(EquipmentHandler))]
public class StatsHandler : MonoBehaviour, IDamageable
{
	public EntityDefinition Definition { get; private set; }
	public NPCStateMachine NpcStateMachine { get; private set; }
	public EquipmentHandler EquipmentHandler { get; private set; }
	private bool _Initialized = false;

	[Header("Team")]
	[SerializeField, ReadOnly] private NPCSpawner.Teams team;
	public NPCSpawner.Teams Team => team;

	[Header("Life State")]
	[SerializeField, ReadOnly] private LifeState lifeState;
	public LifeState LifeState => lifeState;

	#region stats
	[Header("Stats")]
	public int health;
	public int water;
	public int food;
	public int stamina;

	public float headProtection;
	public float chestProtection;

	private float waterDrainTimer;
	private float foodDrainTimer;
	private float staminaDrainTimer;
	private float staminaRegenTimer;

	public bool IsExhausted { get; private set; }
	#endregion

	#region debug options
	[Header("Debug Options")]
	public bool invincible;
	public bool forceRespawn;
	#endregion

	#region events
	public event Action<bool> OnExhausted;
	public event Action<DamageContext> OnHit;
	public event Action OnInitialize;
	public event Action OnDeath;
	public static event Action<GameObject> OnZombificationComplete;
	#endregion

	#region awake + Initialize stats handler method
	private void Awake()
	{
		NpcStateMachine = GetComponent<NPCStateMachine>();
		EquipmentHandler = GetComponent<EquipmentHandler>();

		if (!_Initialized)
			InitializeStats(Team, null);
	}
	public void InitializeStats(NPCSpawner.Teams team, EntityDefinition definition)
	{
		OnInitialize?.Invoke();
		_Initialized = true;
		Definition = definition;
		this.team = team;

		if (definition == null) return; //keeps values in inspector and allows partial component testing
		lifeState = definition.StartingLifeState;

		health = definition.MaxHealth;
		water = definition.MaxWater;
		food = definition.MaxFood;
		stamina = definition.MaxStamina;
	}
	#endregion

	#region event subbing/unsubbing
	private void OnEnable()
	{
		EquipmentHandler.OnEquippedItemChanges += OnEquippedItemChanges;
		EquipmentHandler.OnConsumableUsed += UseConsumable;
	}
	private void OnDisable()
	{
		EquipmentHandler.OnEquippedItemChanges -= OnEquippedItemChanges;
		EquipmentHandler.OnConsumableUsed -= UseConsumable;
	}
	#endregion

	private void Update()
	{
		HandleWaterDrain();
		HandleFoodDrain();

		if (NpcStateMachine != null) //npc handle move intent
		{
			if (NpcStateMachine.IsSprinting)
				HandleStaminaDrain();
			else
				HandleStaminaRegen();
		}
		else
		{
			//would be player so handle reading player movement intent here
		}
	}

	#region Stats Drain Handlers
	private void HandleWaterDrain()
	{
		if (lifeState == LifeState.zombified) return;

		waterDrainTimer -= Time.deltaTime;
		if (waterDrainTimer < 0)
		{
			waterDrainTimer = Definition.WaterDrainSeconds;
			water -= (int)Definition.WaterDrainAmount;
		}
	}
	private void HandleFoodDrain()
	{
		if (lifeState == LifeState.zombified) return;

		foodDrainTimer -= Time.deltaTime;
		if (foodDrainTimer < 0)
		{
			foodDrainTimer = Definition.FoodDrainSeconds;
			food -= (int)Definition.FoodDrainAmount;
		}
	}
	private void HandleStaminaDrain()
	{
		if (lifeState == LifeState.zombified) return;

		staminaDrainTimer -= Time.deltaTime;
		if (staminaDrainTimer < 0)
		{
			staminaDrainTimer = Definition.StaminaDrainSeconds;
			stamina -= (int)Definition.StaminaDrainAmount;

			if (!IsExhausted && stamina <= 0)
			{
				IsExhausted = true;
				OnExhausted?.Invoke(true);
			}
		}
	}
	private void HandleStaminaRegen()
	{
		if (lifeState == LifeState.zombified) return;

		staminaRegenTimer -= Time.deltaTime;
		if (staminaRegenTimer < 0)
		{
			staminaRegenTimer = Definition.StaminaRegenSeconds;
			stamina += (int)Definition.StaminaRegenAmount;

			if (IsExhausted && stamina >= (Definition.ExhaustToSprintThreshold * Definition.MaxStamina))
			{
				IsExhausted = false;
				OnExhausted?.Invoke(false);
			}
		}
	}
	#endregion

	#region Zombification complete event invoking
	public void CompleteZombification()
	{
		OnZombificationComplete?.Invoke(gameObject);
	}
	#endregion

	#region recive damage interface + invoke hit and death events
	public void RecieveDamage(DamageContext damageContext)
	{
		OnHit?.Invoke(damageContext);
		float damageRecieved = damageContext.Damage;

		switch (damageContext.BodyPartHit)
		{
			case HitCollider.BodyPart.Head:
			damageRecieved *= (1 - headProtection);
			break;

			case HitCollider.BodyPart.body:
			damageRecieved *= (1 - chestProtection);
			break;

			default:
			Debug.LogError($"body part of type {damageContext.BodyPartHit} not set up, defaulting to body hit");
			damageRecieved *= (1 - chestProtection);
			break;
		}

		health -= Mathf.RoundToInt(damageRecieved);

		if (invincible) return;
		if (health <= 0 && lifeState != LifeState.dead)
		{
			lifeState = LifeState.dead;
			OnDeath?.Invoke();
		}
	}
	#endregion

	#region on item equip/unequip events, update protection stats
	private void OnEquippedItemChanges(EquipmentSlot slot, bool wasEquipped)
	{
		if (slot.Item.ItemDefinition is not ArmourDefinition armourDefinition) return;

		static float GetProtectionModifier(float modifier, bool wasEquipped)
		{
			return wasEquipped ? modifier : -modifier;
		}

		switch (slot.EquipmentType)
		{
			case EquipmentHandler.EquipmentType.helmet:
			headProtection = GetProtectionModifier(armourDefinition.ProtectionProvided, wasEquipped);
			break;

			case EquipmentHandler.EquipmentType.chest:
			chestProtection = GetProtectionModifier(armourDefinition.ProtectionProvided, wasEquipped);
			break;
		}
	}
	#endregion

	#region on use consumable event, update stats
	private void UseConsumable(EquipmentSlot slot)
	{
		if (slot.Item.ItemDefinition is ConsumableDefinition consumableDefinition)
		{
			if (consumableDefinition.RestorationTypes.HasFlag(ConsumableDefinition.RestorationType.health))
				Mathf.Clamp(health += consumableDefinition.HealthRestored, 0, 100);

			if (consumableDefinition.RestorationTypes.HasFlag(ConsumableDefinition.RestorationType.water))
				Mathf.Clamp(water += consumableDefinition.WaterRestored, 0, 100);

			if (consumableDefinition.RestorationTypes.HasFlag(ConsumableDefinition.RestorationType.food))
				Mathf.Clamp(food += consumableDefinition.FoodRestored, 0, 100);
		}
	}
	#endregion

	#region Debug Kill
	public void DebugKillNpc()
	{
		health = -1;

		if (health <= 0 && lifeState != LifeState.dead)
		{
			lifeState = LifeState.dead;
			OnDeath?.Invoke();
		}
	}
	#endregion
}
