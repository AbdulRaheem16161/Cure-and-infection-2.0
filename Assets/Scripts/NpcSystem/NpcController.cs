using Game.MyNPC;
using UnityEngine;

[RequireComponent(typeof(StatsHandler))]
[RequireComponent(typeof(NPCStateMachine))]
[RequireComponent(typeof(InventoryHandler))]
[RequireComponent(typeof(EquipmentHandler))]
[RequireComponent(typeof(DetectionCone))]
[RequireComponent(typeof(DetectionRadius))]
public class NpcController : MonoBehaviour
{
	private bool _initialized = false;

	public NpcDefinition NpcDefinition;
	public StatsHandler StatsHandler { get; private set; }
	public NPCStateMachine StateMachine { get; private set; }
	public InventoryHandler InventoryHandler { get; private set; }
	public EquipmentHandler EquipmentHandler { get; private set; }
	public DetectionCone DetectionCone { get; private set; }
	public DetectionRadius DetectionRadius { get; private set; }

	private void Awake()
	{
		AssignScriptReferences();

		if (NpcDefinition != null && !_initialized)
			InitializeNpc(NpcDefinition);
	}

	private void AssignScriptReferences()
	{
		StatsHandler = GetComponent<StatsHandler>();
		StateMachine = GetComponent<NPCStateMachine>();
		InventoryHandler = GetComponent<InventoryHandler>();
		EquipmentHandler = GetComponent<EquipmentHandler>();
		DetectionCone = GetComponent<DetectionCone>();
		DetectionRadius = GetComponent<DetectionRadius>();
	}

	public void InitializeNpc(NpcDefinition npcDefinition)
	{
		NpcDefinition = npcDefinition;
		StatsHandler.InitializeStats(EquipmentHandler, NpcDefinition);
		StateMachine.InitializeStateMachine(StatsHandler, EquipmentHandler, NpcDefinition);
		InventoryHandler.InitializeInventoryHandler(EquipmentHandler);
		EquipmentHandler.InitializeEquipmentHandler(InventoryHandler, npcDefinition);
		DetectionCone.Initialize(NpcDefinition);
		DetectionRadius.Initialize(NpcDefinition);

		gameObject.name = NpcDefinition.NpcName;
		_initialized = true;
	}
}
