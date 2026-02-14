using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class WiningAndLosingManager_E : MonoBehaviour
{
    private List<DeathManager_E> crewMateDeaths = new List<DeathManager_E>();
    private List<DeathManager_E> imposterDeaths = new List<DeathManager_E>();

    private bool gameEnded = false;
    private bool canCheckGameState = false;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI winningText;
    [SerializeField] private TextMeshProUGUI losingText;
    [SerializeField] private GameObject fadeInCanvas;

    [Header("Game Settings")]
    [SerializeField] private int MinCrewsLeftForGameOver = 0;
    [SerializeField] private float restartDelay = 5f;

    private void Start()
    {
        SetupUI();
        InitializeCrewMates();
        StartCoroutine(InitializeImpostersAndStartChecking());
        fadeInCanvas.SetActive(false);
    }

    private void Update()
    {
        if (canCheckGameState && !gameEnded)
        {
            CheckGameState();
        }
    }

    private void SetupUI()
    {
        if (winningText != null) winningText.gameObject.SetActive(false);
        if (losingText != null) losingText.gameObject.SetActive(false);
    }

    private void InitializeCrewMates()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.CompareTag("Crew Mate"))
            {
                DeathManager_E deathManager = obj.GetComponent<DeathManager_E>();
                if (deathManager != null)
                {
                    crewMateDeaths.Add(deathManager);
                }
            }
        }
    }

    private IEnumerator InitializeImpostersAndStartChecking()
    {
        yield return null;
        yield return null; // Let scene settle

        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.CompareTag("Imposter"))
            {
                DeathManager_E deathManager = obj.GetComponent<DeathManager_E>();
                if (deathManager != null)
                {
                    imposterDeaths.Add(deathManager);
                }
            }
        }

        yield return null;
        canCheckGameState = true;
    }

    private void CheckGameState()
    {
        int aliveCrewmates = 0;
        int aliveImposters = 0;

        foreach (var crew in crewMateDeaths)
        {
            if (crew != null && !crew.IsDead)
                aliveCrewmates++;
        }

        foreach (var imposter in imposterDeaths)
        {
            if (imposter != null && !imposter.IsDead)
                aliveImposters++;
        }

        if (aliveImposters == 0)
        {
            gameEnded = true;
            if (winningText != null) winningText.gameObject.SetActive(true);
            Debug.Log("🎉 You Won!");
            fadeInCanvas.SetActive(true);
            StartCoroutine(RestartGameAfterDelay());
        }
        else if (aliveCrewmates <= MinCrewsLeftForGameOver)
        {
            gameEnded = true;
            if (losingText != null) losingText.gameObject.SetActive(true);
            Debug.Log("💀 You Lost!");
            fadeInCanvas.SetActive(true);
            StartCoroutine(RestartGameAfterDelay());
        }
    }

    private IEnumerator RestartGameAfterDelay()
    {
        // Wait for the specified delay time
        yield return new WaitForSeconds(restartDelay);

        // Reload the current scene to restart the game
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}