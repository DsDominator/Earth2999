
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class UpgradeMenu : MonoBehaviour
{
    public enum UpgradeType
    {
        Health,
        Speed,
        Damage,
        DashBoost,
        DashTime,
        DashCooldown
    }

    [Serializable]
    public class UpgradeOption
    {
        public string upgradeName;

        [TextArea]
        public string description;

        public UpgradeType type;
    }

    [Header("Upgrade UI")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private TMP_Text[] upgradeNames;
    [SerializeField] private TMP_Text[] upgradeDescriptions;
    [SerializeField] private Image[] upgradeIcons;

    [Header("References")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Upgrade Icons")]
    [SerializeField] private Sprite healthIcon;
    [SerializeField] private Sprite speedIcon;
    [SerializeField] private Sprite damageIcon;
    [SerializeField] private Sprite dashBoostIcon;
    [SerializeField] private Sprite dashTimeIcon;
    [SerializeField] private Sprite dashCooldownIcon;

    private UpgradeType[] currentOptions = new UpgradeType[3];

    public Action OnUpgradeSelected;

    [SerializeField] private UpgradeOption[] allUpgrades =
    {
        new UpgradeOption
        {
            upgradeName = "Health Boost",
            description = "Increase Max HP by 20 and restore 20 HP.",
            type = UpgradeType.Health
        },

        new UpgradeOption
        {
            upgradeName = "Speed Boost",
            description = "Increase movement speed by 15%.",
            type = UpgradeType.Speed
        },

        new UpgradeOption
        {
            upgradeName = "Damage Boost",
            description = "Increase weapon damage by 20%.",
            type = UpgradeType.Damage
        },

        new UpgradeOption
        {
            upgradeName = "Dash Power",
            description = "Increase Dash speed by 2.",
            type = UpgradeType.DashBoost
        },

        new UpgradeOption
        {
            upgradeName = "Dash Duration",
            description = "Increase Dash duration by 0.1 seconds.",
            type = UpgradeType.DashTime
        },

        new UpgradeOption
        {
            upgradeName = "Quick Dash",
            description = "Reduce Dash cooldown by 0.3 seconds.",
            type = UpgradeType.DashCooldown
        }
    };

    private void Start()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }

        if (playerStats == null)
        {
            playerStats = FindAnyObjectByType<PlayerStats>();
        }
    }

    public void ShowUpgradeMenu()
    {
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats not found!");
            return;
        }

        upgradePanel.SetActive(true);

        List<UpgradeOption> available =
            new List<UpgradeOption>(allUpgrades);

        // Chọn ngẫu nhiên 3 nâng cấp khác nhau
        for (int i = 0; i < 3; i++)
        {
            int randomIndex = UnityEngine.Random.Range(
                0, available.Count
            );

            UpgradeOption option = available[randomIndex];

            available.RemoveAt(randomIndex);

            currentOptions[i] = option.type;

            upgradeNames[i].text = option.upgradeName;
            upgradeDescriptions[i].text = option.description;

            if (upgradeIcons != null &&
                i < upgradeIcons.Length &&
                upgradeIcons[i] != null)
            {
                upgradeIcons[i].sprite = GetUpgradeIcon(option.type);
            }
        }

        Time.timeScale = 0f;
    }

    public void SelectUpgrade(int index)
    {
        if (index < 0 || index >= currentOptions.Length)
            return;

        if (playerStats == null)
            return;

        switch (currentOptions[index])
        {
            case UpgradeType.Health:
                playerStats.UpgradeHealth();
                break;

            case UpgradeType.Speed:
                playerStats.UpgradeSpeed();
                break;

            case UpgradeType.Damage:
                playerStats.UpgradeDamage();
                break;

            case UpgradeType.DashBoost:
                playerStats.UpgradeDashBoost();
                break;

            case UpgradeType.DashTime:
                playerStats.UpgradeDashTime();
                break;

            case UpgradeType.DashCooldown:
                playerStats.UpgradeDashCooldown();
                break;
        }

        upgradePanel.SetActive(false);

        Time.timeScale = 1f;

        OnUpgradeSelected?.Invoke();
    }

    private Sprite GetUpgradeIcon(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Health:
                return healthIcon;

            case UpgradeType.Speed:
                return speedIcon;

            case UpgradeType.Damage:
                return damageIcon;

            case UpgradeType.DashBoost:
                return dashBoostIcon;

            case UpgradeType.DashTime:
                return dashTimeIcon;

            case UpgradeType.DashCooldown:
                return dashCooldownIcon;

            default:
                return null;
        }
    }
}
