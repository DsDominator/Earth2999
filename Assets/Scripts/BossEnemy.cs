using UnityEngine;

public class BossEnemy : Enemy
{
    [SerializeField] private GameObject bulletPrefabs;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float speedNormalBullet = 20f;
    [SerializeField] private float speedCircleBullet = 10f;
    [SerializeField] private float hpValue = 100f;
    [SerializeField] private GameObject miniEnemy;
    [SerializeField] private float skillCooldown = 2f;
    [SerializeField] private GameObject usbPrefabs;
    private float nextSkillTime = 0f;
    protected override void Update()
    {
        base.Update();
        if (Time.time>=nextSkillTime)
        {
            UseSkill();
        }
    }
    protected override void Die()
    {   
        Instantiate(usbPrefabs,transform.position,Quaternion.identity);
        base.Die();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if(player != null)
            {
                player.TakeDamage(enterDamage);
            }
            
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (player != null)
            {
               player.TakeDamage(stayDamage); 
            } 
        }
    }

    private void NormalBullet()
    {
        if (player != null)
        {
            Vector3 directionToPlayer = player.transform.position - firePoint.position;
            directionToPlayer.Normalize();
            GameObject bullet = Instantiate(bulletPrefabs,firePoint.position,Quaternion.identity);
            EnemyBullet enemyBullet = bullet.AddComponent<EnemyBullet>();
            enemyBullet.SetMovementDirection(directionToPlayer*speedNormalBullet);
        }
    }

    private void CircleBullet()
    {
        const int bulletCount = 12;
        float angleStep = 360f/bulletCount;
        for(int i = 0; i < bulletCount; i++)
        {   
            float angle = i * angleStep;
            Vector3 bulletDiretion = new Vector3(Mathf.Cos(Mathf.Deg2Rad*angle),Mathf.Sin(Mathf.Deg2Rad*angle),0);
            GameObject bullet = Instantiate(bulletPrefabs,transform.position,Quaternion.identity);
            EnemyBullet enemyBullet = bullet.AddComponent<EnemyBullet>();
            enemyBullet.SetMovementDirection(bulletDiretion*speedCircleBullet);
        }
    }

    private void BossHeal(float hpAmount)
    {
        currentHp = Mathf.Min(maxHp,currentHp+hpAmount);
        UpdateHpBar();
    }

    private void SpawnMiniEnemy()
    {
        Instantiate(miniEnemy,transform.position,Quaternion.identity);
    }
    
    private void Teleport()
    {
        if (player != null)
        {
            float minDistance = 2f;
            float maxDistance = 4f;

            // Chọn hướng ngẫu nhiên
            Vector2 randomDirection = Random.insideUnitCircle.normalized;

            // Chọn khoảng cách ngẫu nhiên
            float distance = Random.Range(minDistance, maxDistance);

            // Tính vị trí dịch chuyển
            Vector2 teleportPosition =
                (Vector2)player.transform.position + randomDirection * distance;

            transform.position = teleportPosition;
        }
    }

    private void RandomSkill()
    {
        int randomSkill = Random.Range(0,5);
        switch (randomSkill)
        {
            case 0:
                NormalBullet();
                break;
            case 1:
                CircleBullet();
                break;
            case 2:
                BossHeal(hpValue);
                break;
            case 3:
                SpawnMiniEnemy();
                break;
            case 4:
                Teleport();
                break;
        }
    }

    private void UseSkill()
    {
        nextSkillTime = Time.time + skillCooldown;
        RandomSkill();
    }
}
