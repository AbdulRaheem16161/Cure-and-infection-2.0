using System;
using UnityEngine;

public abstract class Item<T> : Item where T : ItemDefinition
{
	public T TypedDefinition { get; private set; }
	public override ItemDefinition ItemDefinition => TypedDefinition;

	#region initialize item
	public override void InitializeItem(ItemDefinition definition, GameObject itemModel, int itemStack)
	{
		if (definition is not T typedDef)
		{
			Debug.LogError($"Invalid definition type. Expected {typeof(T)}, got {definition.GetType()}");
			return;
		}

		InitializeItem(typedDef, itemModel, itemStack);
	}
	public virtual void InitializeItem(T definition, GameObject itemModel, int itemStack)
	{
		TypedDefinition = definition;
		gameObject.name = TypedDefinition.ItemName;
		CurrentItemStack = itemStack;

		IsEquipped = false;
		IsInHands = false;

		UpdateItemModel(definition, itemModel);
		gameObject.SetActive(true);
	}
	#endregion

	#region update item model
	private void UpdateItemModel(T definition, GameObject itemModel)
	{
		if (itemModel == null && definition.ModelPrefab != null)
		{
			Debug.LogError($"{TypedDefinition.ItemName} model expected but none provided. ItemSpawner or pooling system failed.");
			return;
		}
		else if (itemModel == null && definition.ModelPrefab == null)
		{
			Debug.LogWarning($"{TypedDefinition.ItemName} model not assigned ignore if intended or lacks model and isnt required.");
			return;
		}

		/* remove old model, separate model pooling kept for potential future use but not required with current 1:1 items planned.
		if (ModelReference != null)
			CleanUpItemModel();
		*/

		itemModel.transform.SetParent(gameObject.transform);
		itemModel.transform.localScale = Vector3.one;
		itemModel.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		itemModel.SetActive(true);
		ModelReference = itemModel;
	}
	#endregion
}

public abstract class Item : MonoBehaviour
{
	public abstract ItemDefinition ItemDefinition { get; }
	[HideInInspector] public GameObject ModelReference { get; protected set; }

	public bool IsInHands { get; protected set; }
	public bool IsEquipped { get; protected set; }

	public int CurrentItemStack { get; protected set; }

	public static event Action<Item> OnCleanUpItem;
	public static event Action<ItemDefinition, GameObject> OnCleanUpItemModel;

	public abstract void InitializeItem(ItemDefinition definition, GameObject itemModel, int itemStack);

	#region base equip/unequip methods
	public virtual void EquipItem(EquipmentHandler equipmentHandler, Transform parentTransform)
	{
		transform.SetParent(parentTransform);
		transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		gameObject.SetActive(true);
		IsEquipped = true;
	}
	public virtual void UnEquipItem(EquipmentHandler equipmentHandler)
	{
		IsEquipped = false;
		gameObject.SetActive(false);
	}
	#endregion

	#region base holster/unholster methods
	public virtual void HolsterItem()
	{
		IsInHands = false;
	}
	public virtual void UnHolsterItem()
	{
		IsInHands = true;
	}
	#endregion

	#region item pickup
	public virtual void PickUp(InventoryHandler inventory)
	{
		InventoryItem newItem = new(ItemDefinition, CurrentItemStack);
		inventory.AddNewItem(newItem);
		CleanUpItem();
	}
	#endregion

	#region item and model clean up calls
	public void CleanUpItem()
	{
		OnCleanUpItem?.Invoke(this);
	}
	public void CleanUpItemModel()
	{
		if (ModelReference == null)
			return;

		OnCleanUpItemModel?.Invoke(ItemDefinition, ModelReference);
		ModelReference = null;
	}
	#endregion
}
