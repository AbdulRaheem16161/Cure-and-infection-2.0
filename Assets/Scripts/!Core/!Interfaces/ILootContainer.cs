using UnityEngine;

public interface ILootContainer
{
    ItemContainer ItemContainer { get; }

    string ContainerName { get; }

    bool CanLoot {  get; }
}
