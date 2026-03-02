using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.MyNPC;
using Game.MyPlayer;
using Game.Core;

public class WeaponSound : MonoBehaviour
{
    #region References
    public StateMachine stateMachine;
    #endregion

    #region Settings
    [Header("Sound Settings")]
    [SerializeField] private float soundDuration = 0.5f;

    [Header("Gizmo Settings")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color defaultColor = Color.yellow;
    [SerializeField] private Color detectedColor = Color.red;
    #endregion

    #region Runtime
    public float currentRadius;
    public bool gizmoActive;
    public bool targetDetected;
    #endregion

    public void GenerateWeaponSound(float soundRadius)
    {
        #region Generate Gun Sound

        currentRadius = soundRadius;
        StartCoroutine(GunSoundRoutine());

        #endregion
    }

    private IEnumerator GunSoundRoutine()
    {
        #region Gun Sound Routine
        gizmoActive = true;
        targetDetected = false;

        Collider[] hits = Physics.OverlapSphere(transform.position, currentRadius);

        foreach (Collider hit in hits)
        {
            // Skip if tag is not in target list
            if (!stateMachine.TargetTags.Contains(hit.tag))
                continue;

            // Try to get NPC StateMachine
            if (!hit.TryGetComponent(out NPCStateMachine otherStateMachine))
                continue;

            // Trigger alert mode
            otherStateMachine.DetectionRadius.EnableAlertMode(stateMachine.gameObject);
            otherStateMachine.DetectionCone.EnableAlertMode(stateMachine.gameObject);

            targetDetected = true;
        }

        yield return new WaitForSeconds(soundDuration);

        gizmoActive = false;
        #endregion
    }

    private void OnDrawGizmos()
    {
        #region Draw Gizmos
        if (!showGizmos || !gizmoActive)
            return;

        Gizmos.color = targetDetected ? detectedColor : defaultColor;
        Gizmos.DrawWireSphere(transform.position, currentRadius);
        #endregion
    }
}
