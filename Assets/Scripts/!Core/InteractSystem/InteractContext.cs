using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class InteractContext
{
    public string name;
    public IInteractable interactable;
    public Collider collider;
    public LootState lootState;
    public float squaredDistance;

    public enum LootState
    {
        unSet, notLootable, LootableButBlocked, Lootable, alreadyLooted
    }

    public InteractContext(IInteractable interactable, Collider collider, Vector3 npcPosition)
    {
        name = collider.name;
        this.interactable = interactable;
        this.collider = collider;
        lootState = LootState.unSet;
        CanBeLooted();
        UpdateDistance(npcPosition);
    }

    public bool CheckIfAlreadyLooted(InteractContext interact)
    {
        return this == interact;
    }

    public bool CanBeLooted()
    {
        if (lootState == LootState.notLootable || lootState == LootState.alreadyLooted) return false;

        if (interactable is ILootContainer loot)
        {
            if (loot.CanLoot)
                lootState = LootState.Lootable;
            else
                lootState = LootState.LootableButBlocked;
        }
        else
            lootState = LootState.notLootable;

        return lootState == LootState.Lootable;
    }

    public void MarkAsAlreadyLooted()
    {
        lootState = LootState.alreadyLooted;
    }

    public void UpdateDistance(Vector3 currentPosition)
    {
        squaredDistance = (currentPosition - collider.transform.position).sqrMagnitude;
    }
}
