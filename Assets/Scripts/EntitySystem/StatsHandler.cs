using UnityEngine;
using System;

public class StatsHandler : MonoBehaviour, IDamageable
{
	public EquipmentHandler EquipmentHandler { get; private set; }
	private BoxCollider hitCollider;
	private bool _Initialized = false;

	[Header("Team")]
	[ReadOnly] private NPCSpawner.Teams team;
	public NPCSpawner.Teams Team => team;

	#region stats
	[Header("Stats")]
	public int health;
	public int water;
	public int food;
	public int stamina;

	public float headProtection;
	public float chestProtection;
	#endregion

	[HideInInspector] public bool EnableDeath;
	[HideInInspector] public bool EnableRespawn;
	[HideInInspector] public bool EnableZombification;
	public bool IsDead;
	public bool IsPlayer { get; private set; }

	#region events
	public event Action OnHit;
	public event Action OnDeath;
	#endregion

	#region awake + Initializing of stats script
	private void Awake()
	{
		hitCollider = GetComponent<BoxCollider>();

		if (hitCollider == null)
			Debug.LogError("No HitCollider on :" + gameObject + "ignore if testing, or add one");

		if (!_Initialized)
			InitializeStats(NPCSpawner.Teams.FreeFighter, GetComponent<EquipmentHandler>(), null);
	}
	public void InitializeStats(NPCSpawner.Teams team, EquipmentHandler equipmentHandler, NpcDefinition npcDefinition)
	{
		_Initialized = true;
		EquipmentHandler = equipmentHandler;

		if (EquipmentHandler == null)
		{
			Debug.LogError($"EquipmentHandler script not found on this gameobject: {gameObject.name}");
			return;
		}

		IsDead = false;
		EnableDeath = true;
		EnableRespawn = false;

		this.team = team;

		if (npcDefinition == null) return; //keep values in inspector

		if (npcDefinition.IsZombie)
			EnableZombification = false;
		else
			EnableZombification = true;

		health = npcDefinition.MaxHealth;
		water = npcDefinition.MaxWater;
		food = npcDefinition.MaxFood;
		stamina = npcDefinition.MaxStamina;
	}
	#endregion

	#region event subbing/unsubbing on enable/disable
	private void OnEnable()
	{
		EquipmentHandler.OnItemEquip += OnItemEquipped;
		EquipmentHandler.OnItemUnEquip += OnItemUnEquipped;
		EquipmentHandler.OnConsumableUsed += UseConsumable;
	}

	private void OnDisable()
	{
		EquipmentHandler.OnItemEquip -= OnItemEquipped;
		EquipmentHandler.OnItemUnEquip -= OnItemUnEquipped;
		EquipmentHandler.OnConsumableUsed -= UseConsumable;
	}
	#endregion

	#region recive damage interface + invoke hit and death events
	public void RecieveDamage(int damageAmount, GameObject Attacker = null)
	{
		OnHit?.Invoke();
		health -= damageAmount;

		if (!EnableDeath) return;
		if (health <= 0 && !IsDead)
		{
			IsDead = true;
			OnDeath?.Invoke();
		}
	}
	#endregion

	#region on item equip/unequip events, update protection stats
	private void OnItemEquipped(EquipmentSlot slot)
	{
		if (slot.item.ItemDefinition is ArmourDefinition armourDefinition)
		{
			switch (slot.equipmentType)
			{
				case EquipmentHandler.EquipmentType.helmet:
				headProtection += armourDefinition.ProtectionProvided;
				break;

				case EquipmentHandler.EquipmentType.chest:
				chestProtection += armourDefinition.ProtectionProvided;
				break;
			}
		}
	}
	private void OnItemUnEquipped(EquipmentSlot slot)
	{
		if (slot.item.ItemDefinition is ArmourDefinition armourDefinition)
		{
			switch (slot.equipmentType)
			{
				case EquipmentHandler.EquipmentType.helmet:
				headProtection -= armourDefinition.ProtectionProvided;
				break;

				case EquipmentHandler.EquipmentType.chest:
				chestProtection -= armourDefinition.ProtectionProvided;
				break;
			}
		}
	}
	#endregion

	#region on use consumable event, update stats
	private void UseConsumable(EquipmentSlot slot)
	{
		if (slot.item.ItemDefinition is ConsumableDefinition consumableDefinition)
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
}
