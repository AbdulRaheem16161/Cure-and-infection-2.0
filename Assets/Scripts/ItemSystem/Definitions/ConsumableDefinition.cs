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
}
