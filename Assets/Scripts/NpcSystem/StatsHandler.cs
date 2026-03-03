using UnityEngine;
using Game.MyNPC;
using System;

public class StatsHandler : MonoBehaviour, IDamageable
{
	private bool IsPlayer = false;
	public NpcController NpcController { get; private set; }
	public NPCStateMachine StateMachine { get; private set; }
	public EquipmentHandler EquipmentHandler { get; private set; }
	public InventoryHandler InventoryHandler { get; private set; }

	public bool EnableDeath;
	public bool IsDead { get; private set; }

	public int health;
	public int water;
	public int food;
	public int stamina;

	public float headProtection;
	public float chestProtection;

	public event Action OnHit;
	public event Action OnDeath;

	#region set up and initilze
	private void Awake()
	{
		NpcController = GetComponent<NpcController>();

		if (NpcController == null)
		{
			Debug.LogError($"NpcController script not found on this gameobject: {gameObject.name}");
			return;
		}

		StateMachine = GetComponent<NPCStateMachine>();

		if (StateMachine == null)
		{
			Debug.LogWarning($"NPCStateMachine script not found on this gameobject: {gameObject.name}, it may not be needed");
			IsPlayer = false;
		}
		else
			IsPlayer = true;

		EquipmentHandler = GetComponent<EquipmentHandler>();

		if (EquipmentHandler == null)
		{
			Debug.LogError($"EquipmentHandler script not found on this gameobject: {gameObject.name}");
			return;
		}

		InventoryHandler = GetComponent<InventoryHandler>();

		if (InventoryHandler == null)
		{
			Debug.LogError($"InventoryHandler script not found on this gameobject: {gameObject.name}");
			return;
		}
	}

	public void InitilizeStats(NpcDefinition npcDefinition)
	{
		EnableDeath = true;
		IsDead = false;

		health = npcDefinition.MaxHealth;
		water = npcDefinition.MaxWater;
		food = npcDefinition.MaxFood;
		stamina = npcDefinition.MaxStamina;
	}
	#endregion

	#region event subbing/unsubbing
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

	#region recieve damage (will need updating to account for body part protection)
	public void RecieveDamage(int damageAmount, GameObject Attacker = null)
	{
		OnHit?.Invoke();
		health -= damageAmount;
		Debug.LogError("recieved damage");

		if (!EnableDeath) return;
		if (health <= 0 && !IsDead)
		{
			IsDead = true;
			OnDeath?.Invoke();
		}
	}
	#endregion

	#region item equip event listeners
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

	#region consumable used event listener
	private void UseConsumable(EquipmentSlot slot)
	{
		if (slot.item.ItemDefinition is ConsumableDefinition consumableDefinition)
		{
			if (consumableDefinition.RestorationTypes.HasFlag(ConsumableDefinition.RestorationType.health))
			{
				Mathf.Clamp(health += consumableDefinition.HealthRestored, 0, 100);
			}
			if (consumableDefinition.RestorationTypes.HasFlag(ConsumableDefinition.RestorationType.water))
			{
				Mathf.Clamp(water += consumableDefinition.WaterRestored, 0, 100);
			}
			water += consumableDefinition.WaterRestored;
			if (consumableDefinition.RestorationTypes.HasFlag(ConsumableDefinition.RestorationType.food))
			{
				Mathf.Clamp(food += consumableDefinition.FoodRestored, 0, 100);
			}
		}
	}
	#endregion
}
