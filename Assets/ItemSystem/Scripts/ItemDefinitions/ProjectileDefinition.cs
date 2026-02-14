using UnityEngine;

[CreateAssetMenu(fileName = "Projectile", menuName = "ScriptableObjects/Item/Projectile")]
public class ProjectileDefinition : ItemDefinition
{
	//add fields for ui icons, 3d prefab models etc, sfx/vfx specific for projectiles etc...
	#region model, vfx, sfx
	#endregion

	#region projectile behaviour methods
	public override void UseItem()
	{
		//not supported
		Debug.LogError("UseItem method not implemented for this item type");
	}
	public override void EquipItem()
	{
		//not supported
		Debug.LogError("UseItem method not implemented for this item type");
	}
	public override void UnEquipItem()
	{
		//not supported
		Debug.LogError("UseItem method not implemented for this item type");
	}
	public override void Holster()
	{
		//not supported
		Debug.LogError("UseItem method not implemented for this item type");
	}
	public override void UnHolster()
	{
		//not supported
		Debug.LogError("UseItem method not implemented for this item type");
	}
	#endregion
}
