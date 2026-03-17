using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FoodStation : MonoBehaviour
{
    [Header("Food Settings")]
    public string foodName = "Food";
    public FoodType foodType = FoodType.Donut;  // Must match the station
    public GameObject foodPrefab;               // Pastry placed on top of the plate
    public GameObject platePrefab;              // Plate spawned at the counter
    public CounterDisplayZone counterZone;      // Drag CounterDeliveryZone here
    public float preparationTime = 2f;

    [Header("UI")]
    public GameObject      interactPrompt;  // "Press E to Order" panel
    public TextMeshProUGUI interactText;    // Text inside interactPrompt
    public GameObject      preparingUI;     // Panel shown while preparing
    public Slider          progressBar;
    public TextMeshProUGUI preparingText;

    private bool playerInRange = false;
    private bool isPreparing   = false;

    void Update()
    {
        bool counterFull = counterZone != null && counterZone.IsFull;

        // Show prompt: "Counter is Full!" or "Press E to Order"
        if (playerInRange && !isPreparing && interactPrompt != null)
        {
            interactPrompt.SetActive(true);
            if (interactText)
                interactText.text = counterFull ? "Counter is Full!" : "Press E to Order";
        }

        if (playerInRange && !isPreparing && !counterFull && Input.GetKeyDown(KeyCode.E))
            StartCoroutine(PrepareFood());
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            bool counterFull = counterZone != null && counterZone.IsFull;
            if (interactPrompt != null) interactPrompt.SetActive(!counterFull);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactPrompt != null) interactPrompt.SetActive(false);
        }
    }

    IEnumerator PrepareFood()
    {
        isPreparing = true;

        if (interactPrompt != null) interactPrompt.SetActive(false);
        if (preparingUI    != null) preparingUI.SetActive(true);
        if (preparingText  != null) preparingText.text = "Preparing " + foodName + "...";
        if (progressBar    != null) progressBar.value = 0f;

        AudioManager.Instance?.PlaySFX(AudioManager.Instance.prepStartClip);

        float elapsed = 0f;
        while (elapsed < preparationTime)
        {
            elapsed += Time.deltaTime;
            if (progressBar != null)
                progressBar.value = elapsed / preparationTime;
            yield return null;
        }

        if (preparingUI != null) preparingUI.SetActive(false);

        AudioManager.Instance?.PlaySFX(AudioManager.Instance.prepDoneClip);
        SpawnPlateAtCounter();

        isPreparing = false;

        if (playerInRange && interactPrompt != null)
            interactPrompt.SetActive(true);
    }

    public void HidePreparingUI()
    {
        if (preparingUI != null) preparingUI.SetActive(false);
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    void SpawnPlateAtCounter()
    {
        if (platePrefab == null || counterZone == null) return;

        // Ask the shared zone for the next free slot position
        GameObject plateObj = Instantiate(platePrefab, Vector3.zero, counterZone.transform.rotation);
        Vector3 spawnPos    = counterZone.ClaimSlot(plateObj);
        plateObj.transform.position = spawnPos;

        // Add / configure PlateController so the player can pick it up
        PlateController pc = plateObj.GetComponent<PlateController>();
        if (pc == null) pc = plateObj.AddComponent<PlateController>();
        pc.SetFood(foodType);

        // Place the pastry model on top of the plate
        if (foodPrefab != null)
            Instantiate(foodPrefab, spawnPos + Vector3.up * 0.08f, Quaternion.identity, plateObj.transform);
    }
}
