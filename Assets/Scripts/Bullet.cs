using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private float _speed = 10.0f;
    private float damage = 1f;
 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    private void Move()
    {
        transform.position += transform.right * _speed * Time.deltaTime;

        // Optional: Destroy if off-screen
        if (Mathf.Abs(transform.position.x) > 10f || Mathf.Abs(transform.position.y) > 6f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D target)
    {
        //// Thêm debug log để kiểm tra va chạm
        //Debug.Log($"Bullet hit: {target.tag}");

        if (target.CompareTag("Asteroids") || target.CompareTag("SmallAsteroids"))
        {
            if (target.TryGetComponent(out Asteroids big))
            {
                big.TakeDamage(damage);
                //Debug.Log("Hit big asteroid");
            }
            else if (target.TryGetComponent(out SmallAsteroids small))
            {
                small.TakeDamage(damage);
                //Debug.Log("Hit small asteroid");
            }
            Destroy(gameObject);
        }
    }



    ///upDamage
    public void setDamage(float newDamage)
    {
        damage = newDamage;
    }

    public void setSize(float sizeUp)
    {
        transform.localScale *= sizeUp;
    }

    public void setSpeed(float newSpeed)
    {
        _speed = newSpeed;
    }
}
