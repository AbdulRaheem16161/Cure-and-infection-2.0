using UnityEngine;

public abstract class Item<T> : MonoBehaviour where T : ItemDefinition
{
	protected T itemDefinition;

	public int CurrentItemStack{ get; private set; }

	[HideInInspector] protected GameObject modelReference;

	#region initialize item
	public virtual void InitializeItem(T definition, int itemStack)
	{
		itemDefinition = definition;
		gameObject.name = itemDefinition.ItemName;
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
		InventoryItem newItem = new(itemDefinition, CurrentItemStack);
		inventory.AddNewItem(newItem);
		Destroy(gameObject);
	}
	#endregion
}
