using UnityEngine;
using UnityEngine.SceneManagement;

public class UIPageManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject startPanel;
    public GameObject instructionsPanel;
    public GameObject settingsPanel;
    public GameObject hudPanel;
    public GameObject pausePanel;
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Win Screen Stars")]
    public GameObject star1;   // always shown on win
    public GameObject star2;   // shown if 2+ stars
    public GameObject star3;   // shown if 3 stars (no lives lost)

    // Set true before reloading the scene to skip the start menu and go straight to gameplay
    public static bool AutoStartOnLoad = false;

    void Start()
    {
        StartCoroutine(InitAfterFrame());
    }

    // Wait one frame so DontDestroyOnLoad objects (CustomerManager, GameManager)
    // are fully ready before we try to hide their panels.
    System.Collections.IEnumerator InitAfterFrame()
    {
        yield return null;

        if (AutoStartOnLoad)
        {
            AutoStartOnLoad = false;
            StartGame();
        }
        else
        {
            ShowStartMenu();
        }
    }

    public void ShowStartMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;

        // Stop all game logic — prevents timer/spawning running in background on the menu
        GameManager.Instance?.StopGame();

        startPanel.SetActive(true);
        instructionsPanel.SetActive(false);
        settingsPanel.SetActive(false);
        hudPanel.SetActive(false);
        pausePanel.SetActive(false);
        winPanel.SetActive(false);
        losePanel.SetActive(false);

        AudioManager.Instance?.PlayMusic(AudioManager.Instance.lobbyMusic);
    }

    public void StartGame()
    {
        startPanel.SetActive(false);
        instructionsPanel.SetActive(false);
        settingsPanel.SetActive(false);
        pausePanel.SetActive(false);
        winPanel.SetActive(false);
        losePanel.SetActive(false);

        hudPanel.SetActive(true);

        Time.timeScale = 1f;
        GameManager.Instance.StartGame();
    }

    public void ShowInstructions()
    {
        startPanel.SetActive(false);
        instructionsPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    public void ShowSettings()
    {
        startPanel.SetActive(false);
        instructionsPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    // X button on SettingsPanel → close it, go back to Start Menu
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        startPanel.SetActive(true);
    }

    public void BackToMainMenu()
    {
        ShowStartMenu();
    }

    private bool isPaused = false;

    public void TogglePause()
    {
        if (isPaused) ResumeGame(); else PauseGame();
        isPaused = !isPaused;
    }

    public void PauseGame()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    // Called by GameManager with 1, 2, or 3 stars
    public void ShowWin(int stars)
    {
        hudPanel.SetActive(false);
        winPanel.SetActive(true);

        if (star1 != null) star1.SetActive(stars >= 1);
        if (star2 != null) star2.SetActive(stars >= 2);
        if (star3 != null) star3.SetActive(stars >= 3);

        Time.timeScale = 0f;
    }

    public void ShowLose()
    {
        losePanel.SetActive(true);
        hudPanel.SetActive(false);

        Time.timeScale = 0f;
    }

    public void RestartScene()
    {
        // Full scene reload for a clean slate (destroys all customers, plates, coroutines)
        // AutoStartOnLoad skips the start menu and jumps straight into the game
        AutoStartOnLoad = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}