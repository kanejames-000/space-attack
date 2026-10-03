using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int Score { get; private set; }

    [Header("Player References")]
    [SerializeField] private Player playerReference;
    [SerializeField] private GameObject playerShipGameObject;

    [Header("UI References")]
    [SerializeField] private Button playButton;
    [SerializeField] private GameObject menuReference;
    [SerializeField] private GameObject gameOverReference;
    [SerializeField] private Button restartButton;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text scoreText;

    [Header("Enemy Spawns")]
    [SerializeField] private Transform spawnPointReference;
    [SerializeField] private GameObject enemyShipPrefab;
    [SerializeField, Min(1)] private int enemyStartingCount = 1;
    public int enemyWavesCleared;
    public List<EnemyShip> spawnedEnemyShips = new List<EnemyShip>();
    [SerializeField] private int nextWaveDelay = 5;

    private bool hasStarted;
    private bool isSpawningWave;
    private bool isGameOver;
    private Coroutine nextWaveRoutine;
    private Vector3 playerStartingPosition;
    private Quaternion playerStartingRotation;

    private void Awake()
    {
        Instance = this;
        if (playerShipGameObject != null)
        {
            playerStartingPosition = playerShipGameObject.transform.position;
            playerStartingRotation = playerShipGameObject.transform.rotation;
        }
    }

    private void Start()
    {
        GameInit();
    }

    /// <summary>
    /// On Init of game launch.
    /// </summary>
    private void GameInit()
    {
        isGameOver = false;
        hasStarted = false;
        playerShipGameObject.SetActive(false);
        menuReference.SetActive(true);
        gameOverReference.SetActive(false);

        PlayerShip playerShip = playerShipGameObject.GetComponent<PlayerShip>();
        if (playerShip != null)
        {
            UpdatePlayerHealth(playerShip.Health);
        }

        UpdateScoreDisplay();

        playButton.onClick.AddListener(OnPlay);
        restartButton.onClick.AddListener(OnRestart);
    }

    /// <summary>
    /// Controls Play button press, sets up scene to default.
    /// </summary>
    private void OnPlay()
    {
        if (hasStarted)
        {
            return;
        }

        hasStarted = true;
        playerShipGameObject.SetActive(true);
        menuReference.SetActive(false);
        PlayerShip playerShip = playerShipGameObject.GetComponent<PlayerShip>();
        if (playerShip != null)
        {
            UpdatePlayerHealth(playerShip.Health);
        }

        SpawnWave();
    }

    private void OnRestart()
    {
        StopNextWaveRoutine();
        RemoveAllEnemies();

        Score = 0;
        enemyWavesCleared = 0;
        UpdateScoreDisplay();

        isGameOver = false;
        hasStarted = true;
        gameOverReference.SetActive(false);
        menuReference.SetActive(false);

        PlayerShip playerShip = playerShipGameObject.GetComponent<PlayerShip>();
        if (playerShip != null)
        {
            playerShip.ResetHealth();
            playerShip.transform.SetPositionAndRotation(playerStartingPosition, playerStartingRotation);
            Rigidbody2D playerRigidbody = playerShip.GetComponent<Rigidbody2D>();
            if (playerRigidbody != null)
            {
                playerRigidbody.velocity = Vector2.zero;
                playerRigidbody.angularVelocity = 0f;
            }

            playerShipGameObject.SetActive(true);
            UpdatePlayerHealth(playerShip.Health);
        }

        SpawnWave();
    }

    public void ShowGameOver()
    {
        if (isGameOver)
        {
            return;
        }

        isGameOver = true;
        StopNextWaveRoutine();
        RemoveAllEnemies();
        gameOverReference.SetActive(true);
    }

    private void StopNextWaveRoutine()
    {
        if (nextWaveRoutine != null)
        {
            StopCoroutine(nextWaveRoutine);
            nextWaveRoutine = null;
        }
    }

    private void RemoveAllEnemies()
    {
        // Copy first because destroying ships unregisters them from this list.
        EnemyShip[] enemies = spawnedEnemyShips.ToArray();
        spawnedEnemyShips.Clear();

        foreach (EnemyShip enemy in enemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }
    }

    public void UpdatePlayerHealth(int health)
    {
        if (healthText != null)
        {
            healthText.text = $"Health: {health}";
        }
    }

    public void AddScore(int points)
    {
        Score += points;
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {Score}";
        }
    }

    private void SpawnWave()
    {
        if (isGameOver || !hasStarted)
        {
            return;
        }

        if (enemyShipPrefab == null)
        {
            Debug.LogError("GameManager needs an enemy ship prefab to spawn waves.", this);
            return;
        }

        int enemyCount = Mathf.Max(1, enemyStartingCount + enemyWavesCleared * 2);
        Vector3 spawnPosition = spawnPointReference != null
            ? spawnPointReference.position
            : new Vector3(0f, 4f, 0f);

        isSpawningWave = true;
        float horizontalSpacing = enemyCount > 1
            ? Mathf.Min(1.5f, 18f / (enemyCount - 1))
            : 0f;
        for (int i = 0; i < enemyCount; i++)
        {
            // Spread the wave across the play area instead of stacking every ship at one point.
            float horizontalOffset = (i - (enemyCount - 1) * 0.5f) * horizontalSpacing;
            Vector3 position = spawnPosition + Vector3.right * horizontalOffset;
            EnemyShip enemyShip = Instantiate(enemyShipPrefab, position, Quaternion.identity).GetComponent<EnemyShip>();

            if (enemyShip == null)
            {
                Debug.LogError("The enemy ship prefab needs an EnemyShip component.", enemyShipPrefab);
                continue;
            }

            RegisterEnemyShip(enemyShip);
        }

        isSpawningWave = false;
        if (spawnedEnemyShips.Count == 0)
        {
            Debug.LogError("No EnemyShip instances were added to the wave.", this);
        }
    }

    public void RegisterEnemyShip(EnemyShip enemyShip)
    {
        if (enemyShip != null && !spawnedEnemyShips.Contains(enemyShip))
        {
            spawnedEnemyShips.Add(enemyShip);
        }
    }

    public void UnregisterEnemyShip(EnemyShip enemyShip)
    {
        if (ReferenceEquals(enemyShip, null))
        {
            return;
        }

        spawnedEnemyShips.Remove(enemyShip);
        if (hasStarted && !isGameOver && !isSpawningWave && spawnedEnemyShips.Count == 0 && nextWaveRoutine == null)
        {
            nextWaveRoutine = StartCoroutine(SpawnNextWaveAfterDelay());
        }
    }

    private IEnumerator SpawnNextWaveAfterDelay()
    {
        enemyWavesCleared++;
        yield return new WaitForSeconds(nextWaveDelay);

        nextWaveRoutine = null;
        if (!isGameOver && hasStarted)
        {
            SpawnWave();
        }
    }
}
