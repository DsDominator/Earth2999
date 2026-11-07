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
    }

    void movePlayer()
    {
        Vector2 playerInput = new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));
        rb.linearVelocity=playerInput.normalized*moveSpeed;
        if (playerInput.x < 0)
        {
            spriteRenderer.flipX=true;
        } else if(playerInput.x > 0){
            spriteRenderer.flipX=false;
        }
        if (playerInput != Vector2.zero)
        {
            animator.SetBool("isRun",true);
        }
        else
        {
            animator.SetBool("isRun",false);
        }
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
}
