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

    #region Modify Container Size
    public static void ModifyContainerSize(ItemContainer container, int sizeAdjustment, Vector3 dropPosition)
    {
        int newSize = container.Items.Count + sizeAdjustment;

        if (newSize < 0)
        {
            Debug.LogError("Container size cannot be negative.");
            return;
        }

        for (int i = newSize; i < container.Items.Count; i++) //drop items on floor if they dont fit
        {
            if (ItemExists(container.Items[i]))
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
        shopTransferData.seller.ItemContainer.RemoveItem(shopTransferData.slot, shopTransferData.stackCount);
    }
    private static ShopTransferResult ShopTransferValid(
        InventoryHandler seller, InventoryHandler buyer, int slot, bool transferStack, out ShopTransferContext data)
    {
        data = null;

        InventoryItem item = seller.ItemContainer.Items[slot];

        if (buyer.ItemContainer.ContainerCanAcceptItem(item)) { return ShopTransferResult.fullInventory; }
        if (!ItemExists(item)) { return ShopTransferResult.itemNull; }

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

    /// <summary>
    /// currently dragging equipped item to inventory slot doesnt handle item stacking correctly as u logic is set up to always stack into an equipment slot
    /// its fine like this for now but does need to have a bool or something that flips the stacking target to inventory when dragging equipment to inventory
    /// </summary>

    #region Try Resolve Equipping/Unequipping Item
    public static void TryResolveSlotEquipping(
        EquipmentHandler equipmentHandler, EquipmentType equipmentType, ItemContainer inventory, int slot = -1, bool logOutcome = false)
    {
        if (!equipmentHandler.GetMatchingEquipmentSlot(equipmentType, out EquipmentSlot equipmentSlot)) return;

        if (slot != -1 && SlotIndexOutOfBounds(inventory, slot))
        {
            if (logOutcome) Debug.LogError($"Cannot interact with inventory slot, indices out of bounds for container.");
            return;
        }

        ResolveSlotEquipping(equipmentHandler, equipmentSlot, inventory, slot, logOutcome);
    }
    #endregion

    #region Resolve Slot To Slot Equipping
    public static void ResolveSlotEquipping(
        EquipmentHandler equipmentHandler, EquipmentSlot equipmentSlot, ItemContainer inventory, int slot = -1, bool logOutcome = false)
    {
        InventoryItem itemToEquip = null;
        if (slot != -1)
            itemToEquip = inventory.Items[slot];

        InventoryItem itemToUnequp = equipmentSlot.Item;

        bool itemToEquipExists = ItemExists(itemToEquip);
        bool itemToUnEquipExists = ItemExists(itemToUnequp);

        if (!itemToEquipExists && !itemToUnEquipExists)
        {
            Debug.LogError($"Both item to equip and item to unequip dont exist, atleast one of them needs to.");
            return;
        }

        if (itemToEquipExists && !equipmentHandler.EquipmentSlotMatchesAllowed(equipmentSlot, itemToEquip))
        {
            if (logOutcome) Debug.LogError($"{equipmentSlot}'s EquipmentType: {equipmentSlot.EquipmentType} doesnt match items allowed type.");
            return;
        }

        if (CanMergeItems(itemToEquip, itemToUnequp)) //merge stacks to destination, null source slot if all merged
        {
            itemToUnequp = StackEquipmentItem(equipmentSlot, slot, itemToEquip);
            inventory.SetSlotContents(itemToUnequp.CurrentStack > 0 ? itemToUnequp : null, slot);
            equipmentHandler.InvokeEquippedItemChanges(equipmentSlot, true);
            return;
        }

        if (itemToEquipExists && !itemToUnEquipExists)
        {
            equipmentHandler.HandleItemEquipping(equipmentSlot, itemToEquip);
            inventory.SetSlotContents(null, slot);
        }
        else if (!itemToEquipExists && itemToUnEquipExists)
        {
            if (!inventory.ContainerCanAcceptItem(itemToUnequp))
            {
                if (logOutcome) Debug.LogError($"Cannot unequip item to inventory, container cannot accept item.");
                return;
            }

            equipmentHandler.HandleItemUnequipping(equipmentSlot);

            if (slot == -1)
                inventory.AddNewItem(itemToUnequp);
            else
                inventory.SetSlotContents(itemToUnequp, slot);
        }
        else //both exist, swap them
        {
            equipmentHandler.HandleItemUnequipping(equipmentSlot);
            equipmentHandler.HandleItemEquipping(equipmentSlot, itemToEquip);
            inventory.SetSlotContents(itemToUnequp, slot);
        }
    }
    #endregion

    #region Try Resolve Slot Interactions
    public static void TryResolveSlotInteraction(
        ItemContainer source, int sourceSlot, ItemContainer destination, int destinationSlot = -1, bool logOutcome = false)
    {
        bool sourceInvalid = SlotIndexOutOfBounds(source, sourceSlot);
        bool destinationInvalid = destinationSlot != -1 && SlotIndexOutOfBounds(destination, destinationSlot);

        if (sourceInvalid || destinationInvalid)
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
            itemB = StackContainerItem(destination, destinationSlot, itemA);
            source.SetSlotContents(itemB.CurrentStack > 0 ? itemB : null, sourceSlot);
        }
        else if (ItemExists(itemA)) //move itemA, if itemB exists move to source, if not null source (itemB didnt exist)
        {
            destination.SetSlotContents(itemA, destinationSlot);

            if (ItemExists(itemB))
                source.SetSlotContents(itemB, sourceSlot);
            else
                source.SetSlotContents(null, sourceSlot);
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
        item = destination.AddNewItem(item);
    }
    #endregion

    #region Item Stacking Helpers
    public static InventoryItem TryStackItem(ItemContainer container, InventoryItem itemToStack)
    {
        for (int i = 0; i < container.Items.Count; i++)
        {
            if (ItemExists(container.Items[i]))
            {
                if (!CanMergeItems(container.Items[i], itemToStack)) continue;
                itemToStack = StackContainerItem(container, i, itemToStack);
            }
        }
        return itemToStack;
    }
    public static InventoryItem StackContainerItem(ItemContainer container, int slot, InventoryItem itemToSack)
    {
        InventoryItem existingItem = container.Items[slot];
        if (existingItem.CurrentStack == existingItem.ItemDefinition.StackLimit) return itemToSack;

        StackItem(existingItem, itemToSack);
        container.InvokeContainerItemChanged(slot, existingItem);
        return itemToSack;
    }
    public static InventoryItem StackEquipmentItem(EquipmentSlot equipmentSlot, int slot, InventoryItem itemToSack)
    {
        InventoryItem existingItem = equipmentSlot.Item;
        if (existingItem.CurrentStack == existingItem.ItemDefinition.StackLimit) return itemToSack;

        StackItem(existingItem, itemToSack);
        return itemToSack;
    }
    private static InventoryItem StackItem(InventoryItem existingItem, InventoryItem itemToSack)
    {
        int newStackCount = existingItem.CurrentStack + itemToSack.CurrentStack;

        if (newStackCount > existingItem.ItemDefinition.StackLimit) //handle stacking overflow
        {
            existingItem.SetItemStack(existingItem.ItemDefinition.StackLimit); //set max stack limit
            newStackCount -= existingItem.ItemDefinition.StackLimit; //carry overflow
            itemToSack.SetItemStack(newStackCount); //set to overflow
        }
        else
        {
            existingItem.SetItemStack(existingItem.CurrentStack + itemToSack.CurrentStack); //add to stack
            itemToSack.SetItemStack(0); //nothing left to stack
        }

        Debug.Log($"stacked item: {existingItem.ItemDefinition.ItemName} to {existingItem.CurrentStack}");
        return itemToSack;
    }
    #endregion

    #region Helper Checks
    public static bool SlotIndexOutOfBounds(ItemContainer container, int slot)
    {
        return container.Items.Count <= slot || slot < 0;
    }

    public static bool CanMergeItems(InventoryItem itemA, InventoryItem itemB)
    {
        return ItemExists(itemA) && ItemExists(itemB) && itemA.ItemDefinition == itemB.ItemDefinition;
    }
    public static bool ItemExists(InventoryItem item)
    {
        return item != null && !item.ItemDefinitionNull;
    }
    #endregion

    #region Drop Item From Container or Equipment
    public static void DropItem(Vector3 dropPosition, ItemContainer container, int slot, bool dropStack)
    {
        if (!container.SlotExists(slot) || !ItemExists(container.Items[slot]))
        {
            Debug.LogError($"no item exists in slot {slot}");
            return;
        }

        InventoryItem itemToDrop = container.Items[slot];
        int dropAmount = dropStack ? itemToDrop.CurrentStack : 1;

        container.RemoveItem(slot, dropAmount);
        ItemSpawner.GetItem(itemToDrop.ItemDefinition, dropAmount, null, dropPosition, Quaternion.identity);
    }
    public static void DropItem(Vector3 dropPosition, EquipmentHandler equipment, EquipmentType equipmentType)
    {
        EquipmentSlot slot = equipment.GetEquipmentSlot(equipmentType);
        InventoryItem itemToDrop = slot.Item;

        if (!ItemExists(itemToDrop)) return;

        equipment.HandleItemUnequipping(slot);
        ItemSpawner.GetItem(itemToDrop.ItemDefinition, itemToDrop.CurrentStack, null, dropPosition, Quaternion.identity);
    }
    #endregion
}
