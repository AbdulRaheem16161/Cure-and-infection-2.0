using Game.MyPlayer;
using TMPro;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    #region References
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private PlayerStateMachine stateMachine;
    #endregion

    private void Update()
    {
        #region UpdateAmmoUI
        if (stateMachine == null || stateMachine.GunManager == null || stateMachine.GunManager.weapon == null)
            return;

        ammoText.text = $"{stateMachine.GunManager.weapon.currentAmmo} / {stateMachine.GunManager.weapon.ammoCapacity}    -    {stateMachine.GunManager.weapon.totalAmmo}";
        #endregion
    }

}
