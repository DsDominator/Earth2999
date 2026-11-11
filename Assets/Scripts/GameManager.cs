using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    private float currentEnergy;
    [SerializeField] private float energyThreshold = 3;
    [SerializeField] private GameObject boss;
    [SerializeField] private GameObject enemySpawner;
    [SerializeField] private Image energeyBar;
    [SerializeField] GameObject gameUi;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject winMenu;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private CinemachineCamera cam;
    [SerializeField] private UpgradeMenu upgradeMenu;
    private bool isChoosingUpgrade = false;
    [Header("Boss Settings")]
    [SerializeField] private float bossSpawnTime = 120f; // 120 giây
    private Coroutine bossCoroutine;

    private bool bossCalled = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentEnergy = 0;
        UpdateEnergyBar();
        boss.SetActive(false);
        MainMenu();
        audioManager.StopGameAudio();
        cam.Lens.OrthographicSize = 5f;
        upgradeMenu.OnUpgradeSelected += OnPlayerUpgraded;
    }


    public void AddEnergy(float energyValue)
    {
        if (bossCalled || isChoosingUpgrade) return;

        currentEnergy += energyValue;

        UpdateEnergyBar();

        if (currentEnergy >= energyThreshold)
        {
            OpenUpgradeMenu();
        }
    }

    private void CallBoss()
    {
        if (bossCoroutine != null)
        {
            StopCoroutine(bossCoroutine);
            bossCoroutine = null;
        }
        DestroyAllEnemies();
        bossCalled = true;
        boss.SetActive(true);
        enemySpawner.SetActive(false);
        gameUi.SetActive(false);
        audioManager.PlayBossAudio();
        cam.Lens.OrthographicSize = 10f;
    }

    private void UpdateEnergyBar()
    { 
        if (energeyBar != null)
        {
            float fillAmount = Mathf.Clamp01((float) currentEnergy/ (float) energyThreshold);
            energeyBar.fillAmount = fillAmount;
        }
    }

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
        mainMenu.SetActive(false);
        gameOverMenu.SetActive(true);
        pauseMenu.SetActive(false);
        winMenu.SetActive(false);
        Time.timeScale = 0f;
    }

    public void PauseGameMenu()
    {
        mainMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        pauseMenu.SetActive(true);
        winMenu.SetActive(false);
        Time.timeScale = 0f;
    }

    public void WinGameMenu()
    {
        mainMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        pauseMenu.SetActive(false);
        winMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void StartGame()
    {
        mainMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        audioManager.PlayDefaultAudio();
        winMenu.SetActive(false);
        if (!bossCalled && bossCoroutine == null)
        {
            bossCoroutine = StartCoroutine(BossSpawnTimer());
        }
    }

    public void ResumeGame()
    {
        mainMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        winMenu.SetActive(false);
    }
    private IEnumerator BossSpawnTimer()
    {
        yield return new WaitForSeconds(bossSpawnTime);
        bossCoroutine = null;
        CallBoss();  
    }
    private void OpenUpgradeMenu()
    {
        EnemySpawner spawner = enemySpawner.GetComponent<EnemySpawner>();
         if (spawner != null)
        {
            spawner.reduceTimeSpawms();
        }
        isChoosingUpgrade = true;
        energyThreshold = energyThreshold*1.2f;

        currentEnergy = 0;
        UpdateEnergyBar();

        upgradeMenu.ShowUpgradeMenu();
    }
    private void OnPlayerUpgraded()
    {
        isChoosingUpgrade = false;
    }
    private void OnDestroy()
    {
        if (upgradeMenu != null)
        {
            upgradeMenu.OnUpgradeSelected -= OnPlayerUpgraded;
        }
    }
    private void DestroyAllEnemies()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(
            FindObjectsSortMode.None
        );

        foreach (Enemy enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }
    }
}
