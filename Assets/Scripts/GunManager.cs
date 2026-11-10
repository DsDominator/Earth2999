
using UnityEngine;
using TMPro;

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

    private Weapon[] weapons = new Weapon[3];
    private int currentSlot = -1;

    private void Start()
    {
        EquipWeapon(0, firstWeaponPrefab);
        EquipWeapon(1, secondWeaponPrefab);
        EquipWeapon(2, thirdWeaponPrefab);

        SelectWeapon(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SelectWeapon(0);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            SelectWeapon(1);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            SelectWeapon(2);    
    }

    private void EquipWeapon(int slot, Weapon prefab)
    {
        if (prefab == null)
            return;

        Weapon weapon = Instantiate(prefab, weaponHolder);

        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;

        // Truyền AudioManager và AmmoText vào Prefab.
        weapon.Configure(ammoText, audioManager);

        weapon.gameObject.SetActive(false);

        weapons[slot] = weapon;
    }

    public void SelectWeapon(int slot)
    {
        if (slot < 0 || slot >= weapons.Length)
            return;

        if (weapons[slot] == null)
            return;

        if (currentSlot >= 0 && weapons[currentSlot] != null)
        {
            weapons[currentSlot].gameObject.SetActive(false);
        }

        currentSlot = slot;
        weapons[currentSlot].gameObject.SetActive(true);
    }
}
