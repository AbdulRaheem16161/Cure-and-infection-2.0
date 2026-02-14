using UnityEngine;

public abstract class Item<T> : MonoBehaviour where T : ItemDefinition
{
	protected T itemDefinition;

	public int CurrentItemStack{ get; private set; }

	public virtual void InitializeItem(T definition)
	{
		itemDefinition = definition;
	}

	//add generic item behaviour like picking up items etc...
	public virtual void PickUp(InventoryHandler inventory) //would be a player collider/interact action here that passes inventory
	{
		inventory.AddNewItemPickUp(itemDefinition, CurrentItemStack);
		//pick up item and add to inventory etc...
	}
}
