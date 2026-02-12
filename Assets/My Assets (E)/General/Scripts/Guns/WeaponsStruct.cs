using UnityEngine;

public static class WeaponsStruct 
{
    public enum EnumWeaponType
    {
        #region Melee
        Club,
        Axe,
        Hammer,
        Mace,
        #endregion

        #region Pistols
        Glock,
        DeaserEagle,
        M9,
        #endregion

        #region Guns
        Rifle,
        Shotgun,
        SMG,
        Sniper
        #endregion
    }
}
