using UnityEngine;
using System;
using static NpcDefinition;

[RequireComponent(typeof(EquipmentHandler))]
public class StatsHandler : MonoBehaviour, IDamageable
{
	public NpcDefinition NpcDefinition { get; private set; }
	public EquipmentHandler EquipmentHandler { get; private set; }
	private BoxCollider hitCollider;
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
	#endregion

	#region debug options
	[Header("Debug Options")]
	public bool invincible;
	public bool forceRespawn;
	#endregion

	#region events
	public event Action OnHit;
	public event Action OnDeath;
	public static event Action<GameObject> OnZombificationComplete;
	#endregion

	#region awake + Initialize stats handler method
	private void Awake()
	{
		hitCollider = GetComponent<BoxCollider>();
		EquipmentHandler = GetComponent<EquipmentHandler>();

		if (hitCollider == null)
			Debug.LogError("No HitCollider on :" + gameObject + "ignore if testing, or add one");

		if (!_Initialized)
			InitializeStats(Team, null);
	}
	public void InitializeStats(NPCSpawner.Teams team, NpcDefinition npcDefinition)
	{
		_Initialized = true;
		NpcDefinition = npcDefinition;
		this.team = team;

		if (npcDefinition == null) return; //keeps values in inspector and allows partial component testing
		lifeState = npcDefinition.StartingLifeState;

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

	#region Zombification complete event invoking
	public void CompleteZombification()
	{
		OnZombificationComplete?.Invoke(gameObject);
	}
	#endregion

	#region recive damage interface + invoke hit and death events
	public void RecieveDamage(int damageAmount, GameObject Attacker = null)
	{
		OnHit?.Invoke();
		health -= damageAmount;

		if (invincible) return;
		if (health <= 0 && lifeState != LifeState.dead)
		{
			lifeState = LifeState.dead;
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
