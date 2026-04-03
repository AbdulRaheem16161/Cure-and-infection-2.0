using UnityEngine;

[RequireComponent(typeof(EquipmentHandler))]
public class InventoryHandler : MonoBehaviour, IAmmoGiver
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
		EquipmentHandler.OnItemEquip += OnItemEquipped;
		EquipmentHandler.OnItemUnEquip += OnItemUnEquipped;
	}
	private void OnDisable()
	{
		EquipmentHandler.OnItemEquip -= OnItemEquipped;
		EquipmentHandler.OnItemUnEquip -= OnItemUnEquipped;
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
	private void OnItemEquipped(EquipmentSlot slot)
	{
		if (slot.Item.ItemDefinition is ArmourDefinition armourDefinition)
		{
			switch (slot.EquipmentType)
			{
				case EquipmentHandler.EquipmentType.backpack:
				itemContainer.ModifySize((int)armourDefinition.InventorySlotsProvided);
				break;
			}
		}
	}

	private void OnItemUnEquipped(EquipmentSlot slot)
	{
		if (slot.Item.ItemDefinition is ArmourDefinition armourDefinition)
		{
			switch (slot.EquipmentType)
			{
				case EquipmentHandler.EquipmentType.backpack:
				itemContainer.ModifySize(-(int)armourDefinition.InventorySlotsProvided);
				break;
			}
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

	#region item pickup (TODO handle destroying world items/leaving them if stack count not 0)
	/// <summary>
	/// add new items to inventory, by default trying to stack them
	/// </summary>
	public void AddNewItem(InventoryItem newItem, bool tryStack = true)
	{
		itemContainer.AddNewItem(newItem, tryStack);

		//destroy world item
	}
	#endregion

	#region move items to specific slot methods
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

	#region dropping items (TODO: update so world item is spawned)
	public void DropItem(int slot, bool dropStack)
	{
		itemContainer.DropItem(slot, dropStack);

		//spawn world item
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
