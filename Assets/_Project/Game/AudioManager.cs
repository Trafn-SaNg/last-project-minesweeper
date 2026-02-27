using UnityEngine;

public sealed class AudioManager : MonoBehaviour
{
    [Header("Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips")]
    public AudioClip bgmClip;
    public AudioClip boomClip;
    public AudioClip loseClip;
    public AudioClip victoryClip;

    void Awake()
    {
        // Nếu anh quên kéo trong Inspector, tự lấy 2 AudioSource trên object
        if (!bgmSource || !sfxSource)
        {
            var sources = GetComponents<AudioSource>();
            if (sources != null && sources.Length >= 2)
            {
                bgmSource ??= sources[0];
                sfxSource ??= sources[1];
            }
            else if (sources != null && sources.Length == 1)
            {
                // Nếu chỉ có 1 source thì dùng tạm cho cả 2 (không khuyên)
                bgmSource ??= sources[0];
                sfxSource ??= sources[0];
            }
        }

        // Setup BGM source
        if (bgmSource)
        {
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            if (bgmClip) bgmSource.clip = bgmClip;
        }

        // Setup SFX source
        if (sfxSource)
        {
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
    }

    public void PlayBgm()
    {
        if (!bgmSource) return;
        if (bgmSource.clip && !bgmSource.isPlaying) bgmSource.Play();
    }

    public void PlayBoom() => PlaySfx(boomClip);
    public void PlayLose() => PlaySfx(loseClip);
    public void PlayVictory() => PlaySfx(victoryClip);

    void PlaySfx(AudioClip clip)
    {
        if (!sfxSource || !clip) return;
        sfxSource.PlayOneShot(clip);
    }

    public void SetBgmVolume(float v)
    {
        if (bgmSource) bgmSource.volume = Mathf.Clamp01(v);
    }

    public void SetSfxVolume(float v)
    {
        if (sfxSource) sfxSource.volume = Mathf.Clamp01(v);
    }
}