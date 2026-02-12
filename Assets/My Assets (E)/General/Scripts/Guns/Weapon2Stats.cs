using UnityEngine;
using Game.MyPlayer;

public class Weapon2Stats : WeaponBase<Weapon2Stats.PistolType>
{
    public enum PistolType { Glock, DesertEagle, M9 }

    public string weaponTypeName;

    public void Update()
    {
        weaponTypeName = weaponType.ToString();
    }

    protected override void UpdateStats()
    {
        switch (weaponType)
        {
            case PistolType.Glock:
                _damage = 20; _ammoCapacity = 30; _totalAmmo = 120;
                _fireRate = 0.2f; _reloadTime = 2f; _range = 15f;
                _bulletVisualSpeed = 60f; _beamRadius = 0.15f; _soundRadius = 25;
                break;
            case PistolType.DesertEagle:
                _damage = 35; _ammoCapacity = 7; _totalAmmo = 35;
                _fireRate = 0.3f; _reloadTime = 2f; _range = 15f;
                _bulletVisualSpeed = 60f; _beamRadius = 0.15f; _soundRadius = 30;
                break;
            case PistolType.M9:
                _damage = 25; _ammoCapacity = 15; _totalAmmo = 90;
                _fireRate = 0.25f; _reloadTime = 1.8f; _range = 15f;
                _bulletVisualSpeed = 55f; _beamRadius = 0.15f; _soundRadius = 25;
                break;
        }
    }

    public override void PickUp()
    {
        stateMachine.GunManager.Pistol = this;
        stateMachine.GunManager.PistolGameObject = this.gameObject;
        stateMachine.GunManager.Weapon2Type = weaponType.ToString();
        base.PickUp();
    }
}
