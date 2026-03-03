using Game.MyNPC;
using UnityEngine;

public class NpcController : MonoBehaviour
{
	public NpcDefinition NpcDefinition;
	public StatsHandler StatsHandler;
	public NPCStateMachine StateMachine;
	public InventoryHandler InventoryHandler;
	public EquipmentHandler EquipmentHandler;
	public DetectionCone DetectionCone;
	public DetectionRadius DetectionRadius;

	private void Awake()
	{
		StatsHandler = GetComponent<StatsHandler>();
		if (StatsHandler == null)
			Debug.LogError($"StatsHandler component null on {gameObject.name}");

		StateMachine = GetComponent<NPCStateMachine>();
		if (StateMachine == null)
			Debug.LogError($"NPCStateMachine component null on {gameObject.name}");

		InventoryHandler = GetComponent<InventoryHandler>();
		if (InventoryHandler == null)
			Debug.LogError($"InventoryHandler component null on {gameObject.name}");

		EquipmentHandler = GetComponent<EquipmentHandler>();
		if (EquipmentHandler == null)
			Debug.LogError($"EquipmentHandler component null on {gameObject.name}");

		DetectionCone = GetComponent<DetectionCone>();
		if (DetectionCone == null)
			Debug.LogError($"DetectionCone component null on {gameObject.name}");

		DetectionRadius = GetComponent<DetectionRadius>();
		if (DetectionRadius == null)
			Debug.LogError($"DetectionRadius component null on {gameObject.name}");
	}

	public void InitilizeNpc(NpcDefinition npcDefinition)
	{
		NpcDefinition = npcDefinition;
		StatsHandler.InitilizeStats(npcDefinition);
		StateMachine.InitilizeStateMachine(this);
		InventoryHandler.InitilizeInventoryHandler();
		EquipmentHandler.InitilizeEquipmentHandler();
		DetectionCone.Initilize(this);
		DetectionRadius.Initilize(this);
	}
}
