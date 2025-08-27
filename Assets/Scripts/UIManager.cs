using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonSFX;

    public void RestartGame()
    {
        Time.timeScale = 1f; // ✅ Unpause the game
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f; // ✅ Unpause the game
        SceneManager.LoadScene(mainMenuSceneName); // Load from inspector value
    }

    void Start()
    {
        // Include inactive buttons too
        Button[] allButtons = Resources.FindObjectsOfTypeAll<Button>();
        foreach (Button btn in allButtons)
        {
            // Avoid duplicate listeners
            btn.onClick.RemoveListener(() => PlaySoundForTag(btn));
            btn.onClick.AddListener(() => PlaySoundForTag(btn));
        }
    }

    void PlaySoundForTag(Button btn)
    {
        if (audioSource == null) return;

        string tag = btn.gameObject.tag;

        if (tag == "button" && buttonSFX != null)
        {
            audioSource.PlayOneShot(buttonSFX);
        }
    }
}
