using Unity.Mathematics;
using UnityEngine;

public class PlayerBullet : MonoBehaviour
{   
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float timeDestroy = 2f;
    [SerializeField] private float damage = 10f;
    [SerializeField] GameObject bloodPrefabs;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject,timeDestroy);
    }

    // Update is called once per frame
    void Update()
    {
        MoveBullet();
    }

    void MoveBullet()
    {
        transform.Translate(Vector2.right*moveSpeed*Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if(enemy != null)
            {
                enemy.TakeDamage(damage);
                GameObject blood = Instantiate(bloodPrefabs,transform.position,Quaternion.identity);
                Destroy(blood,1f);
            }
            Destroy(gameObject);
        }
    }
}
