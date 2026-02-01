using UnityEngine;
using UnityEngine.SceneManagement;
using Combat;

public class DetectEndLevel : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats.Health = other.GetComponent<Health>().CurrentHealth;
            PlayerStats.MaxHealth = other.GetComponent<Health>().MaxHealth;
            PlayerStats.DamageMultiplier = other.GetComponent<PlayerCombat>().damageMult;
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}

