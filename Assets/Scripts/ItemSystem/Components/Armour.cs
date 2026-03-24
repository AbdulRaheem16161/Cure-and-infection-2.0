using UnityEngine;

public class Armour : Item<ArmourDefinition>
{
	public override void InitializeItem(ArmourDefinition definition, GameObject itemModel, int itemStack)
	{
		base.InitializeItem(definition, itemModel, itemStack);

		// armour-specific setup here
	}
}
