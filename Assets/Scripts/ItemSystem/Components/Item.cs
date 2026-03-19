using System;
using UnityEngine;

public abstract class Item<T> : Item where T : ItemDefinition
{
	public T TypedDefinition { get; private set; }
	public override ItemDefinition ItemDefinition => TypedDefinition;

	public int CurrentItemStack{ get; private set; }

	[HideInInspector] protected GameObject modelReference;

	public static event Action<Item> OnCleanUpItem;

	#region initialize item
	public override void InitializeItem(ItemDefinition definition, int itemStack)
	{
		InitializeItem(definition as T, itemStack);
	}
	public virtual void InitializeItem(T definition, int itemStack)
	{
		TypedDefinition = definition;
		gameObject.name = TypedDefinition.ItemName;
		CurrentItemStack = itemStack;

		UpdateItemModel(definition);
	}
	#endregion

	#region update/instantiate item model from item definition
	private void UpdateItemModel(T definition)
	{
		if (definition.ItemPrefab == null)
		{
			Debug.LogWarning("item definitions model prefab is null");
			return;
		}

		//remove old model
		if (modelReference != null)
			Destroy(modelReference);

		GameObject modelRef = Instantiate(definition.ItemPrefab, transform);
		modelRef.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		modelReference = modelRef;
	}
	#endregion

	#region item pickup (TODO: destroy world object being picked up, decide how its called eg: interact or trigger collider etc...)
	public virtual void PickUp(InventoryHandler inventory)
	{
		InventoryItem newItem = new(TypedDefinition, CurrentItemStack);
		inventory.AddNewItem(newItem);
		CleanUpItem();
	}
	#endregion

	#region item cleanup
	public void CleanUpItem()
	{
		OnCleanUpItem?.Invoke(this);
	}
	#endregion
}

public abstract class Item : MonoBehaviour
{
	public abstract ItemDefinition ItemDefinition { get; }
	public abstract void InitializeItem(ItemDefinition definition, int itemStack);
}
