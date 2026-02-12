using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.MyNPC;
using Game.MyPlayer;

public class NPCWeaponController : MonoBehaviour
{
    #region References
    public NPCStateMachine stateMachine;
    public Transform muzzlePoint;
    public GameObject bulletPrefab;
    public WeaponSound weaponSound;
    #endregion

    #region Settings
    public List<string> HitableObjects = new List<string>();
    public bool showRayGizmos = true;
    public Color rayColor;
    #endregion

    #region Live Values
    public string CurrentWeapon;
    public bool isReloading = false;
    #endregion

    #region Runtime
    private Vector3 _lastHitPoint;
    private float _nextFireTime;
    #endregion

    #region Weapons
    public IWeapon weapon;
    public WeaponsStruct.EnumWeaponType selectedWeapon;

    #region Melee GameObject & Script
    [Header("Melee")]
    public GameObject ClubObject;
    public Weapon1Stats ClubStript;

    public GameObject AxeGameObject;
    public Weapon1Stats AxeStript;

    public GameObject HammerGameObject;
    public Weapon1Stats HammerStript;

    public GameObject MaceGameObject;
    public Weapon1Stats MaceStript;
    #endregion

    #region Pistols GameObject & Script
    [Header("Pistols")]
    public GameObject GlockGameObject;
    public Weapon2Stats GlockStript;

    public GameObject DeaserEagleGameObject;
    public Weapon2Stats DeaserEagleStript;

    public GameObject M9GameObject;
    public Weapon2Stats M9Script;
    #endregion

    #region Guns GameObject & Script
    [Header("Guns")]
    public GameObject RifleGameObject;
    public Weapon3Stats RifleStript;

    public GameObject ShotgunGameObject;
    public Weapon3Stats ShotgunStript;

    public GameObject SMGGameObject;
    public Weapon3Stats SMGStript;

    public GameObject SniperGameObject;
    public Weapon3Stats SniperStript;
    #endregion

    #endregion

    #region Change Weapon 
    private void OnValidate()
    {
        #region Summary
        /// <summary>
        /// Syncs weapon reference and enables the correct
        /// weapon GameObject in editor time
        /// </summary>
        #endregion

        #region Sync Selected Weapon

        if (!Application.isPlaying)
            return;

        DisableAllWeaponGameObjects();

        switch (selectedWeapon)
        {
            #region Melee
            case WeaponsStruct.EnumWeaponType.Club:
                weapon = ClubStript;
                if (ClubObject) ClubObject.SetActive(true);
                CurrentWeapon = "Club";
                break;

            case WeaponsStruct.EnumWeaponType.Axe:
                weapon = AxeStript;
                if (AxeGameObject) AxeGameObject.SetActive(true);
                CurrentWeapon = "Axe";
                break;

            case WeaponsStruct.EnumWeaponType.Hammer:
                weapon = HammerStript;
                if (HammerGameObject) HammerGameObject.SetActive(true);
                CurrentWeapon = "Hammer";
                break;

            case WeaponsStruct.EnumWeaponType.Mace:
                weapon = MaceStript;
                if (MaceGameObject) MaceGameObject.SetActive(true);
                CurrentWeapon = "Mace";
                break;
            #endregion

            #region Pistols
            case WeaponsStruct.EnumWeaponType.Glock:
                weapon = GlockStript;
                if (GlockGameObject) GlockGameObject.SetActive(true);
                CurrentWeapon = "Glock";
                break;

            case WeaponsStruct.EnumWeaponType.DeaserEagle:
                weapon = DeaserEagleStript;
                if (DeaserEagleGameObject) DeaserEagleGameObject.SetActive(true);
                CurrentWeapon = "DeaserEagle";
                break;

            case WeaponsStruct.EnumWeaponType.M9:
                weapon = M9Script;
                if (M9GameObject) M9GameObject.SetActive(true);
                CurrentWeapon = "M9";
                break;
            #endregion

            #region Guns
            case WeaponsStruct.EnumWeaponType.Rifle:
                weapon = RifleStript;
                if (RifleGameObject) RifleGameObject.SetActive(true);
                CurrentWeapon = "Rifle";
                break;

            case WeaponsStruct.EnumWeaponType.Shotgun:
                weapon = ShotgunStript;
                if (ShotgunGameObject) ShotgunGameObject.SetActive(true);
                CurrentWeapon = "Shotgun";
                break;

            case WeaponsStruct.EnumWeaponType.SMG:
                weapon = SMGStript;
                if (SMGGameObject) SMGGameObject.SetActive(true);
                CurrentWeapon = "SMG";
                break;

            case WeaponsStruct.EnumWeaponType.Sniper:
                weapon = SniperStript;
                if (SniperGameObject) SniperGameObject.SetActive(true);
                CurrentWeapon = "Sniper";
                break;
                #endregion
        }

        #endregion
    }

    private void DisableAllWeaponGameObjects()
    {
        #region Summary
        /// <summary>
        /// Disables all weapon GameObjects to ensure
        /// only one weapon is active at a time
        /// </summary>
        #endregion

        #region DisableAllWeaponGameObjects

        // Melee
        if (ClubObject) ClubObject.SetActive(false);
        if (AxeGameObject) AxeGameObject.SetActive(false);
        if (HammerGameObject) HammerGameObject.SetActive(false);
        if (MaceGameObject) MaceGameObject.SetActive(false);

        // Pistols
        if (GlockGameObject) GlockGameObject.SetActive(false);
        if (DeaserEagleGameObject) DeaserEagleGameObject.SetActive(false);
        if (M9GameObject) M9GameObject.SetActive(false);

        // Guns
        if (RifleGameObject) RifleGameObject.SetActive(false);
        if (ShotgunGameObject) ShotgunGameObject.SetActive(false);
        if (SMGGameObject) SMGGameObject.SetActive(false);
        if (SniperGameObject) SniperGameObject.SetActive(false);

        #endregion
    }

    #endregion

    public void Shoot()
    {
        #region Shoot

        if (weapon == null)
            return;

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

        AiShoot();

        weaponSound.GenerateWeaponSound(weapon.soundRadius);

        #endregion
    }

    public void AiShoot()
    {
        #region Summary
        /// <summary>
        /// Shoots from the weapon muzzle using accurate hitscan logic
        /// </summary>
        #endregion

        #region AiShoot

        if (weapon == null)
            return;

        if (stateMachine.CurrentFollowPoint == null)
            return;

        Vector3 origin = muzzlePoint.position;
        Vector3 direction = muzzlePoint.forward;

        string[] hitableTags =
            HitableObjects.Concat(stateMachine.TargetTags).ToArray();

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

            if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
                damageable.RecieveDamage(weapon.damage, stateMachine.gameObject);
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
        #region Summary
        /// <summary>
        /// Checks if the collider matches any allowed target tags
        /// </summary>
        #endregion

        #region IsValidTarget

        for (int i = 0; i < hitableTags.Length; i++)
        {
            if (collider.CompareTag(hitableTags[i]))
                return true;
        }

        return false;
        #endregion
    }

    private bool TryGetAccurateHit(
        Vector3 origin,
        Vector3 direction,
        string[] hitableTags,
        out RaycastHit finalHit
    )
    {
        #region Summary
        /// <summary>
        /// Uses Raycast first for accuracy,
        /// then SphereCast as fallback aim assist
        /// </summary>
        #endregion

        #region TryGetAccurateHit

        finalHit = new RaycastHit();

        #region Raycast
        if (Physics.Raycast(origin, direction, out RaycastHit rayHit, weapon.range))
        {
            if (IsValidTarget(rayHit.collider, hitableTags))
            {
                finalHit = rayHit;
                return true;
            }
        }
        #endregion

        #region SphereCast
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

    private void SpawnVisualBullet(Vector3 hitPoint)
    {
        #region Summary
        /// <summary>
        /// Spawns and animates a visual bullet toward the hit point
        /// </summary>
        #endregion

        #region SpawnVisualBullet

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
        #region Summary
        /// <summary>
        /// Smoothly moves the bullet toward the target point
        /// </summary>
        #endregion

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
