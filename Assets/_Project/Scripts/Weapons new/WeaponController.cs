using UnityEngine;
using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;

namespace temp
{
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] private List<temp.WeaponBase> weaponSlots;
        [SerializeField] private temp.WeaponBase currentWeapon;
        [SerializeField] private Vector3 pickedItemPosition;

        public void PickUpWeapon(temp.WeaponBase WeaponToPick)
        {
            // enter the weapon in the slot
            int SlotIndex = WeaponToPick.data.slotIndex;
            weaponSlots[SlotIndex] = WeaponToPick;

            // unequip the current weapon
            currentWeapon = null;

            // attach the gun to your hand
            WeaponToPick.transform.parent = transform;
            WeaponToPick.transform.position = pickedItemPosition;
            WeaponToPick.GetComponent<Rigidbody>().isKinematic = true;

            // equip the gun
            currentWeapon = WeaponToPick;
        }

        public void DropWeapon()
        {
            if (currentWeapon == null) return;

            // remove the weapon from the slot
            int SlotIndex = currentWeapon.data.slotIndex;
            weaponSlots[SlotIndex] = null;

            // unequip the current weapon
            currentWeapon = null;

            // unattach the gun from the hand and let it drop
            currentWeapon.transform.parent = null;
            currentWeapon.GetComponent<Rigidbody>().isKinematic = false;
        }

        public void SwitchWeapon(int slotIndex)
        {
            WeaponBase weaponToEquip = weaponSlots[slotIndex];

            if (weaponToEquip == null) return;

            currentWeapon.gameObject.SetActive(false);
            currentWeapon = weaponToEquip;
            currentWeapon.gameObject.SetActive(true);
        }

        //public void PerformAttack()
        //{
        //    currentWeapon.Attack();
        //}

        //public void PerformReload()
        //{
        //    currentWeapon.Reload();
        //}
    }
}

