using Game.MyNPC;
using System;
using UnityEngine;
using static NPCSpawner;

[RequireComponent(typeof(StatsHandler))]
[RequireComponent(typeof(NPCStateMachine))]
[RequireComponent(typeof(InventoryHandler))]
[RequireComponent(typeof(EquipmentHandler))]
[RequireComponent(typeof(NpcPerception))]
public class NpcController : MonoBehaviour
{
	private bool _initialized = false;

	public NpcDefinition NpcDefinition;

	public StatsHandler StatsHandler { get; private set; }
	public NpcBeliefs Beliefs { get; private set; }
	public NPCStateMachine StateMachine { get; private set; }
	public InventoryHandler InventoryHandler { get; private set; }
	public EquipmentHandler EquipmentHandler { get; private set; }
	public NpcPerception NpcPerception { get; private set; }

	private void Awake()
	{
		StatsHandler = GetComponent<StatsHandler>();
		Beliefs = GetComponent<NpcBeliefs>();
		StateMachine = GetComponent<NPCStateMachine>();
		InventoryHandler = GetComponent<InventoryHandler>();
		EquipmentHandler = GetComponent<EquipmentHandler>();
		NpcPerception = GetComponent<NpcPerception>();
	}
	private void Start()
	{
		if (!_initialized)
		{
			if (NpcDefinition != null)
				InitializeNpc(NpcDefinition, StatsHandler.Team); //keep current team
			else
				Debug.LogError($"{typeof(NpcDefinition)} null, assign reference in inspector when not using a NpcSpawner");
		}
	}

	public void InitializeNpc(NpcDefinition npcDefinition, Teams team)
	{
		if (npcDefinition == null)
		{
			Debug.LogError($"{typeof(NpcDefinition)} null, NpcSpawner failed to assign definition");
			return;
		}

		NpcDefinition = npcDefinition;
		gameObject.name = NpcDefinition.NpcName;

		StatsHandler.InitializeStats(team, NpcDefinition);
		InventoryHandler.InitializeInventoryHandler();
		EquipmentHandler.InitializeEquipmentHandler(NpcDefinition);
		Beliefs.InitializeBeliefs(NpcDefinition);
		StateMachine.InitializeStateMachine(NpcDefinition);
		NpcPerception.Initialize(NpcDefinition);
		_initialized = true;
	}
}
