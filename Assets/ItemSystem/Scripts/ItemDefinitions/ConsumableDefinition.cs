using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Consumable", menuName = "ScriptableObjects/Item/Consumable")]
public class ConsumableDefinition : ItemDefinition
{
	#region consumable properties
	[Header("Consumable Properties")]
	[SerializeField] private RestorationType restorationType;
	[Flags]
	public enum RestorationType
	{
		health = 1, food = 2, water = 4
	}

	[SerializeField] private int healthRestored;
	[SerializeField] private int foodRestored;
	[SerializeField] private int waterRestored;
	#endregion

	//add fields for ui icons, 3d prefab models etc, sfx/vfx specific for consumables etc...

	#region readonly properties
	public RestorationType RestorationTypes => restorationType;
	public int HealthRestored => healthRestored;
	public int FoodRestored => foodRestored;
	public int WaterRestored => waterRestored;
	#endregion

	#region consumable behaviour methods
	public override void UseItem()
	{
		//adjust stats by consumables stats
		Debug.Log($"used consumable: {ItemName}");
	}
	public override void EquipItem()
	{
		//not supported
		Debug.LogError("EquipItem method not implemented for this item type");
	}
	public override void UnEquipItem()
	{
		//not supported
		Debug.LogError("UnEquipItem method not implemented for this item type");
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
