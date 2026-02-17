using UnityEngine;

public class Armour : Item<ArmourDefinition>
{
	private ArmourDefinition armourDefinition;

	public override void InitializeItem(ArmourDefinition definition, int itemStack)
	{
		base.InitializeItem(definition, itemStack);
		armourDefinition = definition;

		// armour-specific setup here
	}
}
