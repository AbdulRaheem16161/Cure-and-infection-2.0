using UnityEngine;

public class Consumable : Item<ConsumableDefinition>
{
	[SerializeField] private ConsumableDefinition consumableDefinition;

	public ConsumableDefinition ConsumableDefinition => consumableDefinition;

	public override void InitializeItem(ConsumableDefinition definition, int itemStack)
	{
		base.InitializeItem(definition, itemStack);
		consumableDefinition = definition;

		//consumable-specific setup here
	}
}
