using System;
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
	[SerializeField] private int inventorySlotsProvided;

    [SerializeField] private ArmourFlag armourFlags;
    [Flags]
    public enum ArmourFlag
    {
        None = 0,
        Improvised = 1 << 0,
        Civilian = 1 << 1,
        Police = 1 << 2,
        Military = 1 << 3,
        Industrial = 1 << 4,
        Sporting = 1 << 5
    }
    #endregion

    //add fields for ui icons, 3d prefab models etc, sfx/vfx specific for armour etc...
    #region model, vfx, sfx
    #endregion

    #region readoly properties
    public ArmourSlotType ArmourSlot => armourType;
	public float ProtectionProvided => protectionProvided;
	public int InventorySlotsProvided => inventorySlotsProvided;

    public ArmourFlag ArmourFlags => armourFlags;
    #endregion
}
