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

	[SerializeField] private int protectionProvided;
	[SerializeField] private int inventorySlotsProvided;
	#endregion

	//add fields for ui icons, 3d prefab models etc, sfx/vfx specific for armour etc...
	#region model, vfx, sfx
	#endregion

	#region readoly properties
	public ArmourSlotType ArmourSlot => armourType;
	public int ProtectionProvided => protectionProvided;
	public int InventorySlotsProvided => inventorySlotsProvided;
	#endregion

	#region armour behaviour methods
	public override void UseItem()
	{
		//not supported
		Debug.LogError("UseItem method not implemented for this item type");
	}
	public override void EquipItem()
	{
		//add protection to stats
		Debug.Log($"equipped armour: {ItemName}");
	}
	public override void UnEquipItem()
	{
		//remove protection from stats
		Debug.Log($"unequipped armour: {ItemName}");
	}
	public override void Holster()
	{
		//not supported
		Debug.LogError("Holster method not implemented for this item type");
	}
	public override void UnHolster()
	{
		//not supported
		Debug.LogError("UnHolster method not implemented for this item type");
	}
	#endregion
}
