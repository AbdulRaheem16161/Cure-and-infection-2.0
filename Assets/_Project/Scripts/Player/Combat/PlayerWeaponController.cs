using UnityEngine;
using Game.MyNPC;
using System.Collections;
using System.Linq;
using Game.Core;
using Game.MyPlayer;

public class PlayerWeaponController : MonoBehaviour
{
    #region References
    [Header("References")]
    public InputReader InputReader;
    public PlayerStateMachine playerStateMachine;
    public NPCStateMachine npcStateMachine;
    public WeaponSound weaponSound;
    #endregion

    #region Weapons
    [SerializeField] public IWeapon weapon;
    [SerializeField] private Weapon1Stats _melee;
    [SerializeField] private Weapon2Stats _pistol;
    [SerializeField] private Weapon3Stats _gun;

    public GameObject MeleeGameObject;
    public GameObject PistolGameObject;
    public GameObject GunGameObject;
    #endregion

    #region Change Weapon on Pickup
    public Weapon1Stats Melee
    {
        get => _melee;
        set
        {
            #region Change Melee Weapon on Pickup
            // If the new melee weapon is the same as the current, do nothing
            if (_melee == value) return;

            // Drop the current melee weapon game object in the world
            if (MeleeGameObject != null) DropWeaponGameObject(MeleeGameObject, value.gameObject.transform.position); // value.gameObject is the newly picked melee weapon

            // Assign the new melee weapon and update the reference to its game object
            _melee = value;
            MeleeGameObject = _melee.gameObject;

            // Switch to the new melee weapon (update visuals and logic)
            SwitchWeapon1();
            #endregion
        }
    }

    public Weapon2Stats Pistol
    {
        get => _pistol;
        set
        {
            #region Change Pistol Weapon on Pickup
            // If the new pistol is the same as the current, do nothing
            if (_pistol == value) return;

            // Drop the current pistol weapon game object in the world
            if (PistolGameObject != null) DropWeaponGameObject(PistolGameObject, value.gameObject.transform.position);

            // Assign the new pistol and update the reference to its game object
            _pistol = value;
            PistolGameObject = _pistol.gameObject;

            // Switch to the new pistol (update visuals and logic)
            SwitchWeapon2();
            #endregion
        }
    }

    public Weapon3Stats Gun
    {
        get => _gun;
        set
        {
            #region Change Gun Weapon on Pickup
            // If the new gun is the same as the current, do nothing
            if (_gun == value) return;

            // Drop the current gun weapon game object in the world
            if (GunGameObject != null) DropWeaponGameObject(GunGameObject, value.gameObject.transform.position);

            // Assign the new gun and update the reference to its game object
            _gun = value;
            GunGameObject = _gun.gameObject;

            // Switch to the new gun (update visuals and logic)
            SwitchWeapon3();
            #endregion
        }
    }
    #endregion

    #region Melee
    [Header("Melee")]
    public GameObject Club;
    public GameObject Axe;
    public GameObject Hammer;
    public GameObject Mace;
    #endregion

    #region Pistols
    [Header("Pistols")]
    public GameObject Glock;
    public GameObject DeaserEagle;
    public GameObject M9;
    #endregion

    #region Guns
    [Header("Guns")]
    public GameObject Rifle;
    public GameObject Shotgun;
    public GameObject SMG;
    public GameObject Sniper;
    #endregion

    #region Settings
    [Header("Settings")]
    public string[] HitableObjects;
    public bool showRayGizmos = true;
    public Color rayColor = Color.red;
    public float DropOffset = 1f;
    #endregion

    #region Live Values
    [Header("Live Values")]
    public bool isReloading = false;
    public string Weapon1Type = "";
    public string Weapon2Type = "";
    public string Weapon3Type = "";
    public string currentWeaponName = "";

    #endregion

    #region Attachments
    public Transform muzzlePoint;
    public GameObject bulletPrefab;
    #endregion

    #region Runtime
    private float _nextFireTime;
    private Vector3 _lastHitPoint;
    #endregion

    private void Update()
    {
        if (playerStateMachine == null) return;

        #region Handle Input

        WeaponSwitch();

        if (InputReader.AttackPressed)
        {
            Shoot();
        }

        if (InputReader.ReloadTriggered)
        {
            RequestReload();

            InputReader.ReloadTriggered = false;
        }

        #endregion
    }

    #region Weapon Switch
    public void WeaponSwitch()
    {
        if (playerStateMachine == null) return;

        #region Trigger Weapon Switch
        if (InputReader.SwitchWeapon1Triggered && _melee != null)
        {
            SwitchWeapon1();
        }
        if (InputReader.SwitchWeapon2Triggered && _pistol != null)
        {
            SwitchWeapon2();
        }
        if (InputReader.SwitchWeapon3Triggered && _gun != null)
        {
            SwitchWeapon3();
        }
        #endregion
    }

    public void SwitchWeapon1()
    {
        #region SwitchWeapon1

        Debug.Log("Switching to Melee Weapon");

        weapon = _melee;

        #region Enable Weapon model
        switch (_melee.weaponType)
        {
            case Weapon1Stats.MeleeType.Club:
                Club.SetActive(true);
                Axe.SetActive(false);
                Hammer.SetActive(false);
                Mace.SetActive(false);
                break;
            case Weapon1Stats.MeleeType.Axe:
                Club.SetActive(false);
                Axe.SetActive(true);
                Hammer.SetActive(false);
                Mace.SetActive(false);
                break;
            case Weapon1Stats.MeleeType.Hammer:
                Club.SetActive(false);
                Axe.SetActive(false);
                Hammer.SetActive(true);
                Mace.SetActive(false);
                break;
            case Weapon1Stats.MeleeType.Mace:
                Club.SetActive(false);
                Axe.SetActive(false);
                Hammer.SetActive(false);
                Mace.SetActive(true);
                break;
        }

        Glock.SetActive(false);
        DeaserEagle.SetActive(false);
        M9.SetActive(false);
        Rifle.SetActive(false);
        Shotgun.SetActive(false);
        SMG.SetActive(false);
        Sniper.SetActive(false);

        #endregion

        currentWeaponName = _melee.weaponType.ToString();

        InputReader.SwitchWeapon1Triggered = false;

        #endregion
    }
    public void SwitchWeapon2()
    {
        #region SwitchWeapon2

        weapon = _pistol;

        #region Enable Weapon model
        switch (_pistol.weaponType)
        {
            case Weapon2Stats.PistolType.Glock:
                Glock.SetActive(true);
                DeaserEagle.SetActive(false);
                M9.SetActive(false);
                break;
            case Weapon2Stats.PistolType.DesertEagle:
                Glock.SetActive(false);
                DeaserEagle.SetActive(true);
                M9.SetActive(false);
                break;
            case Weapon2Stats.PistolType.M9:
                Glock.SetActive(false);
                DeaserEagle.SetActive(false);
                M9.SetActive(true);
                break;
        }

        Club.SetActive(false);
        Axe.SetActive(false);
        Hammer.SetActive(false);
        Mace.SetActive(false);
        Rifle.SetActive(false);
        Shotgun.SetActive(false);
        SMG.SetActive(false);
        Sniper.SetActive(false);

        #endregion

        currentWeaponName = _pistol.weaponType.ToString();

        InputReader.SwitchWeapon2Triggered = false;

        #endregion
    }
    public void SwitchWeapon3()
    {
        #region SwitchWeapon3

        weapon = _gun;

        #region Enable Weapon model
        switch (_gun.weaponType)
        {
            case Weapon3Stats.GunType.Rifle:
                Rifle.SetActive(true);
                Shotgun.SetActive(false);
                SMG.SetActive(false);
                Sniper.SetActive(false);
                break;
            case Weapon3Stats.GunType.Shotgun:
                Rifle.SetActive(false);
                Shotgun.SetActive(true);
                SMG.SetActive(false);
                Sniper.SetActive(false);
                break;
            case Weapon3Stats.GunType.SMG:
                Rifle.SetActive(false);
                Shotgun.SetActive(false);
                SMG.SetActive(true);
                Sniper.SetActive(false);
                break;
            case Weapon3Stats.GunType.Sniper:
                Rifle.SetActive(false);
                Shotgun.SetActive(false);
                SMG.SetActive(false);
                Sniper.SetActive(true);
                break;
        }

        Club.SetActive(false);
        Axe.SetActive(false);
        Hammer.SetActive(false);
        Mace.SetActive(false);
        Glock.SetActive(false);
        DeaserEagle.SetActive(false);
        M9.SetActive(false);

        #endregion

        currentWeaponName = _gun.weaponType.ToString();

        InputReader.SwitchWeapon3Triggered = false;

        #endregion
    }

    public void DropWeaponGameObject(GameObject WeaponGameObject, Vector3 NewWeaponPosition)
    {
        #region DropWeapon
        WeaponGameObject.transform.position = NewWeaponPosition;
        WeaponGameObject.SetActive(true);
        #endregion
    }
    #endregion

    #region Shoot

    public void Shoot()
    {
        #region safty check
        if (weapon == null)
            return;
        #endregion

        #region Fire Rate
        if (Time.time < _nextFireTime)
            return;
        _nextFireTime = Time.time + weapon.fireRate;
        #endregion

        #region Reload Management
        if (weapon.currentAmmo <= 0)
        {
            RequestReload();
            return;
        }

        if (isReloading)
            return;

        weapon.currentAmmo -= 1;

        #endregion

        #region Shoot
        PlayerShoot();
        #endregion

        #region Generate Gun Sound
        weaponSound.GenerateWeaponSound(weapon.soundRadius);
        #endregion
    }

    public void PlayerShoot()
    {
        #region Summary
        /// <summary>
        /// Shoots from the camera forward using accurate hitscan logic.
        /// </summary>
        #endregion

        #region PlayerShoot

        if (playerStateMachine.CameraTransform == null)
            return;

        Vector3 origin = playerStateMachine.CameraTransform.position;
        Vector3 direction = playerStateMachine.CameraTransform.forward;

        string[] hitableTags =
            HitableObjects.Concat(playerStateMachine.TargetTags).ToArray();

        RaycastHit hit;
        bool hasHit = TryGetAccurateHit(
            origin,
            direction,
            hitableTags,
            out hit
        );

        if (hasHit)
        {
            _lastHitPoint = hit.point;

            IDamageable damageable;
            if (hit.collider.TryGetComponent<IDamageable>(out damageable))
                damageable.RecieveDamage(weapon.damage);
        }
        else
        {
            _lastHitPoint = origin + direction * weapon.range;
        }

        SpawnVisualBullet(_lastHitPoint);
        #endregion
    }

    private bool IsValidTarget(Collider collider, string[] hitableTags)
    {
        #region IsValidTarget
        #region Summary
        /// <summary>
        /// Checks if the collider matches any allowed tag.
        /// </summary>
        #endregion

        for (int i = 0; i < hitableTags.Length; i++)
        {
            if (collider.CompareTag(hitableTags[i]))
                return true;
        }

        return false;
        #endregion
    }


    private bool TryGetAccurateHit(Vector3 origin, Vector3 direction, string[] hitableTags, out RaycastHit finalHit)
    {
        #region TryGetAccurateHit
        #region Summary
        /// <summary>
        /// Tries a precise Raycast first.
        /// If it misses, falls back to a SphereCast.
        /// Returns the closest valid hit.
        /// </summary>
        #endregion

        finalHit = new RaycastHit();

        #region Raycast (Perfect Accuracy)
        RaycastHit rayHit;
        bool raycastHit = Physics.Raycast(
            origin,
            direction,
            out rayHit,
            weapon.range
        );

        if (raycastHit)
        {
            if (IsValidTarget(rayHit.collider, hitableTags))
            {
                finalHit = rayHit;
                return true;
            }
        }
        #endregion

        #region SphereCast (Aim Assist)
        RaycastHit[] hits = Physics.SphereCastAll(
            origin,
            weapon.beamRadius,
            direction,
            weapon.range
        );

        float closestDistance = float.MaxValue;
        bool hitFound = false;

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];

            if (!IsValidTarget(hit.collider, hitableTags))
                continue;

            float distance = Vector3.Distance(origin, hit.point);

            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            finalHit = hit;
            hitFound = true;
        }

        return hitFound;
        #endregion
        #endregion
    }

    #endregion

    private void SpawnVisualBullet(Vector3 hitPoint)
    {
        #region SpawnVisualBullet

        Debug.Log("viusal Bullet");
        GameObject bullet = Instantiate(
            bulletPrefab,
            muzzlePoint.position,
            Quaternion.LookRotation(hitPoint - muzzlePoint.position)
        );

        StartCoroutine(MoveBullet(bullet, hitPoint));

        #endregion
    }

    private IEnumerator MoveBullet(GameObject bullet, Vector3 hitPoint)
    {
        #region MoveBullet

        Vector3 startPos = bullet.transform.position;
        float distance = Vector3.Distance(startPos, hitPoint);
        float travelTime = distance / weapon.bulletVisualSpeed;

        float t = 0f;

        while (t < 1f)
        {
            if (bullet == null)
                yield break;

            bullet.transform.position = Vector3.Lerp(startPos, hitPoint, t);
            t += Time.deltaTime / travelTime;

            yield return null;
        }

        if (bullet != null)
            Destroy(bullet);

        #endregion
    }

    public void RequestReload()
    {
        #region RequestReload

        if (weapon == null)
            return;

        if (isReloading)
            return;

        if (weapon.currentAmmo >= weapon.ammoCapacity)
            return;

        if (weapon.totalAmmo <= 0)
            return;

        StartCoroutine(ReloadAmmo());
        #endregion
    }

    private IEnumerator ReloadAmmo()
    {
        #region ReloadAmmo
        isReloading = true;
        yield return new WaitForSeconds(weapon.reloadTime);
        int ammoNeeded = weapon.ammoCapacity - weapon.currentAmmo;

        if (weapon.totalAmmo >= ammoNeeded)
        {
            weapon.totalAmmo -= ammoNeeded;
            weapon.currentAmmo += ammoNeeded;
        }
        else
        {
            weapon.currentAmmo += weapon.totalAmmo;
            weapon.totalAmmo = 0;
        }

        isReloading = false;
        #endregion
    }

    private void OnDrawGizmos()
    {
        #region Draw Ray Gizmos
        if (!showRayGizmos || muzzlePoint == null || weapon == null)
            return;

        Vector3 start = muzzlePoint.position;
        Vector3 end = (_lastHitPoint != Vector3.zero)
            ? _lastHitPoint
            : start + muzzlePoint.forward * weapon.range;

        Gizmos.color = rayColor;

        // Draw beam body
        Gizmos.DrawWireSphere(start, weapon.beamRadius);
        Gizmos.DrawWireSphere(end, weapon.beamRadius);
        Gizmos.DrawLine(start + Vector3.up * weapon.beamRadius, end + Vector3.up * weapon.beamRadius);
        Gizmos.DrawLine(start - Vector3.up * weapon.beamRadius, end - Vector3.up * weapon.beamRadius);
        Gizmos.DrawLine(start + Vector3.right * weapon.beamRadius, end + Vector3.right * weapon.beamRadius);
        Gizmos.DrawLine(start - Vector3.right * weapon.beamRadius, end - Vector3.right * weapon.beamRadius);

        #endregion
    }
}
