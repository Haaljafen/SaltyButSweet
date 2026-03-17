using UnityEngine;

// Attach to your Trash object (with a Trigger Collider).
// PlayerInteraction detects it so the player can discard a carried plate by pressing E.
public class TrashZone : MonoBehaviour
{
    [Header("UI")]
    public GameObject interactPrompt; // Drag TrashUI here

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && interactPrompt != null)
            interactPrompt.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && interactPrompt != null)
            interactPrompt.SetActive(false);
    }
}
