using UnityEngine;

/// <summary>
/// class is a simulation for player/npc inventory/equipment/stats handlers to test and debug inventory system and ui elements 
/// without needing to fully implement player or npc or having them fully set up in scene.
/// </summary>

[RequireComponent(typeof(StatsHandler))]
[RequireComponent(typeof(InventoryHandler))]
[RequireComponent(typeof(EquipmentHandler))]
public class TestInventoryController : MonoBehaviour
{
    public EntityDefinition Definition;
    public NPCSpawner.Teams team;

    private StatsHandler statsHandler;
    private InventoryHandler inventoryHandler;
    private EquipmentHandler equipmentHandler;

    private void Awake()
    {
        statsHandler = GetComponent<StatsHandler>();
        inventoryHandler = GetComponent<InventoryHandler>();
        equipmentHandler = GetComponent<EquipmentHandler>();
    }

    private void Start()
    {
        statsHandler.InitializeStats(team, Definition);
        inventoryHandler.InitializeInventoryHandler();
        equipmentHandler.InitializeEquipmentHandler(Definition);
    }
}
