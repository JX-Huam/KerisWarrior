using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class VRMenu : MonoBehaviour
{
    [Header("Menu References")]
    public GameObject Menu;

    [Header("Input Actions")]
    public InputActionProperty ToggleMenu;
    public InputActionProperty Restart;
    public InputActionProperty MainMenu;

    [Header("Scene Settings")]
    public string mainMenuSceneName = "MainMenu"; // Set this in Inspector

    private bool menuOpen = false;

    void OnEnable()
    {
        ToggleMenu.action.Enable();
        Restart.action.Enable();
        MainMenu.action.Enable();
    }

    void OnDisable()
    {
        ToggleMenu.action.Disable();
        Restart.action.Disable();
        MainMenu.action.Disable();
    }

    void Update()
    {
        // Toggle menu
        if (ToggleMenu.action.WasPressedThisFrame())
        {
            menuOpen = !menuOpen;
            Menu.SetActive(menuOpen);
            Time.timeScale = menuOpen ? 0f : 1f;
        }

        // Restart game
        if (menuOpen && Restart.action.WasPressedThisFrame())
        {
            RestartGame();
        }

        // Return to main menu
        if (menuOpen && MainMenu.action.WasPressedThisFrame())
        {
            GoToMainMenu();
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
