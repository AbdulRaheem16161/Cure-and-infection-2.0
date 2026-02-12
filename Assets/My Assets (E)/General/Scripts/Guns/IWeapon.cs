public interface IWeapon
{
    // Read-only stats
    int damage { get; }
    int ammoCapacity { get; }
    float fireRate { get; }
    float reloadTime { get; }
    float range { get; }
    float bulletVisualSpeed { get; }
    float beamRadius { get; }
    float soundRadius { get; }

    // Runtime values (mutable)
    int currentAmmo { get; set; }
    int totalAmmo { get; set; }
}
