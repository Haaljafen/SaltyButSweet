using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music")]
    public AudioClip lobbyMusic;
    public AudioClip gameplayMusic;
    public AudioClip gameplayMusicFinal;

    [Header("SFX — Gameplay")]
    public AudioClip wrongOrderClip;
    public AudioClip counterFullClip;
    public AudioClip lifeLostClip;
    public AudioClip coinClip;
    public AudioClip trashClip;
    public AudioClip prepStartClip;
    public AudioClip prepDoneClip;
    public AudioClip winClip;
    public AudioClip loseClip;
    public AudioClip buttonClickClip;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        var sources = GetComponents<AudioSource>();
        if (musicSource == null)
            musicSource = sources.Length > 0 ? sources[0] : gameObject.AddComponent<AudioSource>();
        if (sfxSource == null)
            sfxSource = sources.Length > 1 ? sources[1] : gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;  musicSource.playOnAwake = false; musicSource.volume = 0.03f;
        sfxSource.loop   = false; sfxSource.playOnAwake   = false;

    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        if (sfxSource == null) return;
        if (!SettingsManager.CanPlaySound()) return;
        sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (SettingsManager.Instance != null && !SettingsManager.Instance.MusicEnabled) return;
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PauseMusic()
    {
        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        if (SettingsManager.Instance != null && !SettingsManager.Instance.MusicEnabled) return;
        musicSource.UnPause();
    }

    public void SetMusicMute(bool mute)
    {
        musicSource.mute = mute;
    }
}
