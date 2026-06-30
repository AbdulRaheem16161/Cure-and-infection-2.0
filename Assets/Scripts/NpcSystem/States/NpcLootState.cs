using UnityEngine;
using Game.MyNPC;

public class NpcLootState : NpcBaseMovementState
{
    IInteractable interactableContainer;
    ILootContainer lootableContainer;

    public LootStage lootStage;

    public enum LootStage
    {
        movingToLootable, notLooting, looting, lootingComplete, LootingCanceled
    }

    float lootTimeout;
    float timeToLootItems;
    float lootTimer;

    public NpcLootState(NPCStateMachine stateMachine, int priority) : base(stateMachine, priority) { }

    public override bool IsValid()
    {
        return stateMachine.capabilityOverrides.HasFlag(EntityDefinition.Capability.loot) && Beliefs.FleeTarget == null &&
            Beliefs.Target == null && Beliefs.CanLootContainer;
    }

    public override void Enter()
    {
        lootStage = LootStage.movingToLootable;
        lootTimer = 0;
        MoveToDestination(Beliefs.LootableContainer.collider.transform.position, MoveType.walk);

        interactableContainer = Beliefs.LootableContainer.interactable;
        if (Beliefs.LootableContainer.interactable is ILootContainer lootable)
            lootableContainer = lootable;
    }

    public override void Tick(float deltaTime)
    {
        if (stateMachine.StatsHandler.LifeState == EntityDefinition.LifeState.dead) return;
        if (!Beliefs.CanLootContainer || Beliefs.LootableContainer == null) return;

        if (!HasReachedDestination()) return;

        TryLootItemsInContainer();
        LootItemsWaitTimer(deltaTime);
    }

    public override void Exit()
    {
        interactableContainer.InteractPress(stateMachine.Interactor);
        stateMachine.Agent.updateRotation = true;
        stateMachine.Agent.isStopped = false;
    }

    private void TryLootItemsInContainer()
    {
        if (lootStage == LootStage.movingToLootable)
        {
            lootStage = LootStage.notLooting;
            Beliefs.LootableContainer.interactable.InteractPress(stateMachine.Interactor);
        }

        if (lootStage == LootStage.notLooting && lootableContainer.lootSpawningState == ILootContainer.LootSpawningState.lootSpawned)
            LootItems(lootableContainer);

        if (lootStage == LootStage.lootingComplete)
            CompleteItemLooting();

        if (lootStage == LootStage.LootingCanceled) //doesnt really need any special behaviour, just treat it as looted but forget about it.
            CompleteItemLooting(true);
    }
    private void LootItems(ILootContainer lootable)
    {
        int itemsToLoot = 0;
        lootStage = LootStage.looting;
        timeToLootItems = 1f;
        lootTimeout = 6f;

        for (int i = 0; i < lootable.ItemContainer.Items.Count; i++)
        {
            if (lootable.ItemContainer.Items[i] == null || lootable.ItemContainer.Items[i].ItemDefinitionNull) continue;

            itemsToLoot++;
            timeToLootItems += 0.25f;
            lootTimeout += 0.25f;
            InventoryService.TryResolveSlotInteraction(lootable.ItemContainer, i, stateMachine.InventoryHandler.ItemContainer, -1, true);
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

    private void LootItemsWaitTimer(float deltaTime)
    {
        if (lootStage != LootStage.looting) return;

        lootTimer += deltaTime;

        if (lootTimer > timeToLootItems)
            lootStage = LootStage.lootingComplete;

        if (lootTimer > lootTimeout)
            lootStage = LootStage.LootingCanceled;
    }
}
