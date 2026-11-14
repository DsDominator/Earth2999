
using UnityEngine;
using TMPro;
using System.Collections;

public abstract class Weapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] protected Transform firePos;
    [SerializeField] protected GameObject bulletPrefabs;

    [SerializeField] protected float shotDelay = 0.15f;
    [SerializeField] protected int maxAmmo = 24;
    [SerializeField] protected bool automatic = false;

    [Header("References")]
    [SerializeField] protected TextMeshProUGUI ammoText;
    [SerializeField] protected AudioManager audioManager;
    [Header("Reload Settings")]
    [SerializeField] protected float reloadTime = 1.5f;

    protected bool isReloading = false;

    protected int currentAmmo;
    protected float nextShot;
    protected float rotateObject = 180f;

    protected virtual void Start()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoText();
    }

    protected virtual void Update()
    {
        if (Time.timeScale == 0f) return;
        RotateGun();
        Shoot();
        Reload();
    }

    // Xử lý bắn súng
    private void Shoot()
    {
        bool isShooting = automatic
            ? Input.GetMouseButton(0)
            : Input.GetMouseButtonDown(0);

        if (isShooting &&
            currentAmmo > 0 &&
            Time.time >= nextShot)
        {
            nextShot = Time.time + shotDelay;

            Fire();

            currentAmmo--;
            UpdateAmmoText();

            if (audioManager != null)
            {
                audioManager.PlayShootSound();
            }
        }
    }

    // Mỗi loại súng sẽ có cách bắn riêng
    protected abstract void Fire();

    // Tạo viên đạn
    protected virtual void SpawnBullet(float angle = 0f)
    {
        if (firePos == null || bulletPrefabs == null)
            return;

        Quaternion bulletRotation = firePos.rotation *
            Quaternion.Euler(0, 0, angle);

        Instantiate(
            bulletPrefabs,
            firePos.position,
            bulletRotation
        );
    }

    // Xoay súng theo chuột
    private void RotateGun()
    {
        if (Camera.main == null)
            return;

        if (Input.mousePosition.x < 0 ||
            Input.mousePosition.x > Screen.width ||
            Input.mousePosition.y < 0 ||
            Input.mousePosition.y > Screen.height)
        {
            return;
        }

        Vector3 displacement = transform.position -
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        float angle = Mathf.Atan2(
            displacement.y,
            displacement.x
        ) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(
            0, 0, angle + rotateObject
        );

        if (angle < -90 || angle > 90)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, -1, 1);
        }
    }

    // Nạp đạn bằng chuột phải
    private void Reload()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (currentAmmo >= maxAmmo) return;

            StartCoroutine(ReloadCoroutine());
        }
    }

    // Cập nhật số đạn trên UI
    protected void UpdateAmmoText()
    {
        if (ammoText == null)
            return;

        ammoText.text = currentAmmo > 0
            ? currentAmmo.ToString()
            : "Empty";
    }
    public void Configure(
        TextMeshProUGUI text,
        AudioManager audio
    )
    {
        ammoText = text;
        audioManager = audio;

        UpdateAmmoText();
    }
    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;

        // Hiển thị trạng thái đang thay đạn
        if (ammoText != null)
        {
            ammoText.text = "Reloading...";
        }

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        UpdateAmmoText();

        isReloading = false;

        if (audioManager != null)
        {
            audioManager.PlayReloadSound();
        }
    }
    public virtual void ApplyUpgrade(UpgradeType upgrade)
    {
        Debug.LogWarning("This weapon does not support this upgrade: " + upgrade);
    }
}
