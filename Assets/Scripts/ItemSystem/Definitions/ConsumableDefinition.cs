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
        none = 0,
        health = 1 << 0,
        food = 1 << 1,
        water = 1 << 2,
        stamina = 1 << 3
	}

	[SerializeField] private int healthRestored;
	[SerializeField] private int foodRestored;
	[SerializeField] private int waterRestored;
	[SerializeField] private int staminaRestored;

    [SerializeField] private ConsumableFlag consumableFlags;
    [Flags]
    public enum ConsumableFlag
    {
        none = 0,
        improvised = 1 << 0,
        consumer = 1 << 1,
        clinical = 1 << 2,
        experimental = 1 << 3,

        raw = 1 << 4,
        fresh = 1 << 5,
        packaged = 1 << 6,
        canned = 1 << 7,

        medical = 1 << 8,
    }
    #endregion

    //add fields for ui icons, 3d prefab models etc, sfx/vfx specific for consumables etc...

    #region readonly properties
    public RestorationType RestorationTypes => restorationType;
	public int HealthRestored => healthRestored;
	public int FoodRestored => foodRestored;
	public int WaterRestored => waterRestored;
	public int StaminaRestored => staminaRestored;

    public ConsumableFlag ConsumableFlags => consumableFlags;
	#endregion
}
