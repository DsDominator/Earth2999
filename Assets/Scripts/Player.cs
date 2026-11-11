
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    [Header("Player Stats")]
    [SerializeField] private PlayerStats playerStats;

    [Header("UI")]
    [SerializeField] private Image hpBar;
    [SerializeField] private Image dashCooldownImage;

    [Header("References")]
    [SerializeField] private GameManager gameManager;

    public GameObject dashEffectObject;

    private float dashTimer;
    private float dashCooldownTimer;
    private bool isDashing = false;
    private bool isDead = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (playerStats == null)
        {
            playerStats = GetComponent<PlayerStats>();
        }
    }

    private void Start()
    {
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats chưa được gắn vào Player!");
            return;
        }

        playerStats.currentHealth = playerStats.maxHealth;

        UpdateHpBar();
        UpdateDashUI();
    }

    private void Update()
    {
        if (isDead) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameManager.PauseGameMenu();
        }

        HandleDash();
        UpdateDashUI();
    }

    private void FixedUpdate()
    {
        if (isDead || playerStats == null) return;

        MovePlayer();
    }

    private void MovePlayer()
    {
        Vector2 playerInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        float currentMoveSpeed = playerStats.moveSpeed;

        if (isDashing)
        {
            currentMoveSpeed += playerStats.dashBoost;
        }

        rb.linearVelocity = playerInput.normalized * currentMoveSpeed;

        if (playerInput.x < 0)
            spriteRenderer.flipX = true;
        else if (playerInput.x > 0)
            spriteRenderer.flipX = false;

        animator.SetBool("isRun", playerInput != Vector2.zero);
    }

    // =========================
    // HEALTH SYSTEM
    // =========================

    public void TakeDamage(float damage)
    {
        if (isDead || playerStats == null) return;

        playerStats.currentHealth -= damage;

        playerStats.currentHealth = Mathf.Max(
            playerStats.currentHealth, 0
        );

        UpdateHpBar();

        if (playerStats.currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float healValue)
    {
        if (isDead || playerStats == null) return;

        playerStats.currentHealth = Mathf.Min(
            playerStats.currentHealth + healValue,
            playerStats.maxHealth
        );

        UpdateHpBar();
    }

    private void UpdateHpBar()
    {
        if (hpBar != null && playerStats != null)
        {
            hpBar.fillAmount =
                playerStats.currentHealth / playerStats.maxHealth;
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;

        rb.linearVelocity = Vector2.zero;

        gameManager.GameOverMenu();

        Destroy(gameObject);
    }

    // =========================
    // DASH SYSTEM
    // =========================

    private void HandleDash()
    {
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Space) &&
            !isDashing &&
            dashCooldownTimer <= 0)
        {
            isDashing = true;

            dashTimer = playerStats.dashTime;
            dashCooldownTimer = playerStats.dashCooldown;
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0)
            {
                isDashing = false;
            }
        }

        if (dashEffectObject != null)
        {
            dashEffectObject.SetActive(isDashing);
        }
    }

    private void UpdateDashUI()
    {
        if (dashCooldownImage == null) return;

        dashCooldownImage.fillAmount =
            1f - Mathf.Clamp01(
                dashCooldownTimer / playerStats.dashCooldown
            );
    }
}
