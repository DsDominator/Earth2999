
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Energy Settings")]
    private float currentEnergy;
    [SerializeField] private float energyThreshold = 3f;
    [SerializeField] private Image energeyBar;

    [Header("Game UI")]
    [SerializeField] private GameObject gameUi;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject winMenu;

    [Header("References")]
    [SerializeField] private GameObject enemySpawner;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private CinemachineCamera cam;
    [SerializeField] private UpgradeMenu upgradeMenu;

    [Header("Stage Settings")]
    [SerializeField] private int[] enemiesPerStage;
    [SerializeField] private GameObject[] bosses;

    [Header("Boss Camera")]
    [SerializeField] private float normalCameraSize = 5f;
    [SerializeField] private float bossCameraSize = 10f;

    private int currentStage = 0;
    private int currentBossIndex = 0;

    private bool isStageRunning = false;
    private bool isBossActive = false;
    private bool isChoosingUpgrade = false;
    private bool gameEnded = false;

    private Coroutine stageCoroutine;

    private void Start()
    {
        currentEnergy = 0;

        UpdateEnergyBar();

        // Tắt tất cả Boss khi bắt đầu
        foreach (GameObject boss in bosses)
        {
            if (boss != null)
                boss.SetActive(false);
        }

        if (enemySpawner != null)
            enemySpawner.SetActive(false);

        if (cam != null)
            cam.Lens.OrthographicSize = normalCameraSize;

        MainMenu();

        if (audioManager != null)
            audioManager.StopGameAudio();

        if (upgradeMenu != null)
            upgradeMenu.OnUpgradeSelected += OnPlayerUpgraded;
    }

    // =========================
    // ENERGY SYSTEM
    // =========================

    public void AddEnergy(float energyValue)
    {
        if (gameEnded || isBossActive || isChoosingUpgrade)
            return;

        currentEnergy += energyValue;

        UpdateEnergyBar();

        if (currentEnergy >= energyThreshold)
        {
            OpenUpgradeMenu();
        }
    }

    private void UpdateEnergyBar()
    {
        if (energeyBar == null) return;

        energeyBar.fillAmount =
            Mathf.Clamp01(currentEnergy / energyThreshold);
    }

    private void OpenUpgradeMenu()
    {
        EnemySpawner spawner = enemySpawner.GetComponent<EnemySpawner>();

        if (spawner != null)
        {
            spawner.reduceTimeSpawms();
        }

        isChoosingUpgrade = true;

        energyThreshold *= 1.2f;

        currentEnergy = 0f;

        UpdateEnergyBar();

        upgradeMenu.ShowUpgradeMenu();
    }

    private void OnPlayerUpgraded()
    {
        isChoosingUpgrade = false;
    }

    // =========================
    // GAME MENU
    // =========================

    public void MainMenu()
    {
        mainMenu.SetActive(true);
        gameOverMenu.SetActive(false);
        pauseMenu.SetActive(false);
        winMenu.SetActive(false);

        Time.timeScale = 0f;
    }

    public void GameOverMenu()
    {
        if (gameEnded) return;

        gameEnded = true;

        StopStage();

        mainMenu.SetActive(false);
        gameOverMenu.SetActive(true);
        pauseMenu.SetActive(false);
        winMenu.SetActive(false);

        Time.timeScale = 0f;
    }

    public void PauseGameMenu()
    {
        if (gameEnded || isChoosingUpgrade) return;

        mainMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        pauseMenu.SetActive(true);
        winMenu.SetActive(false);

        Time.timeScale = 0f;
    }

    public void WinGameMenu()
    {
        if (gameEnded) return;

        gameEnded = true;

        StopStage();

        mainMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        pauseMenu.SetActive(false);
        winMenu.SetActive(true);

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (gameEnded) return;

        mainMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        pauseMenu.SetActive(false);
        winMenu.SetActive(false);

        Time.timeScale = 1f;
    }

    // =========================
    // START / RESTART GAME
    // =========================

    public void StartGame()
    {
        StopStage();

        Time.timeScale = 1f;

        mainMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        pauseMenu.SetActive(false);
        winMenu.SetActive(false);

        currentStage = 0;
        currentBossIndex = 0;

        currentEnergy = 0f;
        energyThreshold = 3f;

        isStageRunning = false;
        isBossActive = false;
        isChoosingUpgrade = false;
        gameEnded = false;

        UpdateEnergyBar();

        // Tắt tất cả Boss
        foreach (GameObject boss in bosses)
        {
            if (boss != null)
                boss.SetActive(false);
        }

        // Bật UI
        gameUi.SetActive(true);

        // Camera trở về bình thường
        cam.Lens.OrthographicSize = normalCameraSize;

        audioManager.PlayDefaultAudio();

        // Bắt đầu Stage đầu tiên
        StartEnemyStage();
    }

    // =========================
    // ENEMY STAGE SYSTEM
    // =========================

    private void StartEnemyStage()
    {
        if (isStageRunning || isBossActive || gameEnded)
            return;

        if (currentStage >= enemiesPerStage.Length)
        {
            Debug.LogWarning("No more enemy stages configured!");
            return;
        }

        stageCoroutine = StartCoroutine(EnemyStageCoroutine());
    }

    private IEnumerator EnemyStageCoroutine()
    {
        isStageRunning = true;

        enemySpawner.SetActive(true);

        EnemySpawner spawner =
            enemySpawner.GetComponent<EnemySpawner>();

        if (spawner == null)
        {
            Debug.LogError("EnemySpawner component not found!");
            isStageRunning = false;
            yield break;
        }

        Debug.Log("Starting Stage: " + (currentStage + 1));

        // Spawn quái theo số lượng Stage
        yield return StartCoroutine(
            spawner.SpawnWave(enemiesPerStage[currentStage])
        );

        // Chờ tiêu diệt hết quái nhỏ
        while (CountAliveSmallEnemies() > 0)
        {
            if (gameEnded)
                yield break;

            yield return new WaitForSeconds(0.5f);
        }

        isStageRunning = false;
        stageCoroutine = null;

        // Xuất hiện Boss sau khi hết quái
        SpawnBoss();
    }

    private int CountAliveSmallEnemies()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(
            FindObjectsSortMode.None
        );

        int count = 0;

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null) continue;

            if (enemy is BossEnemy) continue;

            count++;
        }

        return count;
    }

    // =========================
    // BOSS SYSTEM
    // =========================

    private void SpawnBoss()
    {
        if (gameEnded) return;

        if (currentBossIndex >= bosses.Length)
        {
            Debug.LogWarning("No Boss available!");
            return;
        }

        isBossActive = true;

        enemySpawner.SetActive(false);

        gameUi.SetActive(false);

        bosses[currentBossIndex].SetActive(true);

        Debug.Log("Boss " + (currentBossIndex + 1) + " appeared!");

        audioManager.PlayBossAudio();

        cam.Lens.OrthographicSize = bossCameraSize;
    }

    // Gọi hàm này khi Boss chết
    public void BossDefeated()
    {
        if (!isBossActive || gameEnded)
            return;

        isBossActive = false;

        Debug.Log("Boss " + (currentBossIndex + 1) + " defeated!");

        currentBossIndex++;
        currentStage++;

        // Đã tiêu diệt Final Boss
        if (currentBossIndex >= bosses.Length)
        {
            Debug.Log("FINAL BOSS DEFEATED - YOU WIN!");
            WinGameMenu();
            return;
        }

        // Chuẩn bị Stage tiếp theo
        gameUi.SetActive(true);

        cam.Lens.OrthographicSize = normalCameraSize;

        audioManager.PlayDefaultAudio();

        StartEnemyStage();
    }

    // =========================
    // CLEANUP
    // =========================

    private void StopStage()
    {
        if (stageCoroutine != null)
        {
            StopCoroutine(stageCoroutine);
            stageCoroutine = null;
        }

        isStageRunning = false;
    }

    private void OnDestroy()
    {
        if (upgradeMenu != null)
        {
            upgradeMenu.OnUpgradeSelected -= OnPlayerUpgraded;
        }
    }
}
