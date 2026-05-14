using System;
using UnityEngine;

public abstract class Item<T> : Item where T : ItemDefinition
{
	public T TypedDefinition { get; private set; }
	public override ItemDefinition ItemDefinition => TypedDefinition;

	#region initialize item
	public override void InitializeItem(ItemDefinition definition, int itemStack)
	{
		if (definition is not T typedDef)
		{
			Debug.LogError($"Invalid definition type. Expected {typeof(T)}, got {definition.GetType()}");
			return;
		}

		InitializeItem(typedDef, itemStack);
	}
	public virtual void InitializeItem(T definition, int itemStack)
	{
		TypedDefinition = definition;
		gameObject.name = TypedDefinition.ItemName;
		CurrentItemStack = itemStack;

		IsEquipped = false;
		IsInHands = false;

		UpdateItemModel(definition);
		gameObject.SetActive(true);
	}
	#endregion

	#region update item model
	private void UpdateItemModel(T definition)
	{
		if (definition.ModelPrefab == null)
		{
			Debug.LogWarning($"{TypedDefinition.ItemName} model not assigned ignore if intended or lacks model and isnt required.");
			if (ModelReference != null)
				Destroy(ModelReference);
			return;
		}
		else
		{
			if (ModelReference != null)
				Destroy(ModelReference);
			ModelReference = Instantiate(definition.ModelPrefab);
		}
		/* remove old model, separate model pooling kept for potential future use but not required with current 1:1 items planned.
		if (ModelReference != null)
			CleanUpItemModel();
		*/

		ModelReference.transform.SetParent(gameObject.transform);
		ModelReference.transform.localScale = Vector3.one;
		ModelReference.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		ModelReference.SetActive(true);
	}
	#endregion
}

public abstract class Item : MonoBehaviour, IInteractable
{
	public abstract ItemDefinition ItemDefinition { get; }
	[HideInInspector] public GameObject ModelReference { get; protected set; }

	public GameObject CurrentOwner { get; protected set; }

	public bool IsInHands { get; protected set; }
	public bool IsEquipped { get; protected set; }

	public int CurrentItemStack { get; protected set; }

	public static event Action<Item> OnCleanUpItem;
	public static event Action<ItemDefinition, GameObject> OnCleanUpItemModel;

    public abstract void InitializeItem(ItemDefinition definition, int itemStack);

	#region base equip/unequip methods
	public virtual void EquipItem(EquipmentHandler equipmentHandler, Transform parentTransform)
	{
		transform.SetParent(parentTransform);
		transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		gameObject.SetActive(true);
		CurrentOwner = equipmentHandler.gameObject;
		IsEquipped = true;
	}
	public virtual void UnEquipItem(EquipmentHandler equipmentHandler)
	{
		CurrentOwner = null;
		IsEquipped = false;
		gameObject.SetActive(false);
	}
	#endregion

	#region base holster/unholster methods
	public virtual void HolsterItem(EquipmentHandler equipmentHandler)
	{
		IsInHands = false;
		equipmentHandler.StatsHandler.OnHit -= OnHit;
	}
	public virtual void UnHolsterItem(EquipmentHandler equipmentHandler)
	{
		IsInHands = true;
		equipmentHandler.StatsHandler.OnHit += OnHit;
	}
	#endregion

	#region base on owner hit event listener
	public virtual void OnHit(DamageContext damageContext)
	{

	}
    #endregion

    #region item pickup
    public void InteractPress(Interactor interactor)
    {
		PickUpItem(interactor);
        return;
    }

    public void InteractHoldComplete(Interactor interactor)
    {
        return;
    }
    public virtual void PickUpItem(Interactor interactor)
	{
        if (interactor.Inventory == null)
		{
            Debug.LogError($"{typeof(InventoryHandler)} component doesnt exist on object. failed to pick up item");
            return;
        }

        InventoryItem newItem = new(ItemDefinition, CurrentItemStack);
        interactor.Inventory.ItemContainer.AddNewItem(newItem);
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
