using UnityEngine;
using static ILootContainer;

[RequireComponent(typeof(StatsHandler))]
[RequireComponent(typeof(EquipmentHandler))]
public class InventoryHandler : MonoBehaviour, IInteractable, ILootContainer
{
    public StatsHandler StatsHandler { get; private set; }
    public EquipmentHandler EquipmentHandler { get; private set; }
	private bool _Initialized = false;

	#region inventory settings
	[Header("Inventory Settings")]
	[SerializeField] private int money;
	[SerializeField] private int initialInventorySize;

    public bool CanInteract => CanLoot;
    public string InteractableName => $"Loot {ContainerName}";

    public string ContainerName => $"{StatsHandler.Definition.Name} Inventory";
    public bool CanLoot => StatsHandler.LifeState == EntityDefinition.LifeState.dead;
    public LootSpawningState lootSpawningState { get; set; }

    [SerializeField] private ItemContainer itemContainer;
    public ItemContainer ItemContainer => itemContainer;
    #endregion

    #region inventory readonly settings
    public int Money => money;
    #endregion

    #region debug settings
    [Header("Debug Settings")]
	[HideInInspector] public int addMoney;
	[HideInInspector] public int modifyInventorySizeByThis;
	[HideInInspector] public bool actionEffectsStack = false;
	[HideInInspector] public int slotIndex = 0;
	[HideInInspector] public int newSlotIndex = 0;
	[HideInInspector] public ItemDefinition itemToSpawn;
	[HideInInspector] public int itemCount;
	#endregion

	#region awake + initialize inventory handler method
	private void Awake()
	{
		StatsHandler = GetComponent<StatsHandler>();
		EquipmentHandler = GetComponent<EquipmentHandler>();
		lootSpawningState = LootSpawningState.lootSpawned;

		if (!_Initialized)
			InitializeInventoryHandler();
	}
	public void InitializeInventoryHandler()
	{
		_Initialized = true;
		itemContainer = new(initialInventorySize);
	}
	#endregion

	#region event subbing/unsubbing
	private void OnEnable()
	{
		EquipmentHandler.OnEquippedItemChanges += OnEquippedItemChanges;
	}
	private void OnDisable()
	{
		EquipmentHandler.OnEquippedItemChanges -= OnEquippedItemChanges;
	}
	#endregion

	#region modifying money
	public bool HasEnoughMoney(int cost)
	{
		if (cost > money)
			return false;
		else return true;
	}
	public void SetMoney(int moneyToSet)
	{
		money = moneyToSet;
	}
	public void AddMoney(int moneyToAdd)
	{
		money += moneyToAdd;
	}
	public void RemoveMoney(int moneyToRemove)
	{
		money -= moneyToRemove;
	}
	#endregion

	#region item equipment events
	private void OnEquippedItemChanges(EquipmentSlot slot, bool wasEquipped)
	{
		if (slot.Item.ItemDefinition is not ArmourDefinition armourDefinition) return;

		static int GetInventorySizeModifier(int inventorySizeModifier, bool wasEquipped)
		{
			return wasEquipped ? inventorySizeModifier : -inventorySizeModifier;
		}

		switch (slot.EquipmentType)
		{
			case EquipmentHandler.EquipmentType.backpack:
			InventoryService.ModifyContainerSize(itemContainer, 
				GetInventorySizeModifier(armourDefinition.InventorySlotsProvided, wasEquipped), transform.position);
			break;
		}
	}
	#endregion

	#region inventory interact interface methods
	public void InteractPress(Interactor interactor)
    {
		if (!CanLoot) return;

		//open both this inventory + interactor.Inventory in ui
		UiManager.ShowScreen(new(UiManager.UiScreens.LootableInventory, GameManager.Instance.PlayerReference, gameObject, EquipmentHandler, this));
		return;
    }

    public void InteractHoldComplete(Interactor interactor)
    {
        if (!CanLoot) return;
        return;
    }
    #endregion
}
