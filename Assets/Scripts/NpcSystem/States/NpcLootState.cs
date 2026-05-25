using Game.MyNPC;
using UnityEngine;
using static WeaponRangedDefinition;

public class NpcLootState : NpcBaseMovementState
{
    public NpcLootState(NPCStateMachine stateMachine, int priority) : base(stateMachine, priority) { }

    public override bool IsValid()
    {
        return stateMachine.capabilityOverrides.HasFlag(EntityDefinition.Capability.loot) && Beliefs.FleeTarget == null &&
            Beliefs.Target != null && Beliefs.HasLootableContainer;
    }

    public override void Enter()
    {
        MoveToDestination(Beliefs.LootableContainer.collider.transform.position, MoveType.walk);
    }

    public override void Tick(float deltaTime)
    {
        if (stateMachine.StatsHandler.LifeState == EntityDefinition.LifeState.dead) return;

        if (!Beliefs.HasLootableContainer || Beliefs.LootableContainer == null) return;

        if (!HasReachedDestination()) return;
        TryLootItemsInContainer();
    }

    public override void Exit()
    {
        stateMachine.Agent.updateRotation = true;
        stateMachine.Agent.isStopped = false;
    }

    private void TryLootItemsInContainer()
    {
        if (Beliefs.LootableContainer.interactable is ILootContainer lootable)
        {
            for (int i = 0; i < lootable.ItemContainer.Items.Length; i++)
            {
                InventoryService.TryResolveSlotInteraction(lootable.ItemContainer, i, stateMachine.InventoryHandler.ItemContainer, -1, true);
            }
        }

        Beliefs.LootableContainer.MarkAsAlreadyLooted();
        
        foreach (InteractContext interactContext in stateMachine.NpcPerception.interactables)
        {
            if (interactContext.interactable != Beliefs.LootableContainer.interactable) continue;
            interactContext.MarkAsAlreadyLooted();
            break;
        }
    }
}
