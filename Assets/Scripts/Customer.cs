using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

// ─────────────────────────────────────────────────────────────────────────────
// Customer behaviour.
//
// Per-character tuning (set differently on each prefab — Bear / Bunny / Cat / Dog):
//   • maxPatience      — how long they wait before leaving angry
//   • moneyReward      — base payment when served correctly
//   • tipAmount        — extra tip dropped in the jar when served
//   • servedMessages   — happy reactions shown on the FeedbackPanel
//   • angryMessages    — frustrated reactions shown when patience runs out
//
// Animator (optional) — set bool parameters on your Animator Controller:
//   "IsWalking"   — true while moving
//   "IsIdle"      — true while standing at counter
//   "IsImpatient" — true when patience drops below impatientThreshold
// ─────────────────────────────────────────────────────────────────────────────
public class Customer : MonoBehaviour
{
    private static readonly int AnimWalking   = Animator.StringToHash("IsWalking");
    private static readonly int AnimIdle      = Animator.StringToHash("IsIdle");
    private static readonly int AnimImpatient = Animator.StringToHash("IsImpatient");

    [Header("Patience")]
    public float maxPatience = 15f;
    [Range(0f, 1f)]
    public float impatientThreshold = 0.3f;
    public Slider patienceBar; // World-space slider above customer (optional)

    [Header("Reward")]
    [Tooltip("Base money added to GameManager when this customer is served correctly")]
    public int moneyReward = 10;
    [Tooltip("Extra tip coins added to the Tip Jar when served")]
    public int tipAmount = 2;
    public GameObject moneyPrefab; // Optional VFX spawned after correct serve

    [Header("Movement")]
    public float moveSpeed = 1.5f;

    [Header("Idle Behaviour")]
    public float idleLookInterval = 2.5f;
    public float idleLookAngle    = 25f;
    public float idleLookSpeed    = 90f;

    [Header("Order Voice Clips")]
    [Tooltip("Index 0 = Donut, 1 = Cookie, 2 = Croissant, 3 = Bread, 4 = Cake, 5 = Macaron, 6 = Coffee")]
    public AudioClip[] orderVoiceClips; // assigned in Inspector per prefab

    [Header("Speech — Happy (shown on FeedbackPanel when served)")]
    [Tooltip("One is chosen at random. Must match order of servedVoiceClips.")]
    public string[] servedMessages = { "Thank you!", "Delicious!", "Perfect!" };
    [Tooltip("Clips played when served correctly. Index matches servedMessages.")]
    public AudioClip[] servedVoiceClips; // thankyou, delicious, perfect

    [Header("Speech — Angry (shown on FeedbackPanel when patience runs out)")]
    [Tooltip("Must match order of angryVoiceClips.")]
    public string[] angryMessages  = { "I'm never coming back here!", "Unbelievable!" };
    [Tooltip("Clips played when patience runs out. Index matches angryMessages.")]
    public AudioClip[] angryVoiceClips; // I'm never coming back here!, Unbelievable

    [Header("Speech — Impatient (played when patience drops low)")]
    public AudioClip[] impatientVoiceClips; // hello i have been waiting, isAnyoneWorking, takingforever

    // ── Runtime state ─────────────────────────────────────────────────────────
    private FoodType orderedFood;
    private Transform counterPoint;
    private Transform exitPoint;
    private CustomerManager manager;
    private TipJar tipJar; // assigned via Initialize

    private bool isAtCounter = false;
    private bool isServed    = false;
    private bool isImpatient = false;

    private Animator      animator;
    private Quaternion    counterFacingRot;

    public FoodType OrderedFood => orderedFood;
    public bool IsAtCounter     => isAtCounter;
    public bool IsServed        => isServed;

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    void Awake()
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        animator = GetComponentInChildren<Animator>();
    }

    // ── Called by CustomerManager after Instantiate ───────────────────────────
    public void Initialize(Transform counter, Transform exit, CustomerManager mgr, TipJar jar)
    {
        counterPoint = counter;
        exitPoint    = exit;
        manager      = mgr;
        tipJar       = jar;

        int max = System.Enum.GetValues(typeof(FoodType)).Length - 1;
        orderedFood = (FoodType)Random.Range(1, max + 1);

        if (patienceBar != null) patienceBar.gameObject.SetActive(false);

        SetAnimWalking(true);
        StartCoroutine(WalkToCounter());
    }

    // ── Walk from entrance to counter ─────────────────────────────────────────
    IEnumerator WalkToCounter()
    {
        while (Vector3.Distance(transform.position, counterPoint.position) > 0.2f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, counterPoint.position, moveSpeed * Time.deltaTime);

            Vector3 dir = (counterPoint.position - transform.position).normalized;
            if (dir != Vector3.zero) transform.forward = dir;

            yield return null;
        }

        transform.position = counterPoint.position;
        counterFacingRot   = transform.rotation;
        isAtCounter        = true;

        SetAnimWalking(false);
        SetAnimIdle(true);

        manager.OnCustomerAtCounter(this);
        ShowOrderBubble();
        StartCoroutine(PatienceCountdown());
        StartCoroutine(IdleBehaviourLoop());
    }

    // ── Idle look-around ──────────────────────────────────────────────────────
    IEnumerator IdleBehaviourLoop()
    {
        while (isAtCounter && !isServed)
        {
            float wait = idleLookInterval + Random.Range(-0.5f, 0.5f);
            yield return new WaitForSeconds(wait);
            if (!isAtCounter || isServed) break;

            float angle = Random.Range(-idleLookAngle, idleLookAngle);
            if (isImpatient) angle *= 1.5f;

            Quaternion targetRot = counterFacingRot * Quaternion.Euler(0f, angle, 0f);
            float duration       = Mathf.Abs(angle) / idleLookSpeed;
            float t              = 0f;
            Quaternion startRot  = transform.rotation;

            while (t < duration)
            {
                t += Time.deltaTime;
                transform.rotation = Quaternion.Slerp(startRot, targetRot, t / duration);
                yield return null;
            }

            yield return new WaitForSeconds(0.4f);

            t = 0f; startRot = transform.rotation;
            while (t < duration)
            {
                t += Time.deltaTime;
                transform.rotation = Quaternion.Slerp(startRot, counterFacingRot, t / duration);
                yield return null;
            }
        }
    }

    // ── Show world-space patience bar ────────────────────────────────────────
    void ShowOrderBubble()
    {
        if (patienceBar != null)
        {
            patienceBar.gameObject.SetActive(true);
            patienceBar.value = 1f;
        }

        // Play the order voice clip for the food type (Donut=1 → index 0, etc.)
        if (orderVoiceClips != null && AudioManager.Instance != null)
        {
            int idx = (int)orderedFood - 1;
            if (idx >= 0 && idx < orderVoiceClips.Length)
                AudioManager.Instance.PlaySFX(orderVoiceClips[idx]);
        }
    }

    // ── Patience countdown ────────────────────────────────────────────────────
    IEnumerator PatienceCountdown()
    {
        float timer = maxPatience;

        while (timer > 0f && !isServed)
        {
            timer -= Time.deltaTime;
            float ratio = Mathf.Clamp01(timer / maxPatience);

            if (patienceBar != null) patienceBar.value = ratio;
            manager.UpdatePatience(ratio);

            if (!isImpatient && ratio <= impatientThreshold)
            {
                isImpatient = true;
                SetAnimImpatient(true);
                PlayVoiceClip(impatientVoiceClips, Random.Range(0, impatientVoiceClips != null ? impatientVoiceClips.Length : 0));
            }

            yield return null;
        }

        if (!isServed)
        {
            // Show angry feedback BEFORE leaving so it's visible
            int angryIdx = Random.Range(0, angryMessages.Length);
            manager.ShowFeedback(angryMessages[angryIdx]);
            PlayVoiceClip(angryVoiceClips, angryIdx);
            GameManager.Instance.LoseLife();
            Leave();
        }
    }

    // ── Called by PlayerInteraction when player presses E ────────────────────
    public bool TryServe(FoodType food)
    {
        if (!isAtCounter || isServed) return false;

        if (food == orderedFood)
        {
            isServed = true;
            HideUI();

            GameManager.Instance.AddMoney(moneyReward);

            // Drop tip into the jar
            if (tipJar != null) tipJar.AddTip(tipAmount);

            if (moneyPrefab != null)
                Instantiate(moneyPrefab, counterPoint.position + Vector3.up * 0.3f, Quaternion.identity);

            // Show happy reaction on the FeedbackPanel and play voice
            int servedIdx = Random.Range(0, servedMessages.Length);
            manager.ShowFeedback(servedMessages[servedIdx]);
            PlayVoiceClip(servedVoiceClips, servedIdx);

            StartCoroutine(DelayedLeave());
            return true;
        }

        return false; // Wrong food — customer stays
    }

    // ── Leave sequence ────────────────────────────────────────────────────────
    IEnumerator DelayedLeave()
    {
        yield return new WaitForSeconds(1f);
        Leave();
    }

    void Leave()
    {
        HideUI();
        isAtCounter = false;
        SetAnimIdle(false);
        SetAnimImpatient(false);
        SetAnimWalking(true);
        manager.OnCustomerDone(this);
        StartCoroutine(WalkToExit());
    }

    IEnumerator WalkToExit()
    {
        while (Vector3.Distance(transform.position, exitPoint.position) > 0.2f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, exitPoint.position, moveSpeed * Time.deltaTime);

            Vector3 dir = (exitPoint.position - transform.position).normalized;
            if (dir != Vector3.zero) transform.forward = dir;

            yield return null;
        }

        Destroy(gameObject);
    }

    void HideUI()
    {
        if (patienceBar != null) patienceBar.gameObject.SetActive(false);
    }

    // ── Animator helpers ──────────────────────────────────────────────────────
    void SetAnimWalking(bool v)   { if (animator != null) TrySetBool(AnimWalking, v); }
    void SetAnimIdle(bool v)      { if (animator != null) TrySetBool(AnimIdle, v); }
    void SetAnimImpatient(bool v) { if (animator != null) TrySetBool(AnimImpatient, v); }

    void TrySetBool(int hash, bool value)
    {
        foreach (var p in animator.parameters)
            if (p.nameHash == hash && p.type == AnimatorControllerParameterType.Bool)
            {
                animator.SetBool(hash, value);
                return;
            }
    }

    // ── Utility ───────────────────────────────────────────────────────────────
    string RandomFrom(string[] arr)
    {
        if (arr == null || arr.Length == 0) return "";
        return arr[Random.Range(0, arr.Length)];
    }

    void PlayVoiceClip(AudioClip[] clips, int index)
    {
        if (clips == null || clips.Length == 0) return;
        if (index < 0 || index >= clips.Length) return;
        AudioManager.Instance?.PlaySFX(clips[index]);
    }
}
