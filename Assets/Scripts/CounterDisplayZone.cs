using System.Collections.Generic;
using UnityEngine;

// Attach to CounterDeliveryZone.
// All FoodStations register their spawned plates here so they sit side-by-side.
public class CounterDisplayZone : MonoBehaviour
{
    [Header("Layout")]
    public Vector3 slotAxis    = new Vector3(-1, 0, 0); // world-space direction plates spread
    public float   slotSpacing = 0.35f;
    public int     maxPlates   = 4;

    [Header("UI")]
    public GameObject counterFullUI; // Drag CounterFullUI here — shown when all slots are taken

    [Header("Player Proximity")]
    public Transform player;
    public float showUIRange = 3f;

    // Fixed-size slot array: index = slot number, null = empty
    private GameObject[] slots;

    void Awake()
    {
        slots = new GameObject[maxPlates];
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        bool playerNearby = player != null &&
                            Vector3.Distance(player.position, transform.position) <= showUIRange;
        counterFullUI?.SetActive(IsFull && playerNearby);
    }

    public bool IsFull
    {
        get
        {
            for (int i = 0; i < slots.Length; i++)
                if (slots[i] == null) return false;
            return true;
        }
    }

    // Returns a live list of all plates currently on the counter
    public List<GameObject> Plates
    {
        get
        {
            var list = new List<GameObject>();
            foreach (var s in slots)
                if (s != null) list.Add(s);
            return list;
        }
    }

    // Destroys all plates and resets all slots — called on new game start
    public void ClearAll()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null) Destroy(slots[i]);
            slots[i] = null;
        }
    }

    // Called by FoodStation to register a plate and get its world-space position.
    // Finds the FIRST empty slot — so removing a middle plate opens that slot for the next one.
    public Vector3 ClaimSlot(GameObject plate)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = plate;
return transform.position + slotAxis.normalized * i * slotSpacing;
            }
        }
        // All slots full (caller should have checked IsFull first)
        return transform.position;
    }
}
