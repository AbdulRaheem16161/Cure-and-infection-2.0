using Game.MyNPC;
using UnityEngine;

public class ZombieTimedDeath : MonoBehaviour
{
    [Header("Death Timer (seconds)")]
    [SerializeField] private float minTime = 3f; // minimum wait
    [SerializeField] private float maxTime = 8f; // maximum wait

    private NPCStateMachine enemyStateMachine;

    private void Awake()
    {
        // Cache reference to the EnemyStateMachine on the same GameObject
        enemyStateMachine = GetComponent<NPCStateMachine>();
    }

    private void Start()
    {
        // Start countdown coroutine
        StartCoroutine(DeathCountdown());
    }

    private System.Collections.IEnumerator DeathCountdown()
    {
        // Pick a random delay between minTime and maxTime
        float waitTime = Random.Range(minTime, maxTime);

        yield return new WaitForSeconds(waitTime);

        // Call death if the state machine is still valid
        if (enemyStateMachine != null)
        {
            enemyStateMachine.Die();
        }
    }
}
