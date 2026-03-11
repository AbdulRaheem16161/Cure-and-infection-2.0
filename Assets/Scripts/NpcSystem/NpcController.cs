using Game.MyNPC;
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
	public NPCStateMachine StateMachine { get; private set; }
	public InventoryHandler InventoryHandler { get; private set; }
	public EquipmentHandler EquipmentHandler { get; private set; }
	public NpcPerception NpcPerception { get; private set; }

	private void Awake()
	{
		AssignScriptReferences();

		if (NpcDefinition != null && !_initialized)
			InitializeNpc(NpcDefinition, Teams.FreeFighter); //set default team to freedom fighter
	}

	private void AssignScriptReferences()
	{
		StatsHandler = GetComponent<StatsHandler>();
		StateMachine = GetComponent<NPCStateMachine>();
		InventoryHandler = GetComponent<InventoryHandler>();
		EquipmentHandler = GetComponent<EquipmentHandler>();
		NpcPerception = GetComponent<NpcPerception>();
	}

	public void InitializeNpc(NpcDefinition npcDefinition, Teams team)
	{
		NpcDefinition = npcDefinition;
		StatsHandler.InitializeStats(EquipmentHandler, NpcDefinition);
		InventoryHandler.InitializeInventoryHandler(EquipmentHandler);
		EquipmentHandler.InitializeEquipmentHandler(InventoryHandler, NpcDefinition);
		StateMachine.InitializeStateMachine(StatsHandler, EquipmentHandler, InventoryHandler, NpcPerception, NpcDefinition, team);
		NpcPerception.Initialize(NpcDefinition, StateMachine);

		gameObject.name = NpcDefinition.NpcName;
		_initialized = true;
	}
}
