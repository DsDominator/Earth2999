using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource effectAudioSource;
    [SerializeField] AudioSource defaultAudioSource;
    [SerializeField] AudioSource bossAudioSource;
    [SerializeField] AudioClip shootClip;
    [SerializeField] AudioClip reloadClip;
    [SerializeField] AudioClip energyClip;

    public void PlayShootSound()
    {
        effectAudioSource.PlayOneShot(shootClip);
    }
    public void PlayReloadSound()
    {
        effectAudioSource.PlayOneShot(reloadClip);
    }
    public void PlayEnergySound()
    {
        effectAudioSource.PlayOneShot(energyClip);
    }

    public void PlayDefaultAudio()
    {
        bossAudioSource.Stop();
        defaultAudioSource.Play();
    }

    public void PlayBossAudio()
    {
        defaultAudioSource.Stop();
        bossAudioSource.Play();
    }
    public void StopGameAudio()
    {
        bossAudioSource.Stop();
        defaultAudioSource.Stop();
        effectAudioSource.Stop();
    }

    
}
