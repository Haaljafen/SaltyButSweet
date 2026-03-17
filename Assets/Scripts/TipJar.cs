using UnityEngine;
using TMPro;

// Attach this to the Tip Jar world-space object in the scene.
// Customers add tips here when served. Player presses E (via PlayerInteraction) to collect.
public class TipJar : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Optional TextMeshPro above the jar showing current tip amount")]
    public TextMeshProUGUI tipLabel;

    [Tooltip("Show the label only when there are tips waiting")]
    public bool hideWhenEmpty = true;

    private int pendingTips = 0;

    void Start()
    {
        RefreshLabel();
    }

    // Called by Customer when it is served successfully
    public void AddTip(int amount)
    {
        if (amount <= 0) return;
        pendingTips += amount;
        RefreshLabel();
    }

    // Called by PlayerInteraction when player presses E near the jar
    public bool TryCollect()
    {
        if (pendingTips <= 0) return false;

        GameManager.Instance.AddMoney(pendingTips);
        pendingTips = 0;
        RefreshLabel();
        return true;
    }

    public int PendingTips => pendingTips;

    void RefreshLabel()
    {
        if (tipLabel == null) return;

        if (hideWhenEmpty)
            tipLabel.gameObject.SetActive(pendingTips > 0);

        tipLabel.text = pendingTips > 0 ? $"+${pendingTips}" : "";
    }
}
