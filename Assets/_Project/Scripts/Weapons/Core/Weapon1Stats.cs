using UnityEngine;
using Game.MyPlayer;

public class Weapon1Stats : WeaponBase<Weapon1Stats.MeleeType>
{
    public enum MeleeType { Club, Axe, Hammer, Mace }

    public string weaponTypeName;

    public void Update()
    {
        weaponTypeName = weaponType.ToString();
    }

    protected override void UpdateStats()
    {
        _ammoCapacity = int.MaxValue;
        _currentAmmo = int.MaxValue;
        _totalAmmo = int.MaxValue;
        _soundRadius = 3f;

        switch (weaponType)
        {
            case MeleeType.Club: _damage = 20; _fireRate = 0.5f; _range = 2f; break;
            case MeleeType.Axe: _damage = 35; _fireRate = 0.7f; _range = 2.5f; break;
            case MeleeType.Hammer: _damage = 50; _fireRate = 1f; _range = 3f; break;
            case MeleeType.Mace: _damage = 40; _fireRate = 0.8f; _range = 3.5f; break;
        }
    }

    public override void PickUp()
    {
        stateMachine.GunManager.Melee = this;
        stateMachine.GunManager.MeleeGameObject = this.gameObject;
        stateMachine.GunManager.Weapon1Type = weaponType.ToString();
        base.PickUp();
    }
}
