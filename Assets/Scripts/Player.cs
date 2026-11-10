using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour     
{
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    [SerializeField] protected float maxHp = 100f;
    protected float currentHp;
    [SerializeField] private Image hpBar;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private float dashBoost = 5f;
    [SerializeField] private float dashTime = 0.3f;
    [SerializeField] private float dashCooldown = 2f;
    [Header("Dash UI")]
    [SerializeField] private Image dashCooldownImage;

    private float currentMoveSpeed;
    private float dashTimer;
    private float dashCooldownTimer;
    private bool isDashing = false;
    public GameObject dashEffectObject;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }
    void Start()
    {
        currentHp = maxHp;
        currentMoveSpeed = moveSpeed;
        UpdateHpBar();
    }

    // Update is called once per frame
    void Update()
    {
        movePlayer();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameManager.PauseGameMenu();
        }
        HandleDash();
        UpdateDashUI();
    }

    void movePlayer()
    {
        Vector2 playerInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        rb.linearVelocity = playerInput.normalized * currentMoveSpeed;

        if (playerInput.x < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (playerInput.x > 0)
        {
            spriteRenderer.flipX = false;
        }

        animator.SetBool("isRun", playerInput != Vector2.zero);
    }
    public void TakeDamage(float damage)
    {
        currentHp-=damage;
        currentHp=Mathf.Max(currentHp,0);
        UpdateHpBar();
        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
        gameManager.GameOverMenu();
    }

    protected void UpdateHpBar()
    {
        if (hpBar != null)
        {
            hpBar.fillAmount = currentHp/maxHp;
        }
    }

    public void Heal(float healValue)
    {
        if(currentHp < maxHp)
        {
            currentHp += healValue;
            currentHp = Mathf.Min(currentHp,maxHp);
            UpdateHpBar();
        }
    }
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
            dashTimer = dashTime;

            dashCooldownTimer = dashCooldown;

            currentMoveSpeed = moveSpeed + dashBoost;
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0)
            {
                isDashing = false;
                currentMoveSpeed = moveSpeed;
            }
        }
        dashEffectObject.SetActive(isDashing);
    }
    private void UpdateDashUI()
    {
        if (dashCooldownImage == null) return;

        dashCooldownImage.fillAmount =
            1f - Mathf.Clamp01(dashCooldownTimer / dashCooldown);
    }
}
