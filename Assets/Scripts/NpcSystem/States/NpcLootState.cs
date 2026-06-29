using Game.MyNPC;

public class NpcLootState : NpcBaseMovementState
{
    private bool lootItems;

    readonly float lootTimeout = 5f;
    readonly float lootDelay = 2f;
    float lootTimer;

    public NpcLootState(NPCStateMachine stateMachine, int priority) : base(stateMachine, priority) { }

    public override bool IsValid()
    {
        return stateMachine.capabilityOverrides.HasFlag(EntityDefinition.Capability.loot) && Beliefs.FleeTarget == null &&
            Beliefs.Target == null && Beliefs.CanLootContainer;
    }

    public override void Enter()
    {
        lootItems = true;
        lootTimer = 0;
        MoveToDestination(Beliefs.LootableContainer.collider.transform.position, MoveType.walk);
    }

    public override void Tick(float deltaTime)
    {
        if (stateMachine.StatsHandler.LifeState == EntityDefinition.LifeState.dead) return;
        if (!Beliefs.CanLootContainer || Beliefs.LootableContainer == null) return;

        if (!HasReachedDestination()) return;
        TryLootItemsInContainer(deltaTime);
    }

    public override void Exit()
    {
        stateMachine.Agent.updateRotation = true;
        stateMachine.Agent.isStopped = false;
    }

    private void TryLootItemsInContainer(float deltaTime)
    {
        if (Beliefs.LootableContainer.interactable is ILootContainer lootable)
        {
            Beliefs.LootableContainer.interactable.InteractPress(stateMachine.Interactor);
            lootTimer += deltaTime;

            if (lootItems && lootTimer > lootDelay)
            {
                lootItems = false;
                for (int i = 0; i < lootable.ItemContainer.Items.Count; i++)
                {
                    if (lootable.ItemContainer.Items[i] == null || lootable.ItemContainer.Items[i].ItemDefinitionNull) continue;
                    InventoryService.TryResolveSlotInteraction(lootable.ItemContainer, i, stateMachine.InventoryHandler.ItemContainer, -1, true);
                }
            }

            if (lootTimer > lootTimeout)
                CompleteItemLooting();
        }
    }
    private void CompleteItemLooting(bool forgetLootable = false)
    {
        Beliefs.LootableContainer.MarkAsAlreadyLooted();

        foreach (InteractContext interactContext in stateMachine.NpcPerception.interactables)
        {
            if (interactContext.interactable != Beliefs.LootableContainer.interactable) continue;
            interactContext.MarkAsAlreadyLooted();

            if (forgetLootable) break;

            Beliefs.AddLootableToLongTermMemory(interactContext);
            break;
        }
    }
}
