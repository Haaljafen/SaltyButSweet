using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CustomerManager : MonoBehaviour
{
    [Header("Spawning")]
    public GameObject[] customerPrefabs;
    public Transform spawnPoint;
    public Transform counterPoint;
    public Transform exitPoint;
    public float spawnInterval = 15f;
    [Tooltip("Seconds before next customer spawns after the current one leaves")]
    public float spawnDelayAfterLeave = 0f;
    public int maxCustomers = 3;

    [Header("Difficulty Progression")]
    [Tooltip("Fastest spawn interval reached at end of game")]
    public float spawnIntervalMin = 5f;
    [Tooltip("Patience multiplier applied at end of game (0.5 = half patience)")]
    public float patienceMultiplierAtEnd = 0.5f;

    [Header("Tip Jar")]
    [Tooltip("Drag the TipJar object from the scene here")]
    public TipJar tipJar;

    [Header("Order UI")]
    public GameObject      orderPanel;    // Drag OrderPanel here
    public TextMeshProUGUI orderText;     // Drag TextOrder here
    public Image           orderIcon;     // Drag food icon Image here
    public Sprite[]        foodSprites;   // 0=Donut … 6=Coffee
    public Slider          patientSlider; // Slider inside OrderPanel

    [Header("Feedback UI")]
    [Tooltip("The FeedbackPanel child inside OrderPanel — shows customer reactions")]
    public GameObject      feedbackPanel;
    [Tooltip("TextMeshPro inside FeedbackPanel")]
    public TextMeshProUGUI feedbackText;
    [Tooltip("How long the feedback bubble stays visible (seconds)")]
    public float feedbackDuration = 1.8f;

    private List<Customer> activeCustomers = new List<Customer>();
    private bool counterOccupied = false;
    private Coroutine hideFeedbackRoutine;
    private Image patientSliderFill;

    // Awake fires before Start on every object — panels are guaranteed hidden
    // before UIPageManager or anything else reads them.
    void Awake()
    {
        orderPanel?.SetActive(false);
        feedbackPanel?.SetActive(false);
    }

    // Called by GameManager.StartGame() — destroys any leftover customers/state
    // from a previous game so every new game starts completely clean.
    public void ResetForNewGame()
    {
        StopAllCoroutines();

        foreach (var c in activeCustomers)
            if (c != null) Destroy(c.gameObject);

        activeCustomers.Clear();
        counterOccupied  = false;
        CounterCustomer  = null;
        hideFeedbackRoutine = null;

        HideOrderPanel();
    }

    public Customer CounterCustomer { get; private set; }

    public void BeginSpawning()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        TrySpawnCustomer();

        while (GameManager.Instance.GameActive)
        {
            // Interval shrinks as game progresses
            float interval = Mathf.Lerp(spawnInterval, spawnIntervalMin, GameManager.Instance.DifficultyScale);
            yield return new WaitForSeconds(interval);
            TrySpawnCustomer();
        }
    }

    void TrySpawnCustomer()
    {
        if (counterOccupied) return;
        if (activeCustomers.Count >= maxCustomers) return;

        int index = Random.Range(0, customerPrefabs.Length);
        GameObject obj = Instantiate(customerPrefabs[index], spawnPoint.position, Quaternion.identity);

        Customer customer = obj.GetComponent<Customer>();
        if (customer == null)
        {
            Debug.LogWarning($"[CustomerManager] '{obj.name}' has no Customer component.", obj);
            Destroy(obj);
            return;
        }

        Transform counter = counterPoint != null ? counterPoint : spawnPoint;
        Transform exit    = exitPoint    != null ? exitPoint    : spawnPoint;
        customer.Initialize(counter, exit, this, tipJar);

        // Scale patience down as difficulty increases
        float scale = Mathf.Lerp(1f, patienceMultiplierAtEnd, GameManager.Instance.DifficultyScale);
        customer.maxPatience *= scale;

        activeCustomers.Add(customer);
        counterOccupied = true;
    }

    // ── Called when customer reaches the counter ──────────────────────────────
    public void OnCustomerAtCounter(Customer customer)
    {
        CounterCustomer = customer;

        feedbackPanel?.SetActive(false);
        orderPanel?.SetActive(true);

        if (orderText != null) orderText.text = customer.OrderedFood.ToString();

        if (orderIcon != null && foodSprites != null)
        {
            int idx = (int)customer.OrderedFood - 1;
            orderIcon.sprite = (idx >= 0 && idx < foodSprites.Length) ? foodSprites[idx] : null;
        }

        if (patientSlider != null)
        {
            patientSlider.value = 1f;
            patientSlider.gameObject.SetActive(true);
            if (patientSliderFill == null && patientSlider.fillRect != null)
                patientSliderFill = patientSlider.fillRect.GetComponent<Image>();
            if (patientSliderFill != null) patientSliderFill.color = Color.green;
        }
    }

    // ── Called every frame by Customer to drain the OrderPanel patience bar ───
    public void UpdatePatience(float value01)
    {
        if (patientSlider != null) patientSlider.value = value01;
        if (patientSliderFill != null)
            patientSliderFill.color = Color.Lerp(Color.red, Color.green, value01);
    }

    // ── Shows a reaction bubble (served or angry) for feedbackDuration seconds ─
    public void ShowFeedback(string message)
    {
        if (feedbackPanel == null) return;

        // Hide the order UI, show the reaction bubble
        orderPanel?.SetActive(false);
        feedbackPanel.SetActive(true);
        if (feedbackText != null) feedbackText.text = message;

        // Restart hide timer
        if (hideFeedbackRoutine != null) StopCoroutine(hideFeedbackRoutine);
        hideFeedbackRoutine = StartCoroutine(HideFeedbackAfterDelay());
    }

    IEnumerator HideFeedbackAfterDelay()
    {
        yield return new WaitForSeconds(feedbackDuration);
        feedbackPanel?.SetActive(false);
    }

    // ── Called when a customer finishes (served or timed out) ─────────────────
    public void OnCustomerDone(Customer customer)
    {
        activeCustomers.Remove(customer);
        counterOccupied  = false;
        CounterCustomer  = null;

        orderPanel?.SetActive(false);

        if (GameManager.Instance.GameActive)
            StartCoroutine(SpawnNextCustomerSoon());
    }

    IEnumerator SpawnNextCustomerSoon()
    {
        yield return new WaitForSeconds(spawnDelayAfterLeave);
        TrySpawnCustomer();
    }

    public void HideOrderPanel()
    {
        orderPanel?.SetActive(false);
        feedbackPanel?.SetActive(false);
    }
}
