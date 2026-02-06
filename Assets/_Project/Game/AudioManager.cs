using UnityEngine;

public sealed class AudioManager : MonoBehaviour
{
    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips")]
    public AudioClip bgmAlwaysWithMe;
    public AudioClip sfxBoom;
    public AudioClip sfxLose;
    public AudioClip sfxVictory;

    private void Awake()
    {
        // auto-find nếu quên kéo source
        if (!musicSource || !sfxSource)
        {
            var sources = GetComponents<AudioSource>();
            if (sources.Length >= 2)
            {
                musicSource = sources[0];
                sfxSource = sources[1];
            }
        }
    }

    public void PlayBgm()
    {
        if (!musicSource || !bgmAlwaysWithMe) return;
        musicSource.clip = bgmAlwaysWithMe;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopBgm()
    {
        if (!musicSource) return;
        musicSource.Stop();
    }

    public void PlayBoom()
    {
        if (!sfxSource || !sfxBoom) return;
        sfxSource.PlayOneShot(sfxBoom);
    }

    public void PlayLose()
    {
        if (!sfxSource || !sfxLose) return;
        sfxSource.PlayOneShot(sfxLose);
    }

    public void PlayVictory()
    {
        if (!sfxSource || !sfxVictory) return;
        sfxSource.PlayOneShot(sfxVictory);
    }
}
