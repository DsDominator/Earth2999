
using UnityEngine;
using System.Collections;

public class LaserController : MonoBehaviour
{
    [SerializeField] private GameObject chargeEffect;
    [SerializeField] private GameObject coreEffect;
    [SerializeField] private GameObject beamEffect;

    [SerializeField] private float chargeDuration = 1.5f;
    [SerializeField] private float fireDuration = 1f;

    private bool isPlaying;

    public void PlayLaser()
    {
        if (isPlaying) return;

        StartCoroutine(LaserSequence());
    }

    private IEnumerator LaserSequence()
    {
        isPlaying = true;

        // Giai đoạn tích tụ năng lượng
        chargeEffect.SetActive(true);
        coreEffect.SetActive(true);
        beamEffect.SetActive(false);

        yield return new WaitForSeconds(chargeDuration);

        // Bắn Laser
        chargeEffect.SetActive(false);
        coreEffect.SetActive(false);
        beamEffect.SetActive(true);

        yield return new WaitForSeconds(fireDuration);

        // Kết thúc
        beamEffect.SetActive(false);

        isPlaying = false;
    }
}
