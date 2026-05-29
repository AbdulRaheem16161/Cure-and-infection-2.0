using System;
using UnityEngine;

/// <summary>
/// class is a simulation for a GameManager or similar to hold references to player + somewhere to put ui events for testing and debugging inventory system 
/// and ui elements ui events will when proper ui classes and systems made, be moved to more appropriate locations, but the core structure and idea of how 
/// events are called and how ui listens to them will likely remain the same for passing context to ui elements through events. example: when player interacts 
/// with corpse or container, call event with object reference to lootable and open bool, ui listens to event and opens/closes ui panels, grabs relevant info 
/// it needs off of Gameobject lootable. (EquipmentHandler, ItemContainer components)
/// </summary>

public class TestInventoryManager : MonoBehaviour
{
	public static TestInventoryManager Instance;

	public GameObject playerObj;
	public GameObject npcObj;

    public static bool PlayerInventoryVisible { get; private set; }
    public static bool LootableInventoryVisible { get; private set; }

    public static event Action<bool, bool> PlayerInventoryVisibleEvent;
    public static event Action<GameObject, bool> LootableInventoryVisibleEvent;

    private void Awake()
	{
		Instance = this;
        PlayerInventoryVisible = false;
        LootableInventoryVisible = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            TogglePlayerEquipmentAndInventory();
    }

    public static void TogglePlayerEquipmentAndInventory()
    {
        PlayerInventoryVisible = !PlayerInventoryVisible;
        PlayerInventoryVisibleEvent?.Invoke(PlayerInventoryVisible, true);
    }

    public static void LootCorpse(GameObject lootable, bool open)
    {
        PlayerInventoryVisible = open;
        PlayerInventoryVisibleEvent?.Invoke(open, false);

        LootableInventoryVisible = open;
        LootableInventoryVisibleEvent?.Invoke(lootable, open);
    }

    /// <summary>
    /// move to a UiManager of some sort that instead subs to a event in LootableContainer script: 
    /// public static event Action<gameobject, bool> OnLootableContainerInteract;
    /// UiManager then tells what sub ui elements to and has a ref to them like: public InventoryUi playerInventoryUi;
    /// that gets called like PlayerInventoryUi.ShowInventory(lootable) 
    /// merge methods public void UpdateObjectReferences(GameObject newRef) and public void ShowInventory()) to make ui elements simpler
    /// </summary>

    public static void LootContainer(GameObject lootable, bool open)
    {
        PlayerInventoryVisible = open;
        PlayerInventoryVisibleEvent?.Invoke(open, false);

        LootableInventoryVisible = open;
        LootableInventoryVisibleEvent?.Invoke(lootable, open);
    }
}
