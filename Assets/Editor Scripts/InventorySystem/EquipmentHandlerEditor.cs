using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EquipmentHandler))]
public class EquipmentHandlerEditor : Editor
{
	private bool showDebugControls;
	private bool showItemEquippingControls;
	private bool showItemUnequippingControls;
	private bool showItemHolsteringControls;
	private bool showUseConsumableControls;

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		EquipmentHandler equipment = (EquipmentHandler)target;

		GUILayout.Space(10);
		GUILayout.Label("DEBUG CONTROLS", EditorStyles.boldLabel);
		showDebugControls = EditorGUILayout.Toggle("Show Debug Controls", showDebugControls);

		if (!showDebugControls) return;

		GUILayout.Space(10);

		showItemEquippingControls = EditorGUILayout.Toggle("Show Item Equipping Controls", showItemEquippingControls);

		if (showItemEquippingControls)
		{
			#region equip item
			GUILayout.Label("Equipping Items", EditorStyles.boldLabel);
			equipment.itemToEquip = (ItemDefinition)EditorGUILayout.ObjectField("Item To Equip", equipment.itemToEquip, typeof(ItemDefinition), false);
			equipment.slotToEquipItemTo = (EquipmentHandler.EquipmentType)EditorGUILayout.EnumPopup("Slot To Equip Item To", equipment.slotToEquipItemTo);
			equipment.itemToEquipCount = EditorGUILayout.IntField("Item To Equip Count", equipment.itemToEquipCount);

			if (GUILayout.Button("Equip Item (destroys any equipped one)"))
			{
				if (!ApplicationPlaying()) return;

				if (equipment.itemToEquip == null)
				{
					Debug.LogError("no item specified in itemToEquip field");
					return;
				}

				if (equipment.itemToEquipCount > equipment.itemToEquip.StackLimit)
				{
					Debug.LogWarning($"{equipment.itemToEquipCount} is higher then stack limit of {equipment.itemToEquip.StackLimit}, setting to max");
					equipment.itemToEquipCount = equipment.itemToEquip.StackLimit;
				}
				if (equipment.itemToEquipCount < 1)
				{
					Debug.LogWarning($"{equipment.itemToEquipCount} is smaller then minimum limit of 1, setting to 1");
					equipment.itemToEquipCount = 1;
				}

				equipment.EquipItem(equipment.itemToEquip, equipment.itemToEquipCount, equipment.slotToEquipItemTo);
			}
			#endregion

			GUILayout.Space(10);

			#region equip item from inventory
			GUILayout.Label("Equipping Item From Inventory", EditorStyles.boldLabel);
			equipment.equipItemFromInventorySlot = EditorGUILayout.IntField("Equip Item From Inventory Slot", equipment.equipItemFromInventorySlot);
			equipment.slotToEquipItemTo = 
				(EquipmentHandler.EquipmentType)EditorGUILayout.EnumPopup("Slot To Equip Item To", equipment.slotToEquipItemTo);

			if (GUILayout.Button("Equip Item From Inventory"))
			{
				if (!ApplicationPlaying()) return;

				if (equipment.itemToEquip == null)
				{
					Debug.LogError("no item specified in itemToEquip field");
					return;
				}

				equipment.EquipItemFromInventory(equipment.equipItemFromInventorySlot, equipment.slotToEquipItemTo);
			}
			#endregion
		}

		GUILayout.Space(10);

		showItemUnequippingControls = EditorGUILayout.Toggle("Show Item Unequipping Controls", showItemUnequippingControls);

		if (showItemUnequippingControls)
		{
			#region unequip item button
			GUILayout.Label("Unequipping Items", EditorStyles.boldLabel);
			equipment.equipmentSlotToUnequip =
				(EquipmentHandler.EquipmentType)EditorGUILayout.EnumPopup("Slot To Unequip", equipment.equipmentSlotToUnequip);

			if (GUILayout.Button("Unequip Item To Inventory"))
			{
				if (!ApplicationPlaying()) return;

				equipment.UnequipItem(equipment.equipmentSlotToUnequip);
			}
			if (GUILayout.Button("Unequip Item (destroys)"))
			{
				if (!ApplicationPlaying()) return;

				equipment.UnequipItem(equipment.equipmentSlotToUnequip, false);
			}
			#endregion
		}

		GUILayout.Space(10);

		showItemHolsteringControls = EditorGUILayout.Toggle("Show Item Unequipping Controls", showItemHolsteringControls);

		if (showItemHolsteringControls)
		{
			#region item holstering/unholstering buttons
			GUILayout.Label("Holster/Unholster Equipped Items", EditorStyles.boldLabel);
			equipment.unHolsterItem = (EquipmentHandler.EquipmentType)EditorGUILayout.EnumPopup("Unholster Item", equipment.unHolsterItem);

			if (GUILayout.Button("Un Holster Weapon"))
			{
				if (!ApplicationPlaying()) return;

				equipment.UnholsterWeapon(equipment.unHolsterItem);
			}
			if (GUILayout.Button("Holster Weapon"))
			{
				if (!ApplicationPlaying()) return;

				equipment.HolsterWeapon();
			}
			#endregion
		}

		GUILayout.Space(10);

		showUseConsumableControls = EditorGUILayout.Toggle("Show Use Consumable Controls", showUseConsumableControls);

		if (showUseConsumableControls)
		{
			#region use consumable in equipment slot button
			GUILayout.Label("Use Consumable In Slot", EditorStyles.boldLabel);
			equipment.consumableSlotToUse =
				(EquipmentHandler.EquipmentType)EditorGUILayout.EnumPopup("Use Consumable In Slot", equipment.consumableSlotToUse);

			if (GUILayout.Button("Use Consumable Item"))
			{
				if (!ApplicationPlaying()) return;

				equipment.UseConsumable(equipment.consumableSlotToUse);
			}
			#endregion
		}
	}

	private bool ApplicationPlaying()
	{
		if (!Application.isPlaying)
		{
			Debug.LogWarning("Must be in Play Mode");
			return false;
		}

		return true;
	}
}
