
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class GunManager : MonoBehaviour
{
    [Header("Weapon Holder")]
    [SerializeField] private Transform weaponHolder;

    [Header("Weapon Prefabs")]
    [SerializeField] private Weapon firstWeaponPrefab;
    [SerializeField] private Weapon secondWeaponPrefab;
    [SerializeField] private Weapon thirdWeaponPrefab;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private AudioManager audioManager;

    [Header("Weapon Upgrade Data")]
    [SerializeField] private WeaponUpgrade[] allUpgrades;

    [Header("Upgrade UI")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private TMP_Text[] upgradeNames;
    [SerializeField] private TMP_Text[] upgradeDescriptions;
    [SerializeField] private Image[] upgradeIcons;

    private WeaponUpgrade[] currentOptions = new WeaponUpgrade[3];

    private Weapon[] weapons = new Weapon[3];
    private int currentSlot = -1;

    public Weapon CurrentWeapon
    {
        get
        {
            if (currentSlot < 0) return null;
            return weapons[currentSlot];
        }
    }
    private void Reset()
{
    allUpgrades = new WeaponUpgrade[]
    {
        // =========================
        // PISTOL
        // =========================

        new WeaponUpgrade
        {
            weapon = WeaponType.Pistol,
            upgrade = UpgradeType.PistolFireRate,
            upgradeName = "Rapid Fire",
            description = "Increase Pistol firing speed by 15%.",
            icon = null
        },

        new WeaponUpgrade
        {
            weapon = WeaponType.Pistol,
            upgrade = UpgradeType.PistolAmmoCapacity,
            upgradeName = "Extended Magazine",
            description = "Increase Pistol maximum ammo capacity by 20%.",
            icon = null
        },

        new WeaponUpgrade
        {
            weapon = WeaponType.Pistol,
            upgrade = UpgradeType.PistolReloadSpeed,
            upgradeName = "Quick Reload",
            description = "Reduce Pistol reload time by 20%.",
            icon = null
        },

        // =========================
        // SHOTGUN
        // =========================

        new WeaponUpgrade
        {
            weapon = WeaponType.Shotgun,
            upgrade = UpgradeType.ShotgunPelletCount,
            upgradeName = "Extra Pellets",
            description = "Fire one additional pellet with each shot.",
            icon = null
        },

        new WeaponUpgrade
        {
            weapon = WeaponType.Shotgun,
            upgrade = UpgradeType.ShotgunSpread,
            upgradeName = "Tight Spread",
            description = "Reduce Shotgun bullet spread angle by 20%.",
            icon = null
        },

        new WeaponUpgrade
        {
            weapon = WeaponType.Shotgun,
            upgrade = UpgradeType.ShotgunAmmoCapacity,
            upgradeName = "Extended Magazine",
            description = "Increase Shotgun maximum ammo capacity by 20%.",
            icon = null
        },

        // =========================
        // SNIPER
        // =========================

        new WeaponUpgrade
        {
            weapon = WeaponType.Sniper,
            upgrade = UpgradeType.SniperPenetration,
            upgradeName = "Armor Piercing",
            description = "Allow Sniper bullets to penetrate one additional enemy.",
            icon = null
        },

        new WeaponUpgrade
        {
            weapon = WeaponType.Sniper,
            upgrade = UpgradeType.SniperReloadSpeed,
            upgradeName = "Quick Reload",
            description = "Reduce Sniper reload time by 20%.",
            icon = null
        },

        new WeaponUpgrade
        {
            weapon = WeaponType.Sniper,
            upgrade = UpgradeType.SniperAmmoCapacity,
            upgradeName = "Extended Magazine",
            description = "Increase Sniper maximum ammo capacity by 20%.",
            icon = null
        }
    };
}

    private void Start()
    {
        EquipWeapon(0, firstWeaponPrefab);
        EquipWeapon(1, secondWeaponPrefab);
        EquipWeapon(2, thirdWeaponPrefab);

        SelectWeapon(0);

        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            SelectWeapon(0);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            SelectWeapon(1);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            SelectWeapon(2);
    }

    // =========================
    // EQUIP WEAPONS
    // =========================

    private void EquipWeapon(int slot, Weapon prefab)
    {
        if (prefab == null) return;

        Weapon weapon = Instantiate(prefab, weaponHolder);

        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;

        weapon.Configure(ammoText, audioManager);

        weapon.gameObject.SetActive(false);

        weapons[slot] = weapon;
    }

    public void SelectWeapon(int slot)
    {
        if (slot < 0 || slot >= weapons.Length) return;
        if (weapons[slot] == null) return;

        if (currentSlot >= 0 && weapons[currentSlot] != null)
        {
            weapons[currentSlot].gameObject.SetActive(false);
        }

        currentSlot = slot;
        weapons[currentSlot].gameObject.SetActive(true);
    }

    // =========================
    // RANDOM UPGRADE
    // =========================

    private void RandomUpgrade()
    {
        List<WeaponUpgrade> available =
            new List<WeaponUpgrade>(allUpgrades);

        if (available.Count < 3)
        {
            Debug.LogError("Need at least 3 weapon upgrades!");
            return;
        }

        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(0, available.Count);

            currentOptions[i] = available[randomIndex];

            available.RemoveAt(randomIndex);
        }
    }

    // =========================
    // SHOW UPGRADE MENU
    // =========================

    public void ShowUpgradeMenu()
    {
        if (allUpgrades == null || allUpgrades.Length < 3)
        {
            Debug.LogError("Need at least 3 upgrade options!");
            return;
        }

        RandomUpgrade();

        for (int i = 0; i < 3; i++)
        {
            WeaponUpgrade option = currentOptions[i];

            upgradeNames[i].text = option.upgradeName;
            upgradeDescriptions[i].text = option.description;

            if (upgradeIcons != null &&
                i < upgradeIcons.Length &&
                upgradeIcons[i] != null)
            {
                upgradeIcons[i].sprite = option.icon;
                upgradeIcons[i].enabled = option.icon != null;
            }
        }

        upgradePanel.SetActive(true);

        Time.timeScale = 0f;
    }

    // =========================
    // SELECT UPGRADE
    // =========================

    public void SelectUpgrade(int index)
    {
        if (index < 0 || index >= currentOptions.Length)
            return;

        WeaponUpgrade selected = currentOptions[index];

        int weaponIndex = (int)selected.weapon;

        if (weaponIndex < 0 || weaponIndex >= weapons.Length)
            return;

        Weapon targetWeapon = weapons[weaponIndex];

        if (targetWeapon == null)
        {
            Debug.LogWarning("Selected weapon does not exist!");
            return;
        }

        targetWeapon.ApplyUpgrade(selected.upgrade);

        Debug.Log(
            selected.weapon + " upgraded: " + selected.upgrade
        );

        upgradePanel.SetActive(false);

        Time.timeScale = 1f;
    }
}
