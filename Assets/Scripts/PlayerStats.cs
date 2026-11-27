
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Damage")]
    public float damageMultiplier = 1f;

    [Header("Critical")]
    [Range(0f, 1f)]
    public float critChance = 0.25f;
    public float critDamageMultiplier = 1.5f;

    [Header("Dash")]
    public float dashBoost = 5f;
    public float dashTime = 0.3f;
    public float dashCooldown = 2f;

    // Upgrade Health
    public void UpgradeHealth()
    {
        maxHealth += 25f;
        currentHealth = Mathf.Min(currentHealth + 25f, maxHealth);
    }

    // Upgrade Speed
    public void UpgradeSpeed()
    {
        moveSpeed *= 1.15f;
    }

    // Upgrade Damage
    public void UpgradeDamage()
    {
        damageMultiplier *= 1.15f;
    }

    // Upgrade Critical Chance
    public void UpgradeCritChance()
    {
        critChance = Mathf.Min(1f, critChance + 0.15f);
    }

    // Upgrade Critical Damage
    public void UpgradeCritDamage()
    {
        critDamageMultiplier *= 1.25f;
    }

    // Upgrade Dash Speed
    public void UpgradeDashBoost()
    {
        dashBoost += 2f;
    }

    // Upgrade Dash Duration
    public void UpgradeDashTime()
    {
        dashTime += 0.1f;
    }

    // Upgrade Dash Cooldown
    public void UpgradeDashCooldown()
    {
        dashCooldown = Mathf.Max(0.5f, dashCooldown - 0.3f);
    }
}
