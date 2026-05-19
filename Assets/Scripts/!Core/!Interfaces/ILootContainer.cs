using UnityEngine;

public interface ILootContainer
{
    string ContainerName { get; }
    bool CanLoot { get; }

    ItemContainer ItemContainer { get; }
}
