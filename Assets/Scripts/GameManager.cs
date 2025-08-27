using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Dan.Main;

[System.Serializable]
public class EnemySpawnData
{
    [Header("Enemy Configuration")]
    public GameObject enemyPrefab;
    public Vector3 spawnPosition;
    public float moveSpeed = 2f;
    public float shootIntervalMin = 5f;
    public float shootIntervalMax = 10f;
}

[System.Serializable]
public class LevelData
{
    [Header("Level Info")]
    public string levelName = "Level 1";
    
    [Header("Enemy Grid Configuration (Optional - for grid-based spawning)")]
    public bool useGridSpawning = true;
    public int rows = 3;
    public int cols = 5;
    public float spacing = 2f;
    public GameObject gridEnemyPrefab;
    public float gridEnemySpeed = 2f;
    public float gridShootIntervalMin = 5f;
    public float gridShootIntervalMax = 10f;
    
    [Header("Custom Enemy Spawns (Optional - for specific positioning)")]
    public List<EnemySpawnData> customEnemies = new List<EnemySpawnData>();
}

[System.Serializable]
public class BonusRoundData
{
    [Header("Bonus Round Configuration")]
    public GameObject bonusEnemyPrefab;
    public int bonusRows = 2;
    public int bonusCols = 6;
    public float bonusSpacing = 2f;
    public float bonusEnemySpeed = 5f;
    public float bonusShootIntervalMin = 3f;
    public float bonusShootIntervalMax = 6f;
    public float bonusRoundDuration = 15f;
}

public class GameManager : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource bgmSource;
    public AudioSource mysterySource; 
    public AudioClip bonusSFX; 
    
    // MODIFY the Player Model Prefabs section in GameManager class:
    [Header("Player Model Prefabs")]
    [SerializeField] private GameObject defaultCloth;    // Default player model
    [SerializeField] private GameObject cloth1Prefab;    // Cloth 1 model  
    [SerializeField] private GameObject cloth2Prefab;    // Cloth 2 model
    [SerializeField] private GameObject currentPlayerInstance;

    [Header("Bullet Prefabs (Keris)")]
    [SerializeField] private GameObject defaultKeris;    // Default bullet
    [SerializeField] private GameObject keris1Prefab;    // Keris 1 bullet

    [Header("Level Configuration")]
    public List<LevelData> levels = new List<LevelData>();
    public BonusRoundData bonusRoundConfig;
    public int currentLevel = 0;

    [Header("UI Elements")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI levelText1;
    public TextMeshProUGUI bonusCountdownText;
    public TextMeshProUGUI bonusBannerText;
    public TextMeshProUGUI coinsText;
    public GameObject coinText;
    private int totalCoins = 0;
    public TextMeshProUGUI powerUpStatusText;
    public GameObject gameOverPanel;
    public GameObject winPanel;
    public GameObject pausePanel;

    [Header("Bonus and Shields")]
    public GameObject bonusUFO;
    public GameObject shieldPrefab;

    private float nextSpawnTime;
    private int score = 0;
    public bool isGameOver = false;

    [Header("Game Settings")]
    public int coinsPerPoint = 10;

    private int levelStartHealth;
    private bool inBonusRound = false;
    public bool doubleScoreActive = false;

    private List<Vector3> shieldSpawnPositions = new List<Vector3>();
    private List<GameObject> activeShields = new List<GameObject>();

    private bool heartLostDuringLevel = false;

    private int startingHealth;
    private bool healthLostThisGame = false;
    private int powerUpsCollectedThisGame = 0;

    [Header("Power-up System")]
    private bool enemySlowdownActive = false;
    private float originalEnemySpeedMultiplier = 1f;
    private bool doubleScoreFromPowerupActive = false;

    void Start()
    {
        Debug.Log("GameManager Start: CurrentKeris = " + PlayerPrefs.GetString("CurrentKeris", "NOT SET"));
        Debug.Log("GameManager Start: CurrentCloth = " + PlayerPrefs.GetString("CurrentCloth", "NOT SET"));

        // Validate level configuration
        if (levels.Count == 0)
        {
            Debug.LogError("No levels configured in GameManager! Please add levels to the levels array.");
            return;
        }
        
        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                startingHealth = playerController.GetHealth();
                healthLostThisGame = false;
                powerUpsCollectedThisGame = 0;
            }
        }

        SpawnPlayerWithSelectedModel();
        UpdatePlayerBulletPrefab();
        SpawnEnemies();
        UpdateScoreUI();
        nextSpawnTime = Time.time + Random.Range(10f, 20f);

        // Initialize shield positions
        shieldSpawnPositions.Add(new Vector3(1.02f, 0.5f, -5f));
        shieldSpawnPositions.Add(new Vector3(5.52f, 0.5f, -5f));
        shieldSpawnPositions.Add(new Vector3(-3.86f, 0.5f, -5f));
        shieldSpawnPositions.Add(new Vector3(-8.74f, 0.5f, -5f));

        SpawnShields();
    }
    
    void SpawnPlayerWithSelectedModel()
    {
        // Get selected cloth prefab
        GameObject selectedClothPrefab = GetCurrentClothPrefab();

        if (selectedClothPrefab != null)
        {
            // Spawn new player with selected model
            Instantiate(selectedClothPrefab, currentPlayerInstance.transform);
        }
    }

    void UpdatePlayerBulletPrefab()
    {
        if (currentPlayerInstance != null)
        {
            PlayerController playerController = currentPlayerInstance.GetComponent<PlayerController>();
            if (playerController != null)
            {
                GameObject currentKeris = GetCurrentKerisPrefab();
                playerController.SetBulletPrefab(currentKeris);
                Debug.Log($"Updated player bullet to: {currentKeris.name}");
            }
        }
    }

    public GameObject GetCurrentKerisPrefab()
    {
        string currentKerisID = PlayerPrefs.GetString("CurrentKeris", "defaultKeris");

        switch (currentKerisID)
        {
            case "kerisRed":
                return keris1Prefab != null ? keris1Prefab : defaultKeris;
            case "defaultKeris":
            default:
                return defaultKeris;
        }
    }

    // Method to get current cloth prefab based on selected cloth
    GameObject GetCurrentClothPrefab()
    {        
        string currentClothID = PlayerPrefs.GetString("CurrentCloth", "defaultCloth");
        
        switch (currentClothID)
        {
            case "warriorRed":
                return cloth1Prefab != null ? cloth1Prefab : defaultCloth;
            case "warriorGreen":
                return cloth2Prefab != null ? cloth2Prefab : defaultCloth;
            case "defaultCloth":
            default:
                return defaultCloth;
        }
    }

    public void SpawnEnemies()
    {
        if (currentLevel >= levels.Count)
        {
            Debug.LogError($"Current level {currentLevel} exceeds configured levels count {levels.Count}!");
            return;
        }

        LevelData levelData = levels[currentLevel];

        // Update level UI
        if (levelText != null)
            levelText.text = levelData.levelName;

        // Update level UI
        if (levelText1 != null)
            levelText1.text = levelData.levelName;

        // Spawn grid-based enemies if enabled
        if (levelData.useGridSpawning && levelData.gridEnemyPrefab != null)
        {
            SpawnGridEnemies(levelData);
        }

        // Spawn custom positioned enemies
        SpawnCustomEnemies(levelData);

        // Store starting health for this level
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                levelStartHealth = playerController.GetHealth();
            }
        }
        heartLostDuringLevel = false;
    }

    void SpawnGridEnemies(LevelData levelData)
    {
        for (int r = 0; r < levelData.rows; r++)
        {
            for (int c = 0; c < levelData.cols; c++)
            {
                Vector3 pos = new Vector3((c - (levelData.cols - 1) * 0.5f) * levelData.spacing, 0.5f, 5 + r * levelData.spacing);
                GameObject enemy = Instantiate(levelData.gridEnemyPrefab, pos, Quaternion.identity);

                Enemy enemyComponent = enemy.GetComponent<Enemy>();
                if (enemyComponent != null)
                {
                    enemyComponent.moveSpeed = levelData.gridEnemySpeed;
                    enemyComponent.shootIntervalMin = levelData.gridShootIntervalMin;
                    enemyComponent.shootIntervalMax = levelData.gridShootIntervalMax;
                    enemyComponent.gameManager = this;
                }
                else
                {
                    Debug.LogError("Grid enemy prefab does not have an Enemy component!");
                }
            }
        }
    }

    void SpawnCustomEnemies(LevelData levelData)
    {
        foreach (EnemySpawnData enemyData in levelData.customEnemies)
        {
            if (enemyData.enemyPrefab == null)
            {
                Debug.LogWarning("Custom enemy prefab is null, skipping spawn.");
                continue;
            }

            GameObject enemy = Instantiate(enemyData.enemyPrefab, enemyData.spawnPosition, Quaternion.identity);

            Enemy enemyComponent = enemy.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                enemyComponent.moveSpeed = enemyData.moveSpeed;
                enemyComponent.shootIntervalMin = enemyData.shootIntervalMin;
                enemyComponent.shootIntervalMax = enemyData.shootIntervalMax;
                enemyComponent.gameManager = this;
            }
            else
            {
                Debug.LogError("Custom enemy prefab does not have an Enemy component!");
            }
        }
    }

    void Update()
    {
        if (!isGameOver && !inBonusRound && GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            if (!heartLostDuringLevel)
            {
                StartCoroutine(StartBonusRound());
            }
            else if (currentLevel < levels.Count - 1)
            {
                currentLevel++;
                RestorePlayerHealth(); // Restore health when advancing to next level
                SpawnEnemies();
                SpawnShields();
            }
            else
            {
                WinGame();
            }
        }

        if (!isGameOver && Time.time >= nextSpawnTime)
        {
            SpawnBonusUFO();
            nextSpawnTime = Time.time + Random.Range(10f, 20f);
        }
    }

    public void UpdateHealthUI(int health)
    {
        if (healthText != null)
            healthText.text = "Health: " + health;

        if (health < startingHealth)
        {
            healthLostThisGame = true;
        }

        if (health < levelStartHealth)
            heartLostDuringLevel = true;
    }

    public void AddScore(int points)
    {
        if (doubleScoreActive || doubleScoreFromPowerupActive)
            points *= 2;

        score += points;
        UpdateScoreUI();
        UpdateCoinsUI();

        CheckScoreAchievements();
    }

    void CheckScoreAchievements()
    {
        // Rookie Hero - Score 100 points
        if (score >= 100 && PlayerPrefs.GetInt("Achievement_RookieHero", 0) == 0)
        {
            PlayerPrefs.SetInt("Achievement_RookieHero", 1);
            PlayerPrefs.Save();
            Debug.Log("Achievement Unlocked: Rookie Hero!");
        }

        // Master Hero - Score 1000 points
        if (score >= 1000 && PlayerPrefs.GetInt("Achievement_MasterHero", 0) == 0)
        {
            PlayerPrefs.SetInt("Achievement_MasterHero", 1);
            PlayerPrefs.Save();
            Debug.Log("Achievement Unlocked: Master Hero!");
        }

        // Hang Tuah himself - Score 2000 points
        if (score >= 2000 && PlayerPrefs.GetInt("Achievement_HangTuah", 0) == 0)
        {
            PlayerPrefs.SetInt("Achievement_HangTuah", 1);
            PlayerPrefs.Save();
            Debug.Log("Achievement Unlocked: Hang Tuah himself!");
        }
    }

    public void UpdateCoinsUI()
    {
        if (coinsText != null)
            coinsText.text = "Coins: " + (score / coinsPerPoint);
    }

    public void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    public void GameOver()
    {
        isGameOver = true;
        Time.timeScale = 1f;
        SubmitToLeaderboard();
        CheckFinalAchievements();
        SaveCoinsToPlayerPrefs();
        coinText.SetActive(true);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        if (bgmSource != null && bgmSource.isPlaying)
            bgmSource.Stop();
        
    }
    
    private void SubmitToLeaderboard()
    {
        string publicLeaderboardKey = "cd2021ff4cadce1270eb736b9c259840e17a4adde2323e1e388af9ee82b436dc";
        string playerName = "Unknown Player";

        // Try to get player name from PlayerPrefs
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            playerName = PlayerPrefs.GetString("PlayerName");
        }
        
        LeaderboardCreator.UploadNewEntry(publicLeaderboardKey, playerName, score, ((msg) => {
                Leaderboards.KerisWarrior.ResetPlayer();
        }));
    }

    public void SaveCoinsToPlayerPrefs()
    {
        int currentGameCoins = score / coinsPerPoint;
        totalCoins += currentGameCoins;

        // Save to PlayerPrefs
        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        PlayerPrefs.Save();

        Debug.Log($"Coins saved: {currentGameCoins} earned this game, {totalCoins} total coins");
    }

    void CheckFinalAchievements()
    {
        // Untouchable - Complete an entire game without losing health
        if (!healthLostThisGame && PlayerPrefs.GetInt("Achievement_Untouchable", 0) == 0)
        {
            PlayerPrefs.SetInt("Achievement_Untouchable", 1);
            PlayerPrefs.Save();
            Debug.Log("Achievement Unlocked: Untouchable!");
        }

        // Bonus Hunter - Get 5 powerups in a single game
        if (powerUpsCollectedThisGame >= 5 && PlayerPrefs.GetInt("Achievement_BonusHunter", 0) == 0)
        {
            PlayerPrefs.SetInt("Achievement_BonusHunter", 1);
            PlayerPrefs.Save();
            Debug.Log("Achievement Unlocked: Bonus Hunter!");
        }
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    void WinGame()
    {
        isGameOver = true;
        SubmitToLeaderboard();
        CheckFinalAchievements();
        SaveCoinsToPlayerPrefs();
        coinText.SetActive(true);
        if (winPanel != null)
            winPanel.SetActive(true);
        if (bgmSource != null && bgmSource.isPlaying)
            bgmSource.Stop();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
    }
    
    public void ContinueGame()
    {
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
    }

    void SpawnBonusUFO()
    {
        if (bonusUFO == null)
        {
            Debug.LogWarning("Cannot spawn bonus UFO: bonusUFO prefab is not assigned!");
            return;
        }

        Vector3 spawnPos = new Vector3(-10f, 1f, 5f);
        GameObject ufo = Instantiate(bonusUFO, spawnPos, Quaternion.identity);

        if (ufo != null)
        {
            BonusUFO bonusComponent = ufo.GetComponent<BonusUFO>();
            if (bonusComponent != null)
            {
                bonusComponent.SetManager(this);
            }
            else
            {
                Debug.LogError("Bonus UFO prefab does not have a BonusUFO component!");
            }
        }
    }

    public void ActivatePowerUp()
    {
        powerUpsCollectedThisGame++;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found!");
            return;
        }

        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("Player does not have a PlayerController component!");
            return;
        }

        // Randomize power-up type (0-3)
        int powerUpType = Random.Range(0, 4);

        switch (powerUpType)
        {
            case 0: // Double bullets (current powerup)
                playerController.EnablePowerUp(5);
                UpdatePowerUpUI("Double Bullets Active!", 0f);
                Debug.Log("Power-up: Double Bullets!");
                break;

            case 1: // Decrease shooting cooldown
                StartCoroutine(ReduceShootingCooldown(playerController));
                Debug.Log("Power-up: Fast Shooting!");
                break;

            case 2: // Slow down enemy movement
                StartCoroutine(SlowDownEnemies());
                Debug.Log("Power-up: Enemy Slowdown!");
                break;

            case 3: // Double scoring
                StartCoroutine(DoubleScorePowerUp());
                Debug.Log("Power-up: Double Score!");
                break;
        }
    }

    IEnumerator StartBonusRound()
    {
        inBonusRound = true;
        doubleScoreActive = true;

        if (bonusBannerText != null)
            bonusBannerText.gameObject.SetActive(true);

        SpawnBonusEnemies();
        mysterySource.PlayOneShot(bonusSFX);
        float timeRemaining = bonusRoundConfig.bonusRoundDuration;

        // Continue while there's time remaining AND enemies are still alive
        while (timeRemaining > 0 && GameObject.FindGameObjectsWithTag("Enemy").Length > 0)
        {
            if (bonusCountdownText != null)
                bonusCountdownText.text = "Bonus Time: " + Mathf.CeilToInt(timeRemaining) + "s";

            timeRemaining -= Time.deltaTime;
            yield return null;
        }

        // Check if bonus round ended because all enemies were defeated
        bool allEnemiesDefeated = GameObject.FindGameObjectsWithTag("Enemy").Length == 0;

        if (allEnemiesDefeated && bonusCountdownText != null)
        {
            mysterySource.PlayOneShot(bonusSFX);
            bonusCountdownText.text = "All Bonus Enemies Defeated!";
            yield return new WaitForSeconds(2f); // Show message for 1 second
        }

        if (bonusCountdownText != null)
            bonusCountdownText.text = "";

        if (bonusBannerText != null)
            bonusBannerText.gameObject.SetActive(false);

        // Clean up any remaining enemies (in case timer ran out)
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }

        doubleScoreActive = false;
        inBonusRound = false;

        if (currentLevel < levels.Count - 1)
        {
            currentLevel++;
            SpawnEnemies();
            SpawnShields();
        }
        else
        {
            WinGame();
        }
    }

    void SpawnBonusEnemies()
    {
        if (bonusRoundConfig.bonusEnemyPrefab == null)
        {
            Debug.LogError("Cannot spawn bonus enemies: bonusEnemyPrefab is not assigned in bonusRoundConfig!");
            return;
        }

        for (int r = 0; r < bonusRoundConfig.bonusRows; r++)
        {
            for (int c = 0; c < bonusRoundConfig.bonusCols; c++)
            {
                Vector3 pos = new Vector3(-5 + c * bonusRoundConfig.bonusSpacing, 0.0f, 7 + r * bonusRoundConfig.bonusSpacing);
                GameObject enemy = Instantiate(bonusRoundConfig.bonusEnemyPrefab, pos, Quaternion.identity);

                Enemy enemyComponent = enemy.GetComponent<Enemy>();
                if (enemyComponent != null)
                {
                    enemyComponent.moveSpeed = bonusRoundConfig.bonusEnemySpeed;
                    // Disable shooting for bonus enemies by setting very high intervals
                    enemyComponent.shootIntervalMin = 999f;
                    enemyComponent.shootIntervalMax = 999f;
                    enemyComponent.gameManager = this;
                }
                else
                {
                    Debug.LogError("Bonus enemy prefab does not have an Enemy component!");
                }
            }
        }
    }

    void SpawnShields()
    {
        if (shieldPrefab == null)
        {
            Debug.LogWarning("Cannot spawn shields: shieldPrefab is not assigned!");
            return;
        }

        // Clean up existing shields
        foreach (GameObject shield in activeShields)
        {
            if (shield != null)
                Destroy(shield);
        }
        activeShields.Clear();

        // Spawn new shields
        foreach (Vector3 pos in shieldSpawnPositions)
        {
            GameObject shield = Instantiate(shieldPrefab, pos, Quaternion.identity);
            if (shield != null)
            {
                activeShields.Add(shield);
            }
        }
    }

    // Getter for score (useful for other scripts)
    public int GetScore()
    {
        return score;
    }

    // Getter for coins (useful for other scripts)
    public int GetCoins()
    {
        return score / coinsPerPoint;
    }

    // Get current level info
    public LevelData GetCurrentLevelData()
    {
        if (currentLevel < levels.Count)
            return levels[currentLevel];
        return null;
    }

    // Restore player health to maximum when advancing to next level
    void RestorePlayerHealth()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.RestoreFullHealth();
                Debug.Log("Player health restored to maximum!");
            }
            else
            {
                Debug.LogError("Player does not have a PlayerHealth component!");
            }
        }
    }

    // Power-up: Reduce shooting cooldown for 5 seconds
    IEnumerator ReduceShootingCooldown(PlayerController playerController)
    {
        float originalCooldown = playerController.shootCooldown;
        playerController.SetShootCooldown(0.1f); // Very fast shooting

        float timeRemaining = 5f;
        while (timeRemaining > 0)
        {
            UpdatePowerUpUI("Fast Shooting", timeRemaining);
            timeRemaining -= Time.deltaTime;
            yield return null;
        }

        playerController.SetShootCooldown(originalCooldown); // Restore original cooldown
        ClearPowerUpUI();
    }

    // Power-up: Slow down all enemies for 5 seconds
    IEnumerator SlowDownEnemies()
    {
        enemySlowdownActive = true;

        // Get all enemies and slow them down
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Enemy enemyComponent = enemy.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                enemyComponent.moveSpeed *= 0.5f; // Halve the speed
            }
        }

        float timeRemaining = 5f;
        while (timeRemaining > 0)
        {
            UpdatePowerUpUI("Enemy Slowdown", timeRemaining);
            timeRemaining -= Time.deltaTime;
            yield return null;
        }

        // Restore original speed
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                Enemy enemyComponent = enemy.GetComponent<Enemy>();
                if (enemyComponent != null)
                {
                    enemyComponent.moveSpeed *= 2f; // Restore original speed
                }
            }
        }

        enemySlowdownActive = false;
        ClearPowerUpUI();
    }

    // Power-up: Double scoring for 5 seconds
    IEnumerator DoubleScorePowerUp()
    {
        doubleScoreFromPowerupActive = true;
        
        float timeRemaining = 5f;
        while (timeRemaining > 0)
        {
            UpdatePowerUpUI("Double Score", timeRemaining);
            timeRemaining -= Time.deltaTime;
            yield return null;
        }
        
        doubleScoreFromPowerupActive = false;
        ClearPowerUpUI();
    }
    
        void UpdatePowerUpUI(string powerUpName, float timeRemaining)
    {
        if (powerUpStatusText != null)
        {
            if (timeRemaining > 0)
            {
                powerUpStatusText.text = $"{powerUpName}: {Mathf.CeilToInt(timeRemaining)}s";
            }
            else
            {
                powerUpStatusText.text = powerUpName; // For shot-based power-ups like Double Bullets
            }
        }
    }

    // Clear power-up UI text
    public void ClearPowerUpUI()
    {
        if (powerUpStatusText != null)
        {
            powerUpStatusText.text = "";
        }
    }

        public void QuitGame()
    {
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit();
    #endif
    }
}