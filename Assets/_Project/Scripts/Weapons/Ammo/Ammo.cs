using Game.MyPlayer;
using UnityEngine;
using System.Collections.Generic;

public class Ammo : MonoBehaviour
{
    #region References
    public PlayerStateMachine stateMachine;
    #endregion

    #region Settings
    public int ammoAmount;
    public List<string> Teams = new List<string> { "Team 1", "Team 2", "Team 3", "Team 4", "Team 5", "Team 6" };
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        #region OnTriggerEnter
        if (Teams.Contains(other.tag))
        {
            if (other.TryGetComponent<PlayerStateMachine>(out stateMachine))
            {
                stateMachine.GunManager.weapon.totalAmmo += ammoAmount;
            }

            Destroy(this.gameObject);
        }
        #endregion
    }
}
