using UnityEngine;

public class CharacterStatsTest : MonoBehaviour
{
	public EquipmentHandler EquipmentHandler { get; private set; }
	public InventoryHandler InventoryHandler { get; private set; }

	public int health;
	public int water;
	public int food;
	public int stamina;

	public float headProtection;
	public float chestProtection;

	private void Awake()
	{
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
}
