using UnityEngine;
using static EquipmentHandler;

public static class InventoryService
{
    public class ShopTransferContext
    {
        public InventoryHandler seller;
        public InventoryHandler buyer;
        public InventoryItem item;
        public int slot;
        public int stackCount;
        public int price;
        public bool transferStack;
    }

    public enum ShopTransferResult
    {
        fullInventory, itemNull, noMoney, success
    }

    public enum EquipItemResult
    {
        fullInventory, itemNull, invalidEquipmentType, success
    }

    #region Modify Container Size
    public static void ModifyContainerSize(ItemContainer container, int sizeAdjustment, Vector3 dropPosition)
    {
        int newSize = container.Items.Length + sizeAdjustment;

        if (newSize < 0)
        {
            Debug.LogError("Container size cannot be negative.");
            return;
        }

        for (int i = newSize; i < container.Items.Length; i++) //drop items on floor if they dont fit
        {
            if (container.InventoryItemExists(container.Items[i]))
            {
                Debug.LogWarning($"Item {container.Items[i].ItemDefinition.ItemName} was dropped on the ground");
                DropItem(dropPosition, container, i, true);
            }
        }

        container.SetContainerSize(newSize);
    }
    #endregion

    #region Buy/Sell Items
    public static void BuyItemInSlot(InventoryHandler seller, InventoryHandler buyer, int slot, bool transferStack)
    {
        ShopTransferResult result = ShopTransferValid(seller, buyer, slot, transferStack, out ShopTransferContext transferContext);

        if (result == ShopTransferResult.success)
        {
            ProcessTransaction(transferContext);
            Debug.Log($"brought item for: {transferContext.price}"); return;
        }

        switch (result)
        {
            case ShopTransferResult.fullInventory:
                Debug.LogWarning("Cannot buy Item, buyers inventory full."); return;
            case ShopTransferResult.itemNull:
                Debug.LogWarning("Cannot buy Item, no item exists in sellers inventory slot."); return;
            case ShopTransferResult.noMoney:
                Debug.Log($"Cannot buy Item, buyer has insufficient funds: {transferContext.buyer.Money}/{transferContext.price}"); return;
        }
    }
    public static void SellItemInSlot(InventoryHandler seller, InventoryHandler buyer, int slot, bool transferStack)
    {
        ShopTransferResult result = ShopTransferValid(seller, buyer, slot, transferStack, out ShopTransferContext transferContext);

        if (result == ShopTransferResult.success)
        {
            ProcessTransaction(transferContext);
            Debug.Log($"Sold item for: {transferContext.price}"); return;
        }

        switch (result)
        {
            case ShopTransferResult.fullInventory:
                Debug.LogWarning("Cannot sell Item, buyers inventory full."); return;
            case ShopTransferResult.itemNull:
                Debug.LogWarning("Cannot sell Item, no item exists in sellers inventory slot."); return;
            case ShopTransferResult.noMoney:
                Debug.Log($"Cannot sell Item, buyer has insufficient funds: {transferContext.buyer.Money}/{transferContext.price}"); return;
        }
    }
    private static void ProcessTransaction(ShopTransferContext shopTransferData)
    {
        shopTransferData.buyer.RemoveMoney(shopTransferData.price);
        shopTransferData.seller.AddMoney(shopTransferData.price);

        shopTransferData.buyer.ItemContainer.AddNewItem(new(shopTransferData.item.ItemDefinition, shopTransferData.stackCount));
        shopTransferData.seller.ItemContainer.RemoveItemsFromSlot(shopTransferData.slot, shopTransferData.stackCount, shopTransferData.transferStack);
    }
    private static ShopTransferResult ShopTransferValid(
        InventoryHandler seller, InventoryHandler buyer, int slot, bool transferStack, out ShopTransferContext data)
    {
        data = null;

        if (buyer.ItemContainer.ContainerFull()) { return ShopTransferResult.fullInventory; }

        InventoryItem item = seller.ItemContainer.Items[slot];

        if (!seller.ItemContainer.InventoryItemExists(item)) { return ShopTransferResult.itemNull; }

        int stackCount = transferStack ? item.CurrentStack : 1;
        int price = item.ItemDefinition.ItemPrice * stackCount;

        data = new()
        {
            seller = seller,
            buyer = buyer,
            item = item,
            slot = slot,
            stackCount = stackCount,
            price = price,
            transferStack = transferStack
        };

        if (!buyer.HasEnoughMoney(price)) { return ShopTransferResult.noMoney; }

        return ShopTransferResult.success;
    }
    #endregion

    #region Equipping/Unequipping Item
    public static void TryEquipItem(EquipmentHandler equipmentHandler, ItemContainer inventory, int slot, EquipmentType equipmentType)
    {
        if (!inventory.SlotExists(slot) || !inventory.InventoryItemExists(inventory.Items[slot]))
        {
            Debug.LogError($"no item exists in slot {slot}");
            return;
        }
    }
    public static EquipItemResult EquipItemValid(EquipmentHandler equipmentHandler, InventoryItem item, EquipmentType equipmentType)
    {
        if (!equipmentHandler.CanEquipItem(item, equipmentType)) return EquipItemResult.invalidEquipmentType;
        return EquipItemResult.success;
    }
    #endregion

    #region Try Resolve Slot Interactions
    public static void TryResolveSlotInteraction(
        ItemContainer source, int sourceSlot, ItemContainer destination, int destinationSlot = -1, bool logOutcome = false)
    {
        if (SlotIndexOutOfBounds(destination, destinationSlot) || SlotIndexOutOfBounds(source, sourceSlot))
        {
            if (logOutcome) Debug.LogError($"Cannot interact with slots, one or more slot indices out of bounds for their respective container.");
            return;
        }

        if (destinationSlot >= 0)
            ResolveSlotInteraction(source, sourceSlot, destination, destinationSlot, logOutcome);
        else
            ResolveAutoSlotInteraction(source, sourceSlot, destination, logOutcome);

    }
    #endregion

    #region Resolve Slot To Slot Interaction
    private static void ResolveSlotInteraction(ItemContainer source, int sourceSlot, ItemContainer destination, int destinationSlot = -1, bool logOutcome = false)
    {
        InventoryItem itemA = source.Items[sourceSlot];
        InventoryItem itemB = destination.Items[destinationSlot];

        if (CanMergeItems(itemA, itemB)) //merge stacks to destination, null source slot if all merged
        {
            itemB = destination.StackItem(destinationSlot, itemA);
            source.SetItemInSlot(itemB.CurrentStack > 0 ? itemB : null, sourceSlot);
        }
        else if (!itemA.ItemDefinitionNull) //move itemA, if itemB exists move to source, if not null source (itemB didnt exist)
        {
            destination.SetItemInSlot(itemA, destinationSlot);

            if (!itemB.ItemDefinitionNull)
                source.SetItemInSlot(itemB, sourceSlot);
            else
                source.SetItemInSlot(null, sourceSlot);
        }
        else
        {
            if (logOutcome) Debug.LogError($"Failed");
        }
    }
    #endregion

    #region Resolve Auto Slot Interaction
    private static void ResolveAutoSlotInteraction(ItemContainer source, int sourceSlot, ItemContainer destination, bool logOutcome = false)
    {
        InventoryItem item = source.Items[sourceSlot];

        item = destination.TryStackItem(item);

        if (item.CurrentStack > 0) //if we have any left that couldnt be stacked, try to add to an empty slot
        {
            if (destination.ContainerFull())
            {
                if (logOutcome) Debug.LogWarning($"Could not move item, destination container full.");
                return;
            }
            destination.AddNewItem(item);
        }
    }
    #endregion

    #region Helper Checks
    private static bool SlotIndexOutOfBounds(ItemContainer container, int slot)
    {
        return container.Items.Length <= slot || slot < 0;
    }

    private static bool CanMergeItems(InventoryItem itemA, InventoryItem itemB)
    {
        if (itemA == null || itemB == null) return false;
        if (itemA.ItemDefinitionNull || itemB.ItemDefinitionNull) return false;
        if (!itemA.CanStackWith(itemB)) return false;
        return true;
    }
    #endregion

    #region Drop Item
    public static void DropItem(Vector3 dropPosition, ItemContainer container, int slot, bool dropStack)
    {
        if (!container.SlotExists(slot) || !container.InventoryItemExists(container.Items[slot]))
        {
            Debug.LogError($"no item exists in slot {slot}");
            return;
        }

        InventoryItem itemToDrop = container.Items[slot];

        container.RemoveItemsFromSlot(slot, itemToDrop.CurrentStack, dropStack);

        ItemSpawner.GetItem(itemToDrop.ItemDefinition, itemToDrop.CurrentStack, null, dropPosition, Quaternion.identity);
    }
    #endregion
}
