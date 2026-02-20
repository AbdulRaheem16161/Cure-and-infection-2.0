using UnityEngine;

public class CharacterStatsTest : MonoBehaviour
{
	public int health;
	public int water;
	public int food;
	public int stamina;

	public float headProtection;
	public float chestProtection;

	public void RecalculateArmourProtectionStats(Armour[] equippedArmour)
	{
		float headProtection = 0;
		float chestProtection = 0;

		foreach (Armour armour in equippedArmour)
		{
			if (armour == null) continue; //no armour equipped

			ArmourDefinition armourDefinition = armour.ArmourDefinition;

			if (armourDefinition.ArmourSlot == ArmourDefinition.ArmourSlotType.helmet)
				headProtection += armourDefinition.ProtectionProvided;
			else if (armourDefinition.ArmourSlot == ArmourDefinition.ArmourSlotType.chest)
				chestProtection += armourDefinition.ProtectionProvided;
		}
	}

	public void UseConsumable(ConsumableDefinition consumable)
	{
		if (consumable.RestorationTypes.HasFlag(ConsumableDefinition.RestorationType.health))
			health += consumable.HealthRestored;
		if (consumable.RestorationTypes.HasFlag(ConsumableDefinition.RestorationType.water))
			water += consumable.WaterRestored;
		if (consumable.RestorationTypes.HasFlag(ConsumableDefinition.RestorationType.food))
			food += consumable.FoodRestored;
	}
}
