using UnityEngine;

public class Consumable : Item<ConsumableDefinition>
{
	public override void InitializeItem(ConsumableDefinition definition, int itemStack)
	{
		base.InitializeItem(definition, itemStack);

		//consumable-specific setup here
	}
}
