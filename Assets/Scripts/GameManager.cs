using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int totalGenerators = 4;
    public int generatorsFixed = 0;
    public int maxDeaths = 3;
    public int currentDeaths = 0;

    public TextMeshProUGUI statusText;
    public GameObject gateObject;
    public GameObject ghost;
    public GameObject blackoutPanel;

    // Arena bounds (adjust if your arena size changes)
    public float arenaMinX = -20f;
    public float arenaMaxX = 20f;
    public float arenaMinZ = -20f;
    public float arenaMaxZ = 20f;
    public float respawnYLevel = 7f; // Player's Y position on respawn
    public float minRespawnDistanceFromGhost = 30f;
    public int maxRespawnAttempts = 50; // Max attempts to find a suitable respawn spot

    private bool isRespawning = false;
    private Coroutine messageCoroutine;
    private GameObject player;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player GameObject not found. Make sure it's tagged 'Player'.");
        }
    }

    void Start()
    {
        UpdateStatusText();
    }

    public void OnGeneratorFixed()
    {
        generatorsFixed++;
        UpdateStatusText();

        if (generatorsFixed >= totalGenerators)
        {
            if (gateObject != null)
                gateObject.SetActive(false);

            ShowStatusMessage("Escape Gate Open!");
        }
    }

    public void OnPlayerCaught()
    {
        if (isRespawning) return;

        isRespawning = true;
        currentDeaths++;

        if (player != null)
        {
            player.SetActive(false);
        }

        if (currentDeaths >= maxDeaths)
        {
            SetPersistentStatus("You Died.");
            if (blackoutPanel != null) blackoutPanel.SetActive(true);
            Time.timeScale = 0;
            SmartGhostAI ghostAI = ghost.GetComponent<SmartGhostAI>();
            if (ghostAI != null)
                ghostAI.enabled = false;
        }
        else
        {
            StartCoroutine(RespawnWithDelay());
        }
    }

    IEnumerator RespawnWithDelay()
    {
        if (blackoutPanel != null)
            blackoutPanel.SetActive(true);

        ShowStatusMessage("You Were Caught...");

        SmartGhostAI ghostAI = ghost.GetComponent<SmartGhostAI>();
        if (ghostAI != null)
            ghostAI.enabled = false;

        yield return new WaitForSeconds(3f); // Wait for 3 seconds before respawn logic

        Vector3 ghostPos = ghost.transform.position;
        Vector3 respawnPos = Vector3.zero; // Initialize respawnPos
        bool validPositionFound = false;
        int attempts = 0;

        do
        {
            // Generate a random position within the defined arena bounds
            float randomX = Random.Range(arenaMinX, arenaMaxX);
            float randomZ = Random.Range(arenaMinZ, arenaMaxZ);
            respawnPos = new Vector3(randomX, respawnYLevel, randomZ);

            attempts++;

            // Check if the generated position is far enough from the ghost
            if (Vector3.Distance(respawnPos, ghostPos) >= minRespawnDistanceFromGhost)
            {
                validPositionFound = true;
            }

        }
        // Continue looping if a valid position hasn't been found AND max attempts not reached
        while (!validPositionFound && attempts < maxRespawnAttempts);

        if (!validPositionFound)
        {
            // Fallback: If no suitable position is found after max attempts,
            // log a warning and use the last generated position.
            // This position will still be within bounds but might be closer than desired to the ghost.
            Debug.LogWarning($"Could not find a respawn position at least {minRespawnDistanceFromGhost} units away from the ghost after {maxRespawnAttempts} attempts. Using last generated position.");
        }

        if (player != null)
        {
            player.transform.position = respawnPos;
            player.SetActive(true);
        }

        if (ghostAI != null)
            ghostAI.enabled = true;

        if (blackoutPanel != null)
            blackoutPanel.SetActive(false);

        isRespawning = false;
        UpdateStatusText();
    }

    void UpdateStatusText()
    {
        if (generatorsFixed < totalGenerators)
        {
            ShowStatusMessage($"{generatorsFixed}/{totalGenerators} Generators Fixed");
        }
    }

    public void OnPlayerEscaped()
    {
        SetPersistentStatus("You Escaped!");
        if (ghost != null) ghost.SetActive(false);
        Time.timeScale = 0;
        if (player != null)
        {
            player.SetActive(false);
        }
    }

    void ShowStatusMessage(string message)
    {
        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);
        messageCoroutine = StartCoroutine(ShowTemporaryMessage(message));
    }

    void SetPersistentStatus(string message)
    {
        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);
        statusText.text = message;
    }

    IEnumerator ShowTemporaryMessage(string message)
    {
        statusText.text = message;
        yield return new WaitForSeconds(3f);
        if (statusText.text == message) // Only clear if it's still this temporary message
        {
            statusText.text = "";
        }
    }
}