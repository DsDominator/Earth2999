using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    [SerializeField] GameManager gameManager;
    [SerializeField] AudioManager audioManager;
    private bool isOpeningWeaponUpgrade = false;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("EnemyBullet"))
        {
            Player player = GetComponent<Player>();
            player.TakeDamage(10f);
        }
        else if (collision.CompareTag("Usb"))
        {
            if (isOpeningWeaponUpgrade) return;

            GunManager gunManager = GetComponentInChildren<GunManager>();

            if (gunManager == null)
            {
                Debug.LogError("GunManager not found on Player!");
                return;
            }

            isOpeningWeaponUpgrade = true;

            gunManager.ShowUpgradeMenu();

            Destroy(collision.gameObject);
        }
        else if (collision.CompareTag("Energy"))
        {
            gameManager.AddEnergy(1f);
            Destroy(collision.gameObject);
            audioManager.PlayEnergySound();
        }
        else if (collision.CompareTag("SmallEnergy"))
        {
            gameManager.AddEnergy(0.5f);
            Destroy(collision.gameObject);
            audioManager.PlayEnergySound();
        }
    }
}
