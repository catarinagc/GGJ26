using UnityEngine;
using Combat;
public class getStatsPlayer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Health health = GetComponent<Health>();
        if (health != null)
        {
            health._currentHealth = PlayerStats.Health;
            health._maxHealth = PlayerStats.MaxHealth;
        }

        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null)
        {
            combat.damageMult = PlayerStats.DamageMultiplier;
        }
    }

}
