using UnityEngine;

[CreateAssetMenu(fileName = "Armour", menuName = "ScriptableObjects/Item/Armour")]
public class ArmourDefinition : ItemDefinition
{
	#region armour properties
	[Header("Armour Properties")]
	[SerializeField] private ArmourSlotType armourType;
	public enum ArmourSlotType
	{
		unset, helmet, chest, backpack
	}

	[Tooltip("% Damage reduction")]
	[Range(0f, 1f)]
	[SerializeField] private float protectionProvided;
	[SerializeField] private float inventorySlotsProvided;
	#endregion

	//add fields for ui icons, 3d prefab models etc, sfx/vfx specific for armour etc...
	#region model, vfx, sfx
	#endregion

	#region readoly properties
	public ArmourSlotType ArmourSlot => armourType;
	public float ProtectionProvided => protectionProvided;
	public float InventorySlotsProvided => inventorySlotsProvided;
	#endregion

	public override void OnEquip(EquipmentHandler handler, EquipmentSlot slot)
	{
		Armour armourInstance = handler.GetOrCreateItemInstance(slot) as Armour;

		if (armourInstance.ArmourDefinition.ArmourSlot ==ArmourSlotType.helmet)
			armourInstance.transform.SetParent(handler.equippedHelmetParent.transform);
		else if (armourInstance.ArmourDefinition.ArmourSlot == ArmourSlotType.chest)
			armourInstance.transform.SetParent(handler.equippedChestpieceParent.transform);
		else if (armourInstance.ArmourDefinition.ArmourSlot == ArmourSlotType.backpack)
			armourInstance.transform.SetParent(handler.equippedBackpackParent.transform);

		armourInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		armourInstance.gameObject.SetActive(true);
	}

	public override void OnUnequip(EquipmentHandler handler, EquipmentSlot slot)
	{
		Armour armourInstance = handler.GetOrCreateItemInstance(slot) as Armour	;
		armourInstance.gameObject.SetActive(false);
	}
}
