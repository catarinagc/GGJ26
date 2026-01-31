using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
[SerializeField] AudioSource musicSource;
    public AudioClip backgroundMusic;
[SerializeField] AudioSource sfxSource;

    [Header("Main Character Audio Clips")]
    public AudioClip footstep;
    public AudioClip attack;
    public AudioClip jump;
    public AudioClip death;
    public AudioClip hit;


    [Header("Enemy Audio Clips")]
    public AudioClip enemySpeak;
    public AudioClip enemyAttack;
    public AudioClip enemySteps;
    public AudioClip enemyDeath;
    public AudioClip enemyHit;
    [Header("Collectible Audio Clips")]
    public AudioClip pickupHealth;
    public AudioClip pickupDamage;
    [Header("Boss Audio Clips")]
    public AudioClip bossMusic;
    public AudioClip bossSpeak;
    public AudioClip bossDeath;
    public AudioClip bossHit;
    public AudioClip bossAttack;

    private void Start()
    {
        if (musicSource == null)
        {
            Debug.LogError("MusicSource is not assigned in AudioManager!");
            return;
        }
        
        if (backgroundMusic == null)
        {
            Debug.LogError("BackgroundMusic clip is not assigned in AudioManager!");
            return;
        }
        musicSource.clip = backgroundMusic;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null)
        {
            Debug.LogError("SFXSource is not assigned in AudioManager!");
            return;
        }
        
        if (clip == null)
        {
            Debug.LogError("The provided AudioClip is null!");
            return;
        }
        
        sfxSource.PlayOneShot(clip);
    }
}
