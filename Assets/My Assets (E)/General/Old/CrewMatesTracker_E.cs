using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CrewMatesTracker_E : MonoBehaviour
{
    public TextMeshProUGUI crewmateCounterText;
    public TextMeshProUGUI imposterCounterText;

    private List<DeathManager_E> allCrewmates = new List<DeathManager_E>();
    private List<DeathManager_E> allImposters = new List<DeathManager_E>();

    private void Start()
    {
        CacheAllCrewmates();
        UpdateText();
    }

    private void Update()
    {
        UpdateText();
    }

    private void CacheAllCrewmates()
    {
        // Find Crew Mates
        GameObject[] crewmateObjects = GameObject.FindGameObjectsWithTag("Crew Mate");
        foreach (GameObject obj in crewmateObjects)
        {
            DeathManager_E dm = obj.GetComponent<DeathManager_E>();
            if (dm != null)
            {
                allCrewmates.Add(dm);
            }
        }

        // Find Imposters
        GameObject[] imposterObjects = GameObject.FindGameObjectsWithTag("Imposter");
        foreach (GameObject obj in imposterObjects)
        {
            DeathManager_E dm = obj.GetComponent<DeathManager_E>();
            if (dm != null)
            {
                allImposters.Add(dm);
            }
        }
    }

    private void UpdateText()
    {
        int aliveCrewmates = 0;
        int aliveImposters = 0;

        foreach (var crewmate in allCrewmates)
        {
            if (crewmate != null && !crewmate.IsDead)
            {
                aliveCrewmates++;
            }
        }

        foreach (var imposter in allImposters)
        {
            if (imposter != null && !imposter.IsDead)
            {
                aliveImposters++;
            }
        }

        crewmateCounterText.text = $"Crewmates Left: {aliveCrewmates} / {allCrewmates.Count}";
        imposterCounterText.text = $"Imposters Left: {aliveImposters} / {allImposters.Count}";
    }
}
