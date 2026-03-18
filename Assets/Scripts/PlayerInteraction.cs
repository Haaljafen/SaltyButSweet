using UnityEngine;

// Attach to the Player GameObject.
// No holding mechanic — plates stay on the counter.
//
// E  → collect nearby money  OR  serve nearest counter plate to the customer
// R  → trash (destroy) nearest counter plate
public class PlayerInteraction : MonoBehaviour
{
    [Header("Serving")]
    public CustomerManager customerManager;
    public float serveRange = 5f;

    [Header("Counter")]
    public CounterDisplayZone counterZone;
    public float platePickRange = 3f; // how close player must be to a plate to interact

    [Header("Money Detection")]
    public float detectRadius = 1.5f;

    [Header("Tip Jar")]
    [Tooltip("Drag the TipJar object from the scene here")]
    public TipJar tipJar;
    public float tipJarRange = 2f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) HandleE();
        if (Input.GetKeyDown(KeyCode.R)) HandleR();
    }

    // ── E: collect money → collect tips → serve nearest plate ────────────────
    void HandleE()
    {
        // 1. Collect nearby money pickups
        foreach (var money in FindObjectsByType<MoneyPickup>(FindObjectsSortMode.None))
        {
            if (Vector3.Distance(transform.position, money.transform.position) <= detectRadius)
            {
                money.Collect();
                return;
            }
        }

        // 2. Collect tips from jar if close enough
        if (tipJar != null &&
            Vector3.Distance(transform.position, tipJar.transform.position) <= tipJarRange)
        {
            if (tipJar.TryCollect()) return;
        }

        // 2. Serve the plate matching the customer's order (within range)
        Customer customer = GetCounterCustomer();
        if (customer == null) return;

        GameObject match = GetMatchingPlate(customer.OrderedFood);
        if (match == null) return;

        bool served = customer.TryServe(customer.OrderedFood);
        if (served) Destroy(match);
    }

    // ── R: trash nearest plate ────────────────────────────────────────────────
    void HandleR()
    {
        GameObject nearest = GetNearestPlate();
        if (nearest != null)
        {
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.trashClip, 0.2f);
            Destroy(nearest);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    // Returns the closest plate matching the given food type (within platePickRange)
    GameObject GetMatchingPlate(FoodType food)
    {
        if (counterZone == null) return null;

        GameObject best = null;
        float bestDist  = platePickRange;

        foreach (var plate in counterZone.Plates)
        {
            if (plate == null) continue;
            PlateController pc = plate.GetComponent<PlateController>();
            if (pc == null || pc.foodType != food) continue;
            float d = Vector3.Distance(transform.position, plate.transform.position);
            if (d < bestDist) { bestDist = d; best = plate; }
        }
        return best;
    }

    // Returns nearest plate of any type (for trashing)
    GameObject GetNearestPlate()
    {
        if (counterZone == null) return null;

        GameObject best = null;
        float bestDist  = platePickRange;

        foreach (var plate in counterZone.Plates)
        {
            if (plate == null) continue;
            float d = Vector3.Distance(transform.position, plate.transform.position);
            if (d < bestDist) { bestDist = d; best = plate; }
        }
        return best;
    }

Customer GetCounterCustomer()
    {
        if (customerManager == null) return null;
        Customer c = customerManager.CounterCustomer;
        if (c == null) return null;
        return Vector3.Distance(transform.position, c.transform.position) <= serveRange ? c : null;
    }
}
