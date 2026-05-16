using UnityEngine;

public class LootableContainer : MonoBehaviour, IInteractable, ILootContainer
{
    [SerializeField] private ItemContainer itemContainer;
    public ItemContainer ItemContainer => itemContainer;

    public string ContainerName => "Lootable Container";

    public bool CanLoot => true;

    public void InteractPress(Interactor interactor)
    {
        //open both this inventory + interactor.Inventory in ui
        Debug.LogError("Needs implementation");

        return;
    }

    public void InteractHoldComplete(Interactor interactor)
    {
        return;
    }

    private void SpawLootableItems()
    {

    }
}
