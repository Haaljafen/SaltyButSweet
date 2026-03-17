using UnityEngine;

// Spawned on the counter after a customer is served correctly.
// Player presses E while nearby to collect it (handled by PlayerInteraction).
public class MoneyPickup : MonoBehaviour
{
    public int amount = 10;

    public void Collect()
    {
        GameManager.Instance.AddMoney(amount);
        Destroy(gameObject);
    }
}
