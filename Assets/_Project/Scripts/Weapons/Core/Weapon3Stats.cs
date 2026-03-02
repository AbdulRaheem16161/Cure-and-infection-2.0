using UnityEngine;
using Game.MyPlayer;

public class Weapon3Stats : WeaponBase<Weapon3Stats.GunType>
{
    public enum GunType { Rifle, SMG, Shotgun, Sniper }

    public string weaponTypeName;

    public void Update()
    {
        weaponTypeName = weaponType.ToString();
    }

    protected override void UpdateStats()
    {
        switch (weaponType)
        {
            case GunType.Rifle:
                _damage = 10; _ammoCapacity = 30; _totalAmmo = 120;
                _fireRate = 0.2f; _reloadTime = 2f; _range = 20f;
                _bulletVisualSpeed = 60f; _beamRadius = 0.15f; _soundRadius = 30;
                break;
            case GunType.SMG:
                _damage = 5; _ammoCapacity = 50; _totalAmmo = 200;
                _fireRate = 0.1f; _reloadTime = 1.8f; _range = 15f;
                _bulletVisualSpeed = 50f; _beamRadius = 0.15f; _soundRadius = 25;
                break;
            case GunType.Shotgun:
                _damage = 100; _ammoCapacity = 8; _totalAmmo = 32;
                _fireRate = 1f; _reloadTime = 2.5f; _range = 10f;
                _bulletVisualSpeed = 100f; _beamRadius = 0.3f; _soundRadius = 25;
                break;
            case GunType.Sniper:
                _damage = 100; _ammoCapacity = 5; _totalAmmo = 25;
                _fireRate = 1.5f; _reloadTime = 3f; _range = 40f;
                _bulletVisualSpeed = 300f; _beamRadius = 0.05f; _soundRadius = 30;
                break;
        }
    }

    public override void PickUp()
    {
        stateMachine.GunManager.Gun = this;
        stateMachine.GunManager.GunGameObject = this.gameObject;
        stateMachine.GunManager.Weapon3Type = weaponType.ToString();
        base.PickUp();
    }
}
