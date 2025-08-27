using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementManager : MonoBehaviour
{
    [System.Serializable]
    public class AchievementUI
    {
        public string achievementName;
        public string description;
        public string playerPrefKey;
        public GameObject achievementObject;  // The main achievement GameObject
        public GameObject tickImage;          // The tick image that shows when achieved
        public TextMeshProUGUI achievementText; // The achievement text
    }

    [Header("Achievement UI Elements")]
    public AchievementUI[] achievements = new AchievementUI[5];

    [Header("Achievement Panel")]
    public GameObject achievementPanel;

    [Header("Visual Feedback Options")]
    public Color unlockedTextColor = Color.green;
    public Color lockedTextColor = Color.white;

    void Start()
    {
        // Initialize achievement data
        SetupAchievementData();
        
        // Update UI on start
        UpdateAchievementUI();
    }
    
    public void Show()
    {
        if (achievementPanel != null)
        {
            achievementPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Achievement Doesn't Exist!");
        }
    }

    public void Hide()
    {
        if (achievementPanel != null)
        {
            achievementPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Achievement Doesn't Exist!");
        }
    }

    void SetupAchievementData()
    {
        achievements[0].achievementName = "Rookie Hero";
        achievements[0].description = "Score 100 points";
        achievements[0].playerPrefKey = "Achievement_RookieHero";

        achievements[1].achievementName = "Master Hero";
        achievements[1].description = "Score 1000 points";
        achievements[1].playerPrefKey = "Achievement_MasterHero";

        achievements[2].achievementName = "Hang Tuah himself";
        achievements[2].description = "Score 2000 points";
        achievements[2].playerPrefKey = "Achievement_HangTuah";

        achievements[3].achievementName = "Untouchable";
        achievements[3].description = "Complete game without losing health";
        achievements[3].playerPrefKey = "Achievement_Untouchable";

        achievements[4].achievementName = "Bonus Hunter";
        achievements[4].description = "Get 5 powerups in a single game";
        achievements[4].playerPrefKey = "Achievement_BonusHunter";
    }

    public void UpdateAchievementUI()
    {
        for (int i = 0; i < achievements.Length; i++)
        {
            AchievementUI achievement = achievements[i];
            
            if (achievement.achievementObject == null || achievement.achievementText == null)
            {
                Debug.LogWarning($"Achievement {i} UI elements not assigned!");
                continue;
            }

            // Check if achievement is unlocked
            bool isUnlocked = PlayerPrefs.GetInt(achievement.playerPrefKey, 0) == 1;
            
            // Show/hide tick image based on achievement status
            if (achievement.tickImage != null)
            {
                achievement.tickImage.SetActive(isUnlocked);
            }
            else
            {
                Debug.LogWarning($"Tick image not assigned for achievement: {achievement.achievementName}");
            }
            
            Debug.Log($"{achievement.achievementName}: {(isUnlocked ? "✓ Unlocked" : "✗ Locked")}");
        }
    }

    void OnEnable()
    {
        // Update UI whenever this object becomes active
        UpdateAchievementUI();
    }
}