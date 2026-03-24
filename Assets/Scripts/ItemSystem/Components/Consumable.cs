using UnityEngine;

public class Consumable : Item<ConsumableDefinition>
{
	public override void InitializeItem(ConsumableDefinition definition, GameObject itemModel, int itemStack)
	{
		base.InitializeItem(definition, itemModel, itemStack);

		//consumable-specific setup here
	}
}
