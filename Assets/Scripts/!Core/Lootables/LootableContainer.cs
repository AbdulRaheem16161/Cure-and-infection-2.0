using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Hinge))]
public class LootableContainer : MonoBehaviour, IInteractable, ILootContainer
{
    [Header("Loot Container Settings")]
    public string LootableName;
    public string ContainerName => LootableName;
    public string InteractableName
    {
        get { if (Open) return $"Close {LootableName}";
            return $"Open {LootableName}";}
    }
    public bool CanLoot => true;

    [SerializeField] private int InitialContainerSize;
    [SerializeField] private ItemContainer itemContainer;
    public ItemContainer ItemContainer => itemContainer;

    public bool Open { get; private set; }

    private Hinge hinge;

    [Header("Container Item Spawn Settings")]
    [SerializeField] private ItemSpawnConfig itemSpawnConfig;
    [SerializeField] private List<SpawnEntry> spawnTable = new();
    private float totalSpawnWeight;

    private static readonly System.Random systemRandom = new();

    private void Awake()
    {
        hinge = GetComponent<Hinge>();
        Open = false;
        hinge.CloseHinge();
        itemContainer.SetContainerSize(InitialContainerSize);

        CheckForItemSpawnConfigConflicts(itemSpawnConfig.allowedRangedWeaponsFlags, itemSpawnConfig.rangedWeaponWeights, "Ranged Weapon");
        CheckForItemSpawnConfigConflicts(itemSpawnConfig.allowedMeleeWeaponFlags, itemSpawnConfig.meleeWeaponWeights, "Melee Weapon");
        CheckForItemSpawnConfigConflicts(itemSpawnConfig.allowedArmourFlags, itemSpawnConfig.armourWeights, "Armour");
        CheckForItemSpawnConfigConflicts(itemSpawnConfig.allowedConsumableFlags, itemSpawnConfig.consumableWeights, "Consumbale");
    }

    private void Start()
    {
        CreateWeightedSpawnTable();
    }

    #region IInteractable Interface Methods
    public void InteractPress(Interactor interactor)
    {
        Open = !Open;
        hinge.Toggle();
        TestInventoryManager.LootContainer(gameObject, Open);
    }
    public void InteractHoldComplete(Interactor interactor)
    {
        return; //not used
    }
    #endregion

    #region Spawn Lootable Items In Container
    public void SpawnLootableItemsInContainer()
    {
        int tries = 0;
        int maxTries = 10000;
        int itemsFoundToSpawn = 0;
        int itemsToSpawnCount = systemRandom.Next(itemSpawnConfig.minItemsToSpawn, itemSpawnConfig.maxItemsToSpawn + 1);

        Dictionary<ItemDefinition, int> itemsToSpawn = new();

        while (itemsFoundToSpawn < itemsToSpawnCount && tries < maxTries)
        {
            float roll = (float)systemRandom.NextDouble() * totalSpawnWeight;
            float current = 0f;

            foreach (var entry in spawnTable)
            {
                current += entry.weight;

                if (roll > current)
                    continue;

                if (itemsToSpawn.TryGetValue(entry.item, out int count)) //limit duplicates to 2
                {
                    if (count >= 2)
                        break;

                    itemsToSpawn[entry.item]++;
                }
                else
                    itemsToSpawn[entry.item] = 1;

                itemsFoundToSpawn++;
                break;
            }
            tries++;
        }

        foreach (var kvp in itemsToSpawn)
        {
            int itemCount = kvp.Value;
            for (int i = 0; i < itemCount; i++)
                itemContainer.AddNewItem(new InventoryItem(kvp.Key, systemRandom.Next(1, kvp.Key.StackLimit + 1)));
        }
    }
    #endregion

    #region Check For itemSpawnConfig conflicts
    private void CheckForItemSpawnConfigConflicts<T>(T allowedFlags, List<ItemSpawnConfig.FlagWeight<T>> weights, string context) where T : Enum
    {
        int allowed = Convert.ToInt32(allowedFlags);

        int weightedMask = 0;

        foreach (var w in weights)
            weightedMask |= Convert.ToInt32(w.flag);

        int invalid = weightedMask & ~allowed;

        if (invalid != 0)
            Debug.LogWarning($"[{context}] Flags have spawn weights but are NOT allowed: {Convert.ToString(invalid, 2)}");
    }
    #endregion

    #region Create Weighted Spawn Table
    private void CreateWeightedSpawnTable()
    {
        spawnTable.Clear();
        totalSpawnWeight = 0;

        foreach (ItemDefinition item in ItemSpawner.Instance.itemDefinitionList)
        {
            if (!ItemAllowedToSpawn(item)) continue;

            SpawnEntry spawnEntry = CreateSpawnEntry(item);
            spawnTable.Add(spawnEntry);
            totalSpawnWeight += spawnEntry.weight;
        }
    }
    private bool ItemAllowedToSpawn(ItemDefinition item)
    {
        if (item is WeaponRangedDefinition weaponRangedDefinition)
            return (weaponRangedDefinition.WeaponFlags & itemSpawnConfig.allowedRangedWeaponsFlags) != 0;

        else if (item is WeaponMeleeDefinition weaponMeleeDefinition)
            return (weaponMeleeDefinition.WeaponFlags & itemSpawnConfig.allowedMeleeWeaponFlags) != 0;

        else if (item is ArmourDefinition armourDefinition)
            return (armourDefinition.ArmourFlags & itemSpawnConfig.allowedArmourFlags) != 0;

        else if (item is ConsumableDefinition consumableDefinition)
            return (consumableDefinition.ConsumableFlags & itemSpawnConfig.allowedConsumableFlags) != 0;

        else
            return true;
    }
    #endregion

    #region Create Spawn Table Entry
    private SpawnEntry CreateSpawnEntry(ItemDefinition item)
    {
        float weightedSpawn = item.BaseSpawnWeight;

        if (item.BaseSpawnWeight <= 0.0001f || float.IsNaN(item.BaseSpawnWeight))
            Debug.LogError($"[SpawnTable] {item.name} has invalid BaseSpawnWeight: {item.BaseSpawnWeight}. It will never spawn.");

        if (item is WeaponRangedDefinition weaponRangedDefinition)
            weightedSpawn *= ApplyFlagWeights(weaponRangedDefinition.WeaponFlags, itemSpawnConfig.rangedWeaponWeights);

        else if (item is WeaponMeleeDefinition weaponMeleeDefinition)
            weightedSpawn *= ApplyFlagWeights(weaponMeleeDefinition.WeaponFlags, itemSpawnConfig.meleeWeaponWeights);

        else if (item is ArmourDefinition armourDefinition)
            weightedSpawn *= ApplyFlagWeights(armourDefinition.ArmourFlags, itemSpawnConfig.armourWeights);

        else if (item is ConsumableDefinition consumableDefinition)
            weightedSpawn *= ApplyFlagWeights(consumableDefinition.ConsumableFlags, itemSpawnConfig.consumableWeights);

        return new(item, weightedSpawn);
    }
    private float ApplyFlagWeights<T>(T itemFlags, List<ItemSpawnConfig.FlagWeight<T>> weights) where T : Enum
    {
        float result = 1f;
        bool firstHit = true;

        int itemValue = Convert.ToInt32(itemFlags);

        foreach (var weight in weights)
        {
            int flagValue = Convert.ToInt32(weight.flag);

            if ((itemValue & flagValue) == 0)
                continue;

            if (firstHit)
            {
                result *= weight.multiplier;
                firstHit = false;
            }
            else
            {
                float reduced = 1f + (weight.multiplier - 1f) * 0.35f; //diminish effect
                result *= reduced;
            }
        }
        return result;
    }
    #endregion

    [Serializable]
    public class SpawnEntry
    {
        public ItemDefinition item;
        public float weight;

        public SpawnEntry(ItemDefinition item, float weight)
        {
            this.item = item;
            this.weight = weight;
        }
    }
}
