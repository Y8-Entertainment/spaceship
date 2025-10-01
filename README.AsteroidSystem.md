# 🪨 Asteroid System

## Overview
The Asteroid System manages dynamic enemy spawning, movement patterns, and progressive difficulty scaling. The system features two asteroid types (large and small) with different behaviors, intelligent spawning based on score progression, and curved movement patterns that create engaging gameplay challenges.

## 🎯 Asteroid Types

### Large Asteroids
- **Health**: 3 HP
- **Damage**: 5 HP to player
- **Score Value**: 50 points
- **Movement**: Curved or linear patterns
- **Size**: Larger collision box
- **Spawn Trigger**: Available from game start

### Small Asteroids
- **Health**: 3 HP
- **Damage**: Reduced damage to player
- **Score Value**: 50 points
- **Movement**: Faster, more erratic patterns
- **Size**: Smaller collision box
- **Spawn Trigger**: Unlocked at 500 points

## 💻 Technical Implementation

### Core Asteroid Spawning
```csharp
// AsteroidSpawner.cs - Central spawning system
public class AsteroidSpawner : MonoBehaviour
{
    private static AsteroidSpawner _instance;
    public static AsteroidSpawner Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AsteroidSpawner>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("AsteroidSpawner");
                    _instance = go.AddComponent<AsteroidSpawner>();
                }
            }
            return _instance;
        }
    }
    
    [Header("Prefabs")]
    [SerializeField] private GameObject[] _regularAsteroidPrefabs;
    [SerializeField] private GameObject[] _smallAsteroidPrefabs;
    
    [Header("Spawn Settings")]
    [SerializeField] private float _initialSpawnInterval = 4f;
    [SerializeField] private float _minSpawnInterval = 0.3f;
    [SerializeField] private float _spawnRangeX = 8f;
    [SerializeField] private float _spawnPosY = 6f;
    
    [Header("Limits")]
    [SerializeField] private int _maxRegularAsteroids = 50;
    [SerializeField] private int _maxSmallAsteroids = 100;
    [SerializeField] private int _minRegularSpawn = 1;
    [SerializeField] private int _maxRegularSpawn = 2;
    [SerializeField] private int _minSmallSpawn = 1;
    [SerializeField] private int _maxSmallSpawn = 3;
}
```

### Spawning Logic
```csharp
private void Update()
{
    float dt = Time.deltaTime;
    _spawnTimer += dt;
    _decayTimer += dt;
    
    float currentScore = ScoreManager.Instance.getScore();
    
    // Hard mode activation at 5000 points
    if (!_isHardModeActivated && currentScore >= _hardModeScore)
    {
        Debug.Log($"🔥 [Update] Hard Mode ACTIVATED at score {currentScore:F2}!");
        _isHardModeActivated = true;
    }
    
    // Spawn asteroids based on timer
    if (_spawnTimer >= _spawnInterval)
    {
        TrySpawn(currentScore);
        _spawnTimer = 0f;
    }
    
    // Decrease spawn interval over time
    if (_decayTimer >= 10f && _spawnInterval > _minSpawnInterval)
    {
        float speedUp = _isHardModeActivated ? 0.2f : 0.1f;
        _spawnInterval = Mathf.Max(_minSpawnInterval, _spawnInterval - speedUp);
        _decayTimer = 0f;
        Debug.Log($"[Update] Spawn interval giảm còn {_spawnInterval:F1} giây");
    }
}
```

### Intelligent Spawning
```csharp
private void TrySpawn(float currentScore)
{
    // Don't spawn during special effects
    if (BackgroundManager.Instance != null && BackgroundManager.Instance.IsPulsingActive)
    {
        return;
    }
    
    // Spawn regular asteroids
    int possibleRegular = _maxRegularAsteroids - _currentRegularCount;
    if (possibleRegular > 0)
    {
        int toSpawn = Mathf.Min(Random.Range(_minRegularSpawn, _maxRegularSpawn + 1), possibleRegular);
        if (toSpawn > 0)
        {
            SpawnAsteroids(_regularAsteroidPrefabs, toSpawn, _regularAsteroidYOffset, "Asteroids", false);
        }
    }
    
    // Spawn small asteroids if score threshold met
    if (currentScore >= _scoreThresholdForSmallAsteroids)
    {
        TrySpawnSmallAsteroids(currentScore);
    }
}
```

### Small Asteroid Spawning
```csharp
private void TrySpawnSmallAsteroids(float currentScore)
{
    if (currentScore < _scoreThresholdForSmallAsteroids)
        return;
    
    if (!_smallAsteroidsActivated)
    {
        Debug.Log($"[TrySpawn] 🌟 Thiên thạch nhỏ xuất hiện! Điểm hiện tại: {currentScore}");
        _smallAsteroidsActivated = true;
    }
    
    int smallPossible = _maxSmallAsteroids - _currentSmallCount;
    if (smallPossible <= 0) return;
    
    int toSpawnSmall = Mathf.Min(Random.Range(_minSmallSpawn, _maxSmallSpawn + 1), smallPossible);
    if (toSpawnSmall <= 0) return;
    
    Debug.Log($"[TrySpawn] Spawning {toSpawnSmall} small asteroids");
    SpawnAsteroids(_smallAsteroidPrefabs, toSpawnSmall, _smallAsteroidYOffset, "SmallAsteroids", true);
}
```

## 🚀 Asteroid Movement System

### Large Asteroid Movement
```csharp
// Asteroids.cs - Large asteroid behavior
public class Asteroids : MonoBehaviour
{
    [SerializeField] private float _speed = 3.0f;
    [SerializeField] private float _curveAmount = 2.0f;
    [SerializeField] private float _curveSpeed = 1.0f;
    [SerializeField] private float _maxHealth = 3f;
    
    private float _asteroidScore = 50f;
    private float _asteroidDamage = 5f;
    private float _currentHealth;
    private float _horizontalDirection;
    private float _timeSinceSpawn;
    private bool _isMovingInCurve;
    
    private void Start()
    {
        _currentHealth = _maxHealth;
        _isMovingInCurve = Random.Range(0, 2) == 0; // 50% chance for curved movement
        _horizontalDirection = _isMovingInCurve ? 
            (Random.Range(0, 2) == 0 ? -1f : 1f) : 
            Random.Range(-0.5f, 0.5f);
    }
}
```

### Movement Patterns
```csharp
private void Update()
{
    AdjustPropertiesByScore();
    _timeSinceSpawn += Time.deltaTime;
    
    Vector3 pos = transform.position;
    pos.y -= _speed * Time.deltaTime;
    
    if (_isMovingInCurve) // Curved movement
    {
        pos.x += _horizontalDirection * _curveAmount * Mathf.Sin(_timeSinceSpawn * _curveSpeed) * Time.deltaTime;
        transform.Rotate(Vector3.forward, _horizontalDirection * 200 * Time.deltaTime);
    }
    else // Linear movement with drift
    {
        pos.x += _horizontalDirection * _speed * 0.2f * Time.deltaTime;
        transform.Rotate(Vector3.forward, 15 * Time.deltaTime);
    }
    
    transform.position = pos;
    
    // Destroy if off-screen
    if (pos.y < -5.3f || Mathf.Abs(pos.x) > 10.0f)
    {
        DestroySelf();
    }
}
```

### Small Asteroid Movement
```csharp
// SmallAsteroids.cs - Small asteroid behavior
public class SmallAsteroids : MonoBehaviour
{
    [SerializeField] private float _speed = 3.0f;
    [SerializeField] private float _curveAmount = 2.0f;
    [SerializeField] private float _curveSpeed = 1.0f;
    [SerializeField] private float _maxHealth = 3f;
    
    private float _asteroidScore = 50f;
    private float _asteroidDamage = 5f;
    private float _currentHealth;
    private float _horizontalDirection;
    private float _timeSinceSpawn;
    private bool _isMovingInCurve;
    
    // Similar movement logic but with different speed scaling
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
    }
}
```

## 📈 Progressive Difficulty System

### Score-based Scaling
```csharp
// Asteroids.cs - Difficulty scaling
private void AdjustPropertiesByScore()
{
    float currentScore = ScoreManager.Instance.getScore();
    float speedIncrease = Mathf.Floor(currentScore / 1600f) * 0.1f;
    
    if (currentScore < 10000f)
    {
        _speed = 2.0f + speedIncrease;
        _curveAmount = 1f + speedIncrease * 0.3f;
        _curveSpeed = 0.35f + speedIncrease * 0.15f;
    }
    else if (_speed < 9.0f)
    {
        _speed += speedIncrease;
        _curveAmount += speedIncrease * 0.5f;
        _curveSpeed += speedIncrease * 0.2f;
    }
    
    Debug.Log("Big: Speed: " + _speed + " Curve: " + _curveAmount + " CurveSpeed: " + _curveSpeed);
}
```

### Hard Mode Activation
```csharp
// AsteroidSpawner.cs - Hard mode system
[SerializeField] private float _hardModeScore = 5000f;
private static bool _isHardModeActivated = false;

private void Update()
{
    float currentScore = ScoreManager.Instance.getScore();
    
    // Activate hard mode at 5000 points
    if (!_isHardModeActivated && currentScore >= _hardModeScore)
    {
        Debug.Log($"🔥 [Update] Hard Mode ACTIVATED at score {currentScore:F2}!");
        _isHardModeActivated = true;
    }
    
    // Faster spawn rate in hard mode
    if (_decayTimer >= 10f && _spawnInterval > _minSpawnInterval)
    {
        float speedUp = _isHardModeActivated ? 0.2f : 0.1f;
        _spawnInterval = Mathf.Max(_minSpawnInterval, _spawnInterval - speedUp);
        _decayTimer = 0f;
    }
}
```

## 💥 Collision and Damage System

### Asteroid Damage Processing
```csharp
// Asteroids.cs - Damage system
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
```

### Player Collision
```csharp
// Player.cs - Asteroid collision handling
void OnTriggerEnter2D(Collider2D target)
{
    if (target.CompareTag("Asteroids") || target.CompareTag("SmallAsteroids"))
    {
        // Shield protection check
        if (isShieldActive)
        {
            Debug.Log("Damage blocked by shield!");
            DisableShield();
            return;
        }
        
        // Process damage based on asteroid type
        if (target.TryGetComponent(out Asteroids big))
        {
            _currentHealth = big.DealDamage(_currentHealth);
        }
        else if (target.TryGetComponent(out SmallAsteroids small))
        {
            _currentHealth = small.DealDamage(_currentHealth);
        }
        
        // Visual and audio feedback
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(TakedamageEffect(flashDuration));
        AudioManager.Instance.PlaySFX(AudioManager.Instance.takeDamgeSound);
        
        // Check for death
        if (_currentHealth <= 0f)
        {
            HandleHealth();
            Die();
            gameManager.GameOver();
        }
    }
}
```

## 🎯 Bullet Collision System

### Bullet-Asteroid Interaction
```csharp
// Bullet.cs - Projectile collision
private void OnTriggerEnter2D(Collider2D target)
{
    if (target.CompareTag("Asteroids") || target.CompareTag("SmallAsteroids"))
    {
        if (target.TryGetComponent(out Asteroids big))
        {
            big.TakeDamage(damage);
        }
        else if (target.TryGetComponent(out SmallAsteroids small))
        {
            small.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}
```

## 🎨 Visual Effects

### Explosion System
```csharp
// Asteroids.cs - Death effects
private void Die()
{
    if (_explosion != null)
    {
        var explo = Instantiate(_explosion, transform.position, Quaternion.identity);
        Destroy(explo, 0.1f);
    }
    
    // Try to drop an item
    if (SpawnItems.Instance != null)
    {
        SpawnItems.Instance.TryDropItem(transform.position);
    }
    
    // Update spawner count
    if (AsteroidSpawner.Instance != null)
    {
        AsteroidSpawner.Instance.DecreaseAsteroidCount(gameObject.tag);
    }
    
    Destroy(gameObject);
    ScoreManager.Instance.AddScore(_asteroidScore);
}
```

### Rotation Effects
```csharp
// Continuous rotation for visual appeal
if (_isMovingInCurve)
{
    transform.Rotate(Vector3.forward, _horizontalDirection * 200 * Time.deltaTime);
}
else
{
    transform.Rotate(Vector3.forward, 15 * Time.deltaTime);
}
```

## 📊 Spawning Statistics

### Spawn Rates
- **Initial Rate**: 4.0 seconds between spawns
- **Minimum Rate**: 0.3 seconds (hard mode)
- **Decay Interval**: 10 seconds
- **Hard Mode Decay**: 0.2 seconds per interval
- **Normal Mode Decay**: 0.1 seconds per interval

### Asteroid Limits
- **Maximum Large Asteroids**: 50
- **Maximum Small Asteroids**: 100
- **Large Spawn Range**: 1-2 per spawn
- **Small Spawn Range**: 1-3 per spawn

### Score Thresholds
- **Small Asteroids**: 500 points
- **Hard Mode**: 5000 points
- **Speed Increase**: Every 1600 points (large), 1200 points (small)

## 🔧 Configuration

### Spawn Settings
```csharp
[Header("Spawn Settings")]
[SerializeField] private float _initialSpawnInterval = 4f;
[SerializeField] private float _minSpawnInterval = 0.3f;
[SerializeField] private float _spawnRangeX = 8f;
[SerializeField] private float _spawnPosY = 6f;
[SerializeField] private float _scoreThresholdForSmallAsteroids = 500f;
[SerializeField] private float _hardModeScore = 5000f;
```

### Movement Settings
```csharp
[Header("Movement Settings")]
[SerializeField] private float _speed = 3.0f;
[SerializeField] private float _curveAmount = 2.0f;
[SerializeField] private float _curveSpeed = 1.0f;
```

### Health Settings
```csharp
[Header("Health Settings")]
[SerializeField] private float _maxHealth = 3f;
[SerializeField] private float _asteroidScore = 50f;
[SerializeField] private float _asteroidDamage = 5f;
```

## 🐛 Troubleshooting

### Common Issues
1. **Asteroids Not Spawning**
   - Check prefab assignments in `_regularAsteroidPrefabs` and `_smallAsteroidPrefabs`
   - Verify `AsteroidSpawner` singleton initialization
   - Test spawn timer values

2. **Movement Issues**
   - Check `_speed`, `_curveAmount`, and `_curveSpeed` values
   - Verify `_isMovingInCurve` logic
   - Test boundary detection

3. **Collision Problems**
   - Ensure colliders are set to "Trigger"
   - Check tag assignments ("Asteroids", "SmallAsteroids")
   - Verify collision detection setup

### Debug Information
```csharp
// Debug logging for asteroid system
Debug.Log($"Spawn Interval: {_spawnInterval}");
Debug.Log($"Regular Count: {_currentRegularCount}/{_maxRegularAsteroids}");
Debug.Log($"Small Count: {_currentSmallCount}/{_maxSmallAsteroids}");
Debug.Log($"Hard Mode: {_isHardModeActivated}");
Debug.Log($"Asteroid Speed: {_speed}");
Debug.Log($"Curve Amount: {_curveAmount}");
```

## 🔮 Future Enhancements

### Planned Features
- **Asteroid Types**: Different asteroid variants with unique behaviors
- **Boss Asteroids**: Large asteroids with special attack patterns
- **Asteroid Fragments**: Breaking large asteroids into smaller pieces
- **Asteroid Formations**: Grouped asteroid patterns

### Technical Improvements
- **Object Pooling**: Optimize asteroid instantiation and destruction
- **LOD System**: Level of detail for distant asteroids
- **Particle Effects**: Enhanced explosion and trail effects
- **Physics Integration**: More realistic asteroid movement

---

*The Asteroid System provides dynamic and challenging enemy encounters with progressive difficulty scaling and engaging movement patterns.*
