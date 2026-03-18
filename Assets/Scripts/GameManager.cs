using UnityEngine;
using UnityEngine.UI;   // needed for Image
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public int startingLives = 3;
    public float gameDuration = 180f;

    [Header("HUD References")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI moneyText;
    public GameObject[] heartObjects;   // 3 heart UI images in HUD

    [Header("Star Progress Bar")]
    public Slider starProgressBar;

    [Header("References")]
    public UIPageManager uiManager;
    public CustomerManager customerManager;

    private int lives;
    private int money;
    private float timeRemaining;
    private bool gameActive = false;
    private bool finalMusicPlayed = false;

    public bool GameActive => gameActive;

    // 0 at game start → 1 at game end — used by CustomerManager to scale difficulty
    public float DifficultyScale => 1f - Mathf.Clamp01(timeRemaining / gameDuration);

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Called by the Start Game button — always a fully clean fresh start
    public void StartGame()
    {
        // Stop any running game logic first (guards against Back-to-Menu mid-game)
        gameActive = false;

        // Destroy leftover customers, clear plates, stop all spawning coroutines
        customerManager.ResetForNewGame();
        FindFirstObjectByType<CounterDisplayZone>()?.ClearAll();

        // Reset all game state
        lives            = startingLives;
        money            = 0;
        timeRemaining    = gameDuration;
        gameActive       = true;
        finalMusicPlayed = false;

        if (starProgressBar) starProgressBar.value = 0f;
        UpdateHeartsUI();
        UpdateMoneyUI();
        UpdateTimerUI();

        customerManager.BeginSpawning();
        AudioManager.Instance?.PlayMusic(AudioManager.Instance.gameplayMusic);
    }

    // Call this whenever leaving gameplay (Back to Menu, etc.)
    public void StopGame()
    {
        gameActive = false;
        customerManager.ResetForNewGame();
        FindFirstObjectByType<CounterDisplayZone>()?.ClearAll();
        CleanupGameplayUI();
    }

    void Update()
    {
        if (!gameActive) return;

        // Escape key toggles pause
        if (Input.GetKeyDown(KeyCode.Escape))
            uiManager?.TogglePause();

        timeRemaining -= Time.deltaTime;
        UpdateTimerUI();

        if (!finalMusicPlayed && timeRemaining <= 60f)
        {
            finalMusicPlayed = true;
            AudioManager.Instance?.PlayMusic(AudioManager.Instance.gameplayMusicFinal);
        }


        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            EndGame(won: true);
        }
    }

    public void LoseLife()
    {
        if (!gameActive) return;

        lives = Mathf.Max(0, lives - 1);
        UpdateHeartsUI();
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.lifeLostClip);

        if (lives <= 0)
            EndGame(won: false);
    }

    public void AddMoney(int amount)
    {
        money += amount;
        UpdateMoneyUI();
    }

    void EndGame(bool won)
    {
        gameActive = false;
        CleanupGameplayUI();

        if (won)
        {
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.winClip);
            int stars = money >= 150 ? 3 : money >= 80 ? 2 : 1;
            uiManager.ShowWin(stars);
        }
        else
        {
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.loseClip);
            uiManager.ShowLose();
        }
    }

    void CleanupGameplayUI()
    {
        customerManager?.HideOrderPanel();

        foreach (var station in FindObjectsByType<FoodStation>(FindObjectsSortMode.None))
            station.HidePreparingUI();
    }

    void UpdateHeartsUI()
    {
        for (int i = 0; i < heartObjects.Length; i++)
            heartObjects[i].SetActive(i < lives);
    }

    void UpdateMoneyUI()
    {
        if (moneyText != null)
            moneyText.text = "$" + money;

        // Fill the star progress bar linearly: empty at $0, full at $150 (3-star threshold)
        if (starProgressBar != null)
            starProgressBar.value = Mathf.Clamp01(money / 150f);
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int total = Mathf.CeilToInt(timeRemaining);
            timerText.text = (total / 60) + ":" + (total % 60).ToString("00");
        }
    }

}
