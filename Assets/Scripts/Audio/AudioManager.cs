using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
[SerializeField] AudioSource musicSource;
[SerializeField] AudioSource sfxSource;


public AudioClip backgroundMusic;
    // public AudioClip death;
    // public AudioClip attackSword;
    // public AudioClip attackBullet;
    // public AudioClip pickupItem;
    // public AudioClip jump;
    // public AudioClip enemysfx;
    // public AudioClip enemyDeath;

    private void Start()
    {
        if (musicSource == null)
        {
            Debug.LogError("MusicSource não está atribuído no AudioManager!");
            return;
        }
        
        if (backgroundMusic == null)
        {
            Debug.LogError("BackgroundMusic clip não está atribuído no AudioManager!");
            return;
        }
        musicSource.clip = backgroundMusic;
        musicSource.Play();
        musicSource.volume = 0.02f;
    }
}
