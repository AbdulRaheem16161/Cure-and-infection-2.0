using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemSpawnConfig
{
    public int minItemsToSpawn; 
    public int maxItemsToSpawn;

    [Header("Allowed Item Flags")]
    public WeaponRangedDefinition.WeaponFlag allowedRangedWeaponsFlags;
    public WeaponMeleeDefinition.WeaponFlag allowedMeleeWeaponFlags;
    public ArmourDefinition.ArmourFlag allowedArmourFlags;
    public ConsumableDefinition.ConsumableFlag allowedConsumableFlags;

    [Header("Item Flag Spawn Weight Modifers")]
    public List<FlagWeight<WeaponRangedDefinition.WeaponFlag>> rangedWeaponWeights;
    public List<FlagWeight<WeaponMeleeDefinition.WeaponFlag>> meleeWeaponWeights;
    public List<FlagWeight<ArmourDefinition.ArmourFlag>> armourWeights;
    public List<FlagWeight<ConsumableDefinition.ConsumableFlag>> consumableWeights;

    [System.Serializable]
    public struct FlagWeight<T>
    {
        public T flag;
        public float multiplier;
    }
}
