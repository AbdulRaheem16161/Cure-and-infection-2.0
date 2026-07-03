using UnityEngine;

// Concrete Item component for simple / non-special items like wood, steel, etc.
public class SimpleItem : Item<ItemDefinition>
{
	// Simple items shouldn't be equippable - leave base behaviour or override to no-op if desired
	public override void EquipItem(EquipmentHandler equipmentHandler, Transform parentTransform)
	{

	}

	public override void UnEquipItem(EquipmentHandler equipmentHandler)
	{

	}
}
