using UnityEngine;

public class Roi : MonoBehaviour
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
    private Vector3 _initialPosition;

    private void Start()
    {
        _initialPosition = transform.position;
        _currentHealth = _maxHealth;
        _isMovingInCurve = Random.Range(0, 2) == 0;
        _horizontalDirection = _isMovingInCurve ? (Random.Range(0, 2) == 0 ? -1f : 1f) : Random.Range(-0.5f, 0.5f);

        //AdjustPropertiesByScore();
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
            ResetAsteroid();
        }
    }

    private void ResetAsteroid()
    {
        transform.position = _initialPosition;
        _currentHealth = _maxHealth;
        _timeSinceSpawn = 0f;

        _isMovingInCurve = Random.Range(0, 2) == 0;
        _horizontalDirection = _isMovingInCurve ? (Random.Range(0, 2) == 0 ? -1f : 1f) : Random.Range(-0.5f, 0.5f);

        AdjustPropertiesByScore();
    }

    private void AdjustPropertiesByScore()
    {
        if (ScoreManager.Instance == null)
        {
            //Debug.LogWarning("⚠️ ScoreManager.Instance is null! Ensure ScoreManager exists in the scene.");
            return;
        }

        float currentScore = ScoreManager.Instance.getScore();
        float speedIncrease = Mathf.Floor(currentScore / 5000f) * 0.1f;

        _speed += speedIncrease;
        _curveAmount += speedIncrease * 0.5f;
        _curveSpeed += speedIncrease * 0.2f;
    }

    public void TakeDamage(float amount)
    {
        _currentHealth -= amount;
        if (_currentHealth <= 0f)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.explosionSound);
            }
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
            Destroy(explo, 0.1f);
        }

        ResetAsteroid();

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(_asteroidScore);
        }
    }
}
