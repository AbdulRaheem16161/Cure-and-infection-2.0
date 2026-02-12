using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NavAgentController_E))]
public class Imposter_E : MonoBehaviour
{
    [Header("🔪 Kill Settings")]
    [SerializeField] private float killRange = 2f;

    private NavAgentController_E agent;
    private GameObject currentTarget;
    private Coroutine checkTargetStatusCoroutine;

    public bool stopKilling = false;
    private bool isItsTargetDead;

    #region Unity Lifecycle

    private void Update()
    {
        HandleKillAttempt();
    }

    #endregion

    #region Initialization

    public void Initialize()
    {
        agent = GetComponent<NavAgentController_E>();
        ChooseNewTarget();
    }

    #endregion

    #region Target Management

    public void ChooseNewTarget()
    {
        GameObject newTarget = FindRandomCrewMate();

        if (newTarget == null)
        {
            Debug.LogWarning("[Imposter_E] No valid targets.");
            currentTarget = null;
            return;
        }

        SetNewTarget(newTarget);
    }

    private GameObject FindRandomCrewMate()
    {
        var crewMates = new List<GameObject>(GameObject.FindGameObjectsWithTag("Crew Mate"));
        crewMates.Remove(gameObject); // Don’t target yourself

        if (crewMates.Count == 0)
            return null;

        return crewMates[Random.Range(0, crewMates.Count)];
    }

    private void SetNewTarget(GameObject target)
    {
        currentTarget = target;
        agent.target = target.transform;

        Debug.Log($"[Imposter_E] Now targeting: {currentTarget.name}");

        RestartTargetStatusCheck();
    }

    private void RestartTargetStatusCheck()
    {
        if (checkTargetStatusCoroutine != null)
            StopCoroutine(checkTargetStatusCoroutine);

        checkTargetStatusCoroutine = StartCoroutine(CheckTargetStatus());
    }

    #endregion

    #region Kill Handling

    private void HandleKillAttempt()
    {
        if (currentTarget == null || stopKilling) return;

        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
        if (distance <= killRange)
        {
            TryKill(currentTarget);
        }
    }

    private void TryKill(GameObject target)
    {
        var deathManager = target.GetComponent<DeathManager_E>();
        if (deathManager == null)
        {
            Debug.LogWarning("[Imposter_E] Target has no DeathManager_E!");
            return;
        }

        deathManager.Die();

        if (deathManager.IsDead)
        {
            Debug.Log($"[Imposter_E] Eliminated {target.name}");
            StartCoroutine(DelayedTargetSelection());
        }
    }

    #endregion

    #region Status Monitoring

    private IEnumerator CheckTargetStatus()
    {
        while (currentTarget != null)
        {
            var deathManager = currentTarget.GetComponent<DeathManager_E>();
            isItsTargetDead = deathManager.IsDead;

            if (deathManager != null && deathManager.IsDead)
            {
                Debug.Log($"[Imposter_E] Target {currentTarget.name} is already dead. Retargeting...");
                StartCoroutine(DelayedTargetSelection());
                yield break;
            }

            yield return null; // Wait for next frame
        }
    }

    #endregion

    #region Utility

    private IEnumerator DelayedTargetSelection()
    {
        yield return null; // wait one frame
        yield return null; // ...and another frame

        ChooseNewTarget();
    }

    #endregion
}
