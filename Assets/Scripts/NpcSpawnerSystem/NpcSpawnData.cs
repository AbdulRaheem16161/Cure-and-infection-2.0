using System;
using UnityEngine;
using static Game.MyNPC.NPCStateMachine;
using static NPCSpawner;

[Serializable]
public class NpcSpawnData
{
	[Tooltip("Npc definition to spawn")]
	public EntityDefinition Definition;

	[Tooltip("Spawned Npcs team")]
	public Teams team;

	[Tooltip("Spawned Npcs movement type. NPC handles fallback: Patrol > RandomArea > Random")]
	public MovementType movementType;

	[Tooltip("Spawned patrol path, null = randomly chosen")]
	public PatrolPathManager patrolPath;

	[Tooltip("Spawned Npcs spawn point, can use spawn point or patrol points to spawn on, null = spawner location")]
	public Transform spawnPoint;
	public Vector3 SpawnPosition { get; private set; }

	[Tooltip("Spawns Npc as Invincible")]
	public bool forceInvincible;

	[Tooltip("Spawns Npc as alread dead")]
	public bool forceDeath;

	private readonly System.Random systemRandom = new();

	public NpcSpawnData(EntityDefinition definition, MovementType movementType, Vector3 spawnPosition)
	{
		Definition = definition;
		team = AssignRandomTeam();
		this.movementType = movementType;
		patrolPath = null;
		spawnPoint = null;
		SpawnPosition = spawnPosition;
		forceInvincible = false;
		forceDeath = false;
	}
	public NpcSpawnData(EntityDefinition definition, Teams team, MovementType movementType, PatrolPathManager patrolPath, Vector3 spawnPosition)
	{
		Definition = definition;

		if (IsZombie())
			this.team = Teams.Zombie;
		else
			this.team = team;

		this.movementType = movementType;
		this.patrolPath = patrolPath;
		spawnPoint = null;
		SpawnPosition = spawnPosition;
		forceInvincible = false;
		forceDeath = false;
	}
	private Teams AssignRandomTeam()
	{
		if (IsZombie())
			return Teams.Zombie;

		Array teams = Enum.GetValues(typeof(Teams));
		return (Teams)teams.GetValue(systemRandom.Next(teams.Length));
	}
	private bool IsZombie()
	{
		if (Definition.Flags.HasFlag(EntityDefinition.EntityFlags.canBecomeZombie)) //cant become zombie so is zombie team
			return false;
		else return true;
	}
}
