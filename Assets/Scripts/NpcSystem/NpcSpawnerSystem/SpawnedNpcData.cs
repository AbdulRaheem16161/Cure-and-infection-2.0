using System;

[Serializable]
public class SpawnedNpcData
{
    public NpcController npc;
    public NpcSpawnData npcSpawnData;

    public bool staticSpawnedNpc;
    public float squaredDistanceToPlayer;

    public SpawnedNpcData(NpcController npc, bool staticSpawnedNpc, PlayerController player, NpcSpawnData npcSpawnData)
    {
        this.npc = npc;
        this.npcSpawnData = npcSpawnData;
        this.staticSpawnedNpc = staticSpawnedNpc;
        UpdateSquaredDistanceFromPlayer(player);
    }

    public void UpdateSquaredDistanceFromPlayer(PlayerController player) //treat null player as player close
    {
        squaredDistanceToPlayer = player != null ? (npc.transform.position - player.transform.position).sqrMagnitude : 0f;
    }
}
