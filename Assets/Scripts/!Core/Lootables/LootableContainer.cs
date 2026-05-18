using UnityEngine;

[RequireComponent(typeof(Hinge))]
public class LootableContainer : MonoBehaviour, IInteractable, ILootContainer
{
    [SerializeField] private ItemContainer itemContainer;
    public ItemContainer ItemContainer => itemContainer;

    public string LootableName;
    public string ContainerName => LootableName;

    public bool CanLoot => true;
    public bool Open { get; private set; }

    private Hinge hinge;

    private void Awake()
    {
        hinge = GetComponent<Hinge>();
        Open = false;
        hinge.CloseHinge();
    }

    public void InteractPress(Interactor interactor)
    {
        Open = !Open;
        hinge.Toggle();
    }
    public void InteractHoldComplete(Interactor interactor)
    {
        return; //not used
    }

    private void SpawLootableItems()
    {

    }
}
