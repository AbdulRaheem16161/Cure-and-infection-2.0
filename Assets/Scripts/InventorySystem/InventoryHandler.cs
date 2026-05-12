using UnityEngine;

[RequireComponent(typeof(EquipmentHandler))]
public class InventoryHandler : MonoBehaviour, IAmmoGiver, IInteractable
{
	public EquipmentHandler EquipmentHandler { get; private set; }
	private bool _Initialized = false;

	#region inventory settings
	[Header("Inventory Settings")]
	[SerializeField] private int money;
	[SerializeField] private int initialInventorySize;
	[SerializeField] private ItemContainer itemContainer;
	#endregion

    #region inventory readonly settings
	public int Money => money;
	public ItemContainer ItemContainer => itemContainer;
    #endregion

    #region debug settings
    [Header("Debug Settings")]
	[HideInInspector] public int addMoney;
	[HideInInspector] public int modifyInventorySizeByThis;
	[HideInInspector] public bool actionEffectsStack = false;
	[HideInInspector] public int slotIndex = 0;
	[HideInInspector] public ItemDefinition itemToSpawn;
	[HideInInspector] public int itemToSpawnCount;
	#endregion

	#region awake + initialize inventory handler method
	private void Awake()
	{
		EquipmentHandler = GetComponent<EquipmentHandler>();

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
			itemContainer.ModifySize(GetInventorySizeModifier(armourDefinition.InventorySlotsProvided, wasEquipped), transform.position);
			break;
		}
	}
	#endregion

	#region ammo container interface methods
	public int GetAmmo(ProjectileDefinition projectileDefinition, int amountNeeded)
	{
		return ItemContainer.GetAmmo(projectileDefinition, amountNeeded);
	}
	public int TakeAmmo(ProjectileDefinition projectileDefinition, int amountNeeded)
	{
		return ItemContainer.TakeAmmo(projectileDefinition, amountNeeded);
	}
	public bool AmmoAvailable(ProjectileDefinition projectileDefinition)
	{
		return itemContainer.AmmoAvailable(projectileDefinition);
	}
	#endregion

	#region inventory interact interface methods (TODO: make them actually open inventories)
	public void InteractPress(Interactor interactor)
    {
		//open both this inventory + interactor.Inventory in ui
		Debug.LogError("Needs implementation");
		return;
    }

    public void InteractHoldComplete(Interactor interactor)
    {
		return;
    }
    #endregion

    #region adding new item
    /// <summary>
    /// add new items to inventory, by default trying to stack them
    /// </summary>
    public void AddNewItem(InventoryItem newItem, bool tryStack = true)
	{
		itemContainer.AddNewItem(newItem, tryStack);
	}
	#endregion

	#region moving items in slots
	public void SwapItemsInSlots(int currentSlot, int newSlot)
	{
		itemContainer.SwapItemsInSlots(currentSlot, newSlot);
	}
	#endregion

	#region splitting items
	public void SplitItem(int slot)
	{
		itemContainer.SplitItem(slot);
	}
	#endregion

	#region dropping items
	public void DropItem(int slot, bool dropStack)
	{
		itemContainer.DropItem(slot, dropStack, transform.position);
	}
	#endregion

	#region removing items
	public void RemoveItemsFromSlot(int slot, int stackToRemove)
	{
		itemContainer.RemoveItemsFromSlot(slot, stackToRemove);
	}
	#endregion

	#region buying/selling items
	public void BuyItemInSlot(InventoryHandler otherInventory, int slot, bool buyingStack)
	{
		itemContainer.BuyItemInSlot(this, otherInventory, slot, buyingStack);
	}
	public void SellItemInSlot(InventoryHandler otherInventory, int slot, bool sellingStack)
	{
		itemContainer.SellItemInSlot(this, otherInventory, slot, sellingStack);
	}
	#endregion

	#region reset inventory
	public void ResetContainer()
	{
		itemContainer.ResetContainer();
	}
    #endregion
}
