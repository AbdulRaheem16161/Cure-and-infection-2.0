using UnityEngine;

public class Armour : Item<ArmourDefinition>
{
	public override void InitializeItem(ArmourDefinition definition, int itemStack)
	{
		base.InitializeItem(definition, itemStack);

		// armour-specific setup here
	}
}
