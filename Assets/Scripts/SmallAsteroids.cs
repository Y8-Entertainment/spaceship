using UnityEngine;

public class SmallAsteroids : MonoBehaviour
{
    [SerializeField] private float _speed = 3.0f;
    [SerializeField] private GameObject _explosion;
    [SerializeField] private float _maxHealth = 3f;
    [SerializeField] private float _curveAmount = 2.0f;
    [SerializeField] private float _curveSpeed = 1.0f;

    private float _asteroidScore = 50f;
    private float _asteroidDamage = 5f;
    private float _currentHealth;

    private float _horizontalDirection;
    private float _timeSinceSpawn;
    private bool _isMovingInCurve;

    private void Start()
    {
        _currentHealth = _maxHealth;
        _isMovingInCurve = Random.Range(0, 2) == 0;
        _horizontalDirection = _isMovingInCurve ? (Random.Range(0, 2) == 0 ? -1f : 1f) : Random.Range(-0.5f, 0.5f);

    }

    private void Update()
    {
        _timeSinceSpawn += Time.deltaTime;

        Vector3 pos = transform.position;
        pos.y -= _speed * Time.deltaTime;

        if (_isMovingInCurve)
        {
            pos.x += _horizontalDirection * _curveAmount * Mathf.Sin(_timeSinceSpawn * _curveSpeed) * Time.deltaTime;
            transform.Rotate(Vector3.forward, _horizontalDirection * 200 * Time.deltaTime);
        }
        else
        {
            pos.x += _horizontalDirection * _speed * 0.2f * Time.deltaTime;
            transform.Rotate(Vector3.forward, 15 * Time.deltaTime);
        }

        transform.position = pos;

        if (pos.y < -5.3f || Mathf.Abs(pos.x) > 10.0f)
        {
            DestroySelf(); 
        }
        AdjustPropertiesByScore();
    }

    private void AdjustPropertiesByScore()
    {
        float currentScore = ScoreManager.Instance.getScore();
        float speedIncrease = Mathf.Floor(currentScore / 1200f) * 0.5f;

        if (currentScore < 10000f)
        {
            _speed = 3.0f + speedIncrease;
            _curveAmount = 1f + speedIncrease * 0.5f;
            _curveSpeed = 0.5f + speedIncrease * 0.2f;
        }
        else if (_speed < 10f)
        {
            _speed += speedIncrease;
            _curveAmount += speedIncrease * 0.5f;
            _curveSpeed += speedIncrease * 0.2f;
        }
        Debug.Log("Small:  Speed:" + _speed + ", Curve: " + _curveAmount + ", CurveSpeed" + _curveSpeed);
    }

    public void TakeDamage(float amount)
    {
        _currentHealth -= amount;
        if (_currentHealth <= 0f)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.explosionSound);
            Die();
        }
    }

    public float DealDamage(float amount)
    {
        float dealtDamage = amount - _asteroidDamage;
        Die();
        return dealtDamage;
    }

    private void Die()
    {
        if (_explosion != null)
        {
            var explo = Instantiate(_explosion, transform.position, Quaternion.identity);
            Destroy(explo, 0.85f);
        }

        // Try to drop an item
        if (SpawnItems.Instance != null)
        {
            SpawnItems.Instance.TryDropItem(transform.position);
        }

        if (AsteroidSpawner.Instance != null)
        {
            AsteroidSpawner.Instance.DecreaseAsteroidCount("SmallAsteroids");
        }

        Destroy(gameObject);
        ScoreManager.Instance.AddScore(_asteroidScore);
    }

    private void DestroySelf()
    {
        if (AsteroidSpawner.Instance != null)
        {
            AsteroidSpawner.Instance.DecreaseAsteroidCount("SmallAsteroids");
        }

        Destroy(gameObject);
    }
}
