using UnityEngine;
using Game.MyPlayer;

public abstract class WeaponBase<TEnum> : MonoBehaviour, IWeapon, IInteractable
    where TEnum : System.Enum
{
    #region Weapon Type
    public TEnum weaponType;
    #endregion

    #region Serialized Stats
    [SerializeField] protected int _damage;
    [SerializeField] protected int _ammoCapacity;
    [SerializeField] protected float _fireRate;
    [SerializeField] protected float _reloadTime;
    [SerializeField] protected float _range;
    [SerializeField] protected float _bulletVisualSpeed;
    [SerializeField] protected float _beamRadius;
    [SerializeField] protected float _soundRadius;
    #endregion

    #region Runtime / Derived Values
    [SerializeField] protected int _currentAmmo;
    [SerializeField] protected int _totalAmmo;
    #endregion

    #region IWeapon Implementation
    public int damage => _damage;
    public int ammoCapacity => _ammoCapacity;
    public float fireRate => _fireRate;
    public float reloadTime => _reloadTime;
    public float range => _range;
    public float bulletVisualSpeed => _bulletVisualSpeed;
    public float beamRadius => _beamRadius;
    public float soundRadius => _soundRadius;

    public int currentAmmo
    {
        get => _currentAmmo;
        set => _currentAmmo = value;
    }

    public int totalAmmo
    {
        get => _totalAmmo;
        set => _totalAmmo = value;
    }
    #endregion

    #region Pickup
    [Header("Pickup")]
    protected PlayerStateMachine stateMachine;

    protected virtual void Awake()
    {
        stateMachine = Object.FindFirstObjectByType<PlayerStateMachine>();
    }

    public void Interact()
    {
        PickUp();
    }

    public virtual void PickUp()
    {
        // Derived classes will implement how to assign themselves to GunManager
        gameObject.SetActive(false);
    }
    #endregion

    #region OnValidate (Stats Initialization)
    protected abstract void UpdateStats();

    protected virtual void OnValidate()
    {
        UpdateStats();
        _currentAmmo = _ammoCapacity;
    }
    #endregion
}
