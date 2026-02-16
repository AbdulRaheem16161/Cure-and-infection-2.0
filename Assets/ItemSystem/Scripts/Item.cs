using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using static UnityEditor.Progress;

public abstract class Item<T> : MonoBehaviour where T : ItemDefinition
{
	protected T itemDefinition;

	public int CurrentItemStack{ get; private set; }

	public virtual void InitializeItem(T definition)
	{
		itemDefinition = definition;
	}

	#region item pickup (TODO: destroy world object being picked up, decide how its called eg: interact or trigger collider etc...)
	public virtual void PickUp(InventoryHandler inventory)
	{
		InventoryItem newItem = new(itemDefinition, CurrentItemStack);
		inventory.AddNewItemPickUp(newItem);
		//pick up item and add to inventory etc...
	}
	#endregion
}
