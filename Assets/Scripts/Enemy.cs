using UnityEngine;  
using UnityEngine.UI;
using System.Collections.Generic;

public abstract class Enemy : MonoBehaviour
{
    [SerializeField] protected float enemyMovespeed = 1f;
    protected Player player;
    [SerializeField] protected float maxHp = 50f;
    protected float currentHp;
    protected bool isDead = false;
    [SerializeField] private Image hpBar;
    [SerializeField] protected float enterDamage = 10f;
    [SerializeField] protected float stayDamage = 1f;
    [Header("Pathfinding")]
    [SerializeField] private float repathInterval = 0.3f;

    private GridPathfinder pathfinder;
    private List<Vector3> currentPath;
    private int pathIndex;
    private float repathTimer;
    private const float waypointDistance = 0.15f;


    protected virtual void Start()
    {
        pathfinder = FindAnyObjectByType<GridPathfinder>();
        player = FindAnyObjectByType<Player>();
        currentHp = maxHp;
        UpdateHpBar();
    }
    protected virtual void Update()
    {
        MoveToPlayer();
    }
    protected void MoveToPlayer()
    {
        if (player == null)
            return;

        // Nếu chưa có PathfindingManager thì đi thẳng như cũ
        if (pathfinder == null)
        {
            MoveDirectlyToPlayer();
            return;
        }

        // Tính lại đường đi theo chu kỳ
        repathTimer -= Time.deltaTime;

        if (repathTimer <= 0f)
        {
            currentPath = pathfinder.FindPath(
                transform.position,
                player.transform.position
            );

            pathIndex = 0;
            repathTimer = repathInterval;
        }

        // Không tìm được đường
        if (currentPath == null || currentPath.Count == 0)
            return;

        // Đi qua các waypoint
        Vector3 target = currentPath[pathIndex];

        transform.position = Vector2.MoveTowards(
            transform.position,
            target,
            enemyMovespeed * Time.deltaTime
        );

        FlipEnemy();

        // Chuyển sang waypoint tiếp theo
        if (Vector2.Distance(transform.position, target)
            <= waypointDistance)
        {
            pathIndex++;

            if (pathIndex >= currentPath.Count)
                currentPath = null;
        }
    }

    // Di chuyển thẳng khi không có PathfindingManager
    private void MoveDirectlyToPlayer()
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            player.transform.position,
            enemyMovespeed * Time.deltaTime
        );

        FlipEnemy();
    }

    protected void FlipEnemy()
    {
        if(player != null)
        {
            transform.localScale = new Vector3(player.transform.position.x<transform.position.x?-1:1,1,1);
        }
    }
    public virtual void TakeDamage(float damage)
    {
        if (isDead)
        return;
        currentHp-=damage;
        currentHp=Mathf.Max(currentHp,0);
        UpdateHpBar();
        if (currentHp <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if (isDead)
        return;
        isDead = true;
        Destroy(gameObject);
    }

    protected void UpdateHpBar()
    {
        if (hpBar != null)
        {
            hpBar.fillAmount = currentHp/maxHp;
        }
    }
}
