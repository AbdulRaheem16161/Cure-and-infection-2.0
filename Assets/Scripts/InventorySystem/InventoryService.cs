using UnityEngine;

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

    public enum MoveItemResult
    {
        fullInventory, itemNull, success
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

    #region Move Item
    public static void TryMoveItem(ItemContainer destination, ItemContainer source, int slot, bool logOutcome = false)
    {
        MoveItemResult result = MoveItemValid(source, slot);

        switch (result)
        {
            case MoveItemResult.success:
                InventoryItem itemToMove = source.Items[slot];
                destination.AddNewItem(itemToMove, true);
                source.RemoveItemsFromSlot(slot, itemToMove.CurrentStack, true);
                break;

            case MoveItemResult.fullInventory:
                if (logOutcome) Debug.LogWarning("Cannot move Item, destination inventory full.");
                break;

            case MoveItemResult.itemNull:
                if (logOutcome) Debug.LogWarning("Cannot move Item, item in slot null");
                break;
        }
    }

    public static MoveItemResult MoveItemValid(ItemContainer container, int slot)
    {
        if (container.ContainerFull()) return MoveItemResult.fullInventory;
        if (container.InventoryItemExists(container.Items[slot])) return MoveItemResult.itemNull;
        return MoveItemResult.success;
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
