using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Sound")]
    public GameObject soundButtonOn;
    public GameObject soundButtonOff;

    [Header("Music")]
    public GameObject musicButtonOn;
    public GameObject musicButtonOff;

    public bool SoundEnabled { get; private set; }
    public bool MusicEnabled { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        SoundEnabled = PlayerPrefs.GetInt("Sound", 1) == 1;
        MusicEnabled = PlayerPrefs.GetInt("Music", 1) == 1;
    }

    void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.sfxSource.mute = !SoundEnabled;
            AudioManager.Instance.SetMusicMute(!MusicEnabled);
        }
        RefreshUI();
    }

    public void ToggleSound()
    {
        SoundEnabled = !SoundEnabled;
        PlayerPrefs.SetInt("Sound", SoundEnabled ? 1 : 0);
        if (AudioManager.Instance != null)
            AudioManager.Instance.sfxSource.mute = !SoundEnabled;
        RefreshUI();
    }

    public void ToggleMusic()
    {
        MusicEnabled = !MusicEnabled;
        PlayerPrefs.SetInt("Music", MusicEnabled ? 1 : 0);
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicMute(!MusicEnabled);
        RefreshUI();
    }

    void RefreshUI()
    {
        SetToggle(soundButtonOn, soundButtonOff, SoundEnabled);
        SetToggle(musicButtonOn, musicButtonOff, MusicEnabled);
    }

    void SetToggle(GameObject on, GameObject off, bool isOn)
    {
        if (on  != null) on.SetActive(isOn);
        if (off != null) off.SetActive(!isOn);
    }

    public static bool CanPlaySound() => Instance == null || Instance.SoundEnabled;
}
