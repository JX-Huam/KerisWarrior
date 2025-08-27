using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string sceneToLoad = "GameScene";

    [Header("UI")]
    [SerializeField] private TMP_InputField nameInputText;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonSFX;

    void Start()
    {
        nameInputText.text = PlayerPrefs.GetString("PlayerName", "Player" + Random.Range(1000, 9999));

        // Include inactive buttons too
        Button[] allButtons = Resources.FindObjectsOfTypeAll<Button>();
        foreach (Button btn in allButtons)
        {
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

    public void StartGame()
    {
        PlayerPrefs.SetString("PlayerName", nameInputText.text);
        PlayerPrefs.Save();
        SceneManager.LoadScene(sceneToLoad); // Use inspector-assigned name
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
