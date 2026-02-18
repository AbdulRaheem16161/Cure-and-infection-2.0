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
}
