using UnityEngine;

public class LootableContainer : MonoBehaviour, ILootContainer
{
    [SerializeField] private ItemContainer itemContainer;
    public ItemContainer ItemContainer => itemContainer;

    public string ContainerName => "Lootable Container";

    public bool CanLoot => true;
}
