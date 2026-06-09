using UnityEngine;
using static UiManager;

public class UiContext
{
    public UiScreens uiScreen;

    public GameObject playerRef;
    public EquipmentHandler playerEquipment;
    public ItemContainer playerContainer;

    public GameObject otherRef;
    public EquipmentHandler otherEquipment;
    public ItemContainer otherContainer;

    public UiContext(UiScreens uiScreen)
    {
        this.uiScreen = uiScreen;
    }

    public UiContext(UiScreens uiScreen, PlayerController player)
    {
        this.uiScreen = uiScreen;
        playerRef = player.gameObject;
        playerEquipment = player.EquipmentHandler;
        playerContainer = player.InventoryHandler.ItemContainer;
    }

    public UiContext(UiScreens uiScreen, PlayerController player, GameObject obj, EquipmentHandler equipmentHandler, ILootContainer lootContainer)
    {
        this.uiScreen = uiScreen;
        playerRef = player.gameObject;
        playerEquipment = player.EquipmentHandler;
        playerContainer = player.InventoryHandler.ItemContainer;
        otherRef = obj;
        otherEquipment = equipmentHandler;
        otherContainer = lootContainer.ItemContainer;
    }
}
