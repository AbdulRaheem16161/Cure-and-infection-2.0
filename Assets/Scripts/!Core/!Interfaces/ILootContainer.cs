using UnityEngine;

public interface ILootContainer
{
    string ContainerName { get; }
    bool CanLoot { get; }
    public LootSpawningState lootSpawningState { get; set; }

    public enum LootSpawningState
    {
        empty, spawningLoot, lootSpawned
    }

    ItemContainer ItemContainer { get; }
}
