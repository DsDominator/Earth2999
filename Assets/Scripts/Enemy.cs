
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public abstract class Enemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] protected float enemyMovespeed = 1f;

    protected Player player;

    [Header("Health")]
    [SerializeField] protected float maxHp = 50f;
    protected float currentHp;
    protected bool isDead = false;

    [SerializeField] private Image hpBar;

    [Header("Damage")]
    [SerializeField] protected float enterDamage = 10f;
    [SerializeField] protected float stayDamage = 1f;

    [Header("Pathfinding")]
    [SerializeField] private float repathInterval = 0.5f;
    [SerializeField] private float targetMoveThreshold = 1f;
    [SerializeField] private float waypointDistance = 0.2f;

    private GridPathfinder pathfinder;
    private List<Vector3> currentPath;

    private int pathIndex;
    private float repathTimer;

    private Vector3 lastTargetPosition;

    // =====================================
    // START
    // =====================================

    protected virtual void Start()
    {
        pathfinder = FindAnyObjectByType<GridPathfinder>();
        player = FindAnyObjectByType<Player>();

        currentHp = maxHp;
        UpdateHpBar();

        if (player != null)
        {
            lastTargetPosition = player.transform.position;
        }

        // Cho phép tìm đường ngay lần đầu.
        repathTimer = 0f;
    }

    // =====================================
    // UPDATE
    // =====================================

    protected virtual void Update()
    {
        if (isDead)
            return;

        MoveToPlayer();
    }

    // =====================================
    // MOVE TO PLAYER
    // =====================================

    protected void MoveToPlayer()
    {
        if (player == null)
            return;

        // Không có PathfindingManager thì di chuyển trực tiếp.
        if (pathfinder == null)
        {
            MoveDirectlyToPlayer();
            return;
        }

        repathTimer -= Time.deltaTime;

        float targetDistance = Vector2.Distance(
            player.transform.position,
            lastTargetPosition
        );

        bool hasNoPath =
            currentPath == null ||
            currentPath.Count == 0;

        bool targetMoved =
            targetDistance >= targetMoveThreshold;

        // Chỉ tìm đường khi cần thiết.
        if ((hasNoPath || targetMoved) && repathTimer <= 0f)
        {
            List<Vector3> newPath = pathfinder.FindPath(
                transform.position,
                player.transform.position,
                GetPathHalfSize()
            );

            // Chỉ thay đường cũ khi tìm được đường mới hợp lệ.
            if (newPath != null && newPath.Count > 0)
            {
                currentPath = newPath;
                pathIndex = 0;

                lastTargetPosition = player.transform.position;
            }

            repathTimer = repathInterval;
        }

        // Không có đường đi hợp lệ.
        if (currentPath == null || currentPath.Count == 0)
            return;

        // Kiểm tra waypoint.
        if (pathIndex >= currentPath.Count)
        {
            currentPath = null;
            return;
        }

        Vector3 target = currentPath[pathIndex];

        // Di chuyển đến waypoint.
        transform.position = Vector2.MoveTowards(
            transform.position,
            target,
            enemyMovespeed * Time.deltaTime
        );

        FlipEnemy();

        // Đã đến waypoint thì chuyển sang waypoint tiếp theo.
        if (Vector2.Distance(transform.position, target)
            <= waypointDistance)
        {
            pathIndex++;

            if (pathIndex >= currentPath.Count)
            {
                currentPath = null;
            }
        }
    }

    // =====================================
    // DIRECT MOVEMENT
    // =====================================

    private void MoveDirectlyToPlayer()
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            player.transform.position,
            enemyMovespeed * Time.deltaTime
        );

        FlipEnemy();
    }

    // =====================================
    // AGENT SIZE FOR PATHFINDING
    // =====================================

    protected virtual Vector2 GetPathHalfSize()
    {
        return Vector2.zero;
    }

    // =====================================
    // FLIP
    // =====================================

    protected void FlipEnemy()
    {
        if (player == null)
            return;

        transform.localScale = new Vector3(
            player.transform.position.x < transform.position.x ? -1 : 1,
            1,
            1
        );
    }

    // =====================================
    // TAKE DAMAGE
    // =====================================

    public virtual void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHp -= damage;
        currentHp = Mathf.Max(currentHp, 0);

        UpdateHpBar();

        if (currentHp <= 0)
        {
            Die();
        }
    }

    // Allows attacks that know whether they crit to preserve that information
    // for enemies with mechanics that react to critical hits.
    public virtual void TakeDamage(float damage, bool isCritical)
    {
        TakeDamage(damage);
    }

    // =====================================
    // DIE
    // =====================================

    protected virtual void Die()
    {
        if (isDead)
            return;

        isDead = true;
        Destroy(gameObject);
    }

    // =====================================
    // HP BAR
    // =====================================

    protected void UpdateHpBar()
    {
        if (hpBar != null && maxHp > 0)
        {
            hpBar.fillAmount = currentHp / maxHp;
        }
    }
}
