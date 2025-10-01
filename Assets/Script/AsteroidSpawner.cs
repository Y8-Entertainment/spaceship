using UnityEngine;
using System.Collections;

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
    [SerializeField] private float _regularAsteroidYOffset = 0f;
    [SerializeField] private float _smallAsteroidYOffset = 1f;

    [Header("Limits")]
    [SerializeField] private int _maxRegularAsteroids = 50;
    [SerializeField] private int _maxSmallAsteroids = 100;
    [SerializeField] private int _minRegularSpawn = 1;
    [SerializeField] private int _maxRegularSpawn = 2;
    [SerializeField] private int _minSmallSpawn = 1;
    [SerializeField] private int _maxSmallSpawn = 3;

    [Header("Score Settings")]
    [SerializeField] private float _scoreThresholdForSmallAsteroids = 500f;
    [SerializeField] private float _hardModeScore = 5000f;

    private float _spawnTimer;
    private float _decayTimer;
    private float _spawnInterval;
    private bool _smallAsteroidsActivated = false;
    private static bool _isHardModeActivated = false;
    private int _currentRegularCount = 0;
    private int _currentSmallCount = 0;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.Log($"[AsteroidSpawner] Destroying duplicate instance on GameObject: {gameObject.name}");
            Destroy(gameObject);
            return;
        }

        _instance = this;
        Reset();
        Debug.Log("[AsteroidSpawner] Instance initialized");
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
            Debug.Log("[AsteroidSpawner] Instance destroyed and reference cleared");
        }
    }

    private void Start()
    {
        Reset();
        Debug.Log($"[Init] Spawn interval bắt đầu: {_spawnInterval}");
    }

    private void Reset()
    {
        _spawnTimer = 0f;
        _decayTimer = 0f;
        _spawnInterval = _initialSpawnInterval;
        _smallAsteroidsActivated = false;
        _isHardModeActivated = false;
        _currentRegularCount = 0;
        _currentSmallCount = 0;
        Debug.Log("Reset");
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        _spawnTimer += dt;
        _decayTimer += dt;

        float currentScore = ScoreManager.Instance.getScore();

        if (!_isHardModeActivated && currentScore >= _hardModeScore)
        {
            Debug.Log($"🔥 [Update] Hard Mode ACTIVATED at score {currentScore:F2}!");
            _isHardModeActivated = true;
        }

        if (_spawnTimer >= _spawnInterval)
        {
            TrySpawn(currentScore);
            _spawnTimer = 0f;
        }

        if (_decayTimer >= 10f && _spawnInterval > _minSpawnInterval)
        {
            float speedUp = _isHardModeActivated ? 0.2f : 0.1f;
            _spawnInterval = Mathf.Max(_minSpawnInterval, _spawnInterval - speedUp);
            _decayTimer = 0f;
            Debug.Log($"[Update] Spawn interval giảm còn {_spawnInterval:F1} giây");
        }
    }

    private void TrySpawn(float currentScore)
    {
        // Don't spawn asteroids if the pulsing effect is active
        if (BackgroundManager.Instance != null && BackgroundManager.Instance.IsPulsingActive)
        {
            return;
        }

        //Debug.Log($"[TrySpawn] Đang kiểm tra spawn thiên thạch... Current Score: {currentScore:F2}");

        // Spawn thiên thạch thường
        int possibleRegular = _maxRegularAsteroids - _currentRegularCount;
        if (possibleRegular > 0)
        {
            int toSpawn = Mathf.Min(Random.Range(_minRegularSpawn, _maxRegularSpawn + 1), possibleRegular);
            if (toSpawn > 0)
            {
                SpawnAsteroids(_regularAsteroidPrefabs, toSpawn, _regularAsteroidYOffset, "Asteroids", false);
            }
        }

        // Spawn thiên thạch nhỏ ngay lập tức nếu đủ điều kiện
        if (currentScore >= _scoreThresholdForSmallAsteroids)
        {
            TrySpawnSmallAsteroids(currentScore);
        }
    }

    private void TrySpawnSmallAsteroids(float currentScore)
    {
        if (currentScore < _scoreThresholdForSmallAsteroids)
        {
            //Debug.Log($"[TrySpawn] Chưa đủ điểm để spawn thiên thạch nhỏ. Cần: {_scoreThresholdForSmallAsteroids:F2}, Hiện tại: {currentScore:F2}");
            return;
        }

        if (!_smallAsteroidsActivated)
        {
            //Debug.Log($"[TrySpawn] 🌟 Thiên thạch nhỏ xuất hiện! Điểm hiện tại: {currentScore}");
            _smallAsteroidsActivated = true;
        }

        int smallPossible = _maxSmallAsteroids - _currentSmallCount;
        if (smallPossible <= 0) return;

        int toSpawnSmall = Mathf.Min(Random.Range(_minSmallSpawn, _maxSmallSpawn + 1), smallPossible);
        if (toSpawnSmall <= 0) return;

        Debug.Log($"[TrySpawn] Spawning {toSpawnSmall} small asteroids");
        SpawnAsteroids(_smallAsteroidPrefabs, toSpawnSmall, _smallAsteroidYOffset, "SmallAsteroids", true);
    }

    private void SpawnAsteroids(GameObject[] prefabs, int count, float yOffset, string tagName, bool isSmall)
    {
        for (int i = 0; i < count; i++)
        {
            // Thêm độ trễ nhỏ giữa các lần spawn
            StartCoroutine(SpawnWithDelay(prefabs, yOffset, tagName, isSmall, i * 0.1f));
        }
    }

    private IEnumerator SpawnWithDelay(GameObject[] prefabs, float yOffset, string tagName, bool isSmall, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogError($"[SpawnWithDelay] No prefabs assigned for {(isSmall ? "small" : "regular")} asteroids!");
            yield break;
        }

        int idx = Random.Range(0, prefabs.Length);
        if (prefabs[idx] == null)
        {
            Debug.LogError($"[SpawnWithDelay] Prefab at index {idx} is null!");
            yield break;
        }

        // Đảm bảo spawn trên cùng một mặt phẳng 2D (z = 0)
        Vector3 pos = new Vector3(
            Random.Range(-_spawnRangeX, _spawnRangeX),
            _spawnPosY + yOffset + Random.Range(-0.5f, 0.5f),
            0f  // Luôn spawn ở z = 0
        );

        GameObject asteroid = Instantiate(prefabs[idx], pos, Quaternion.identity);

        // Đảm bảo các component cần thiết
        if (!asteroid.GetComponent<SpriteRenderer>())
        {
            Debug.LogError($"[SpawnWithDelay] Asteroid prefab missing SpriteRenderer!");
            Destroy(asteroid);
            yield break;
        }

        // Đảm bảo sprite renderer được cấu hình đúng
        SpriteRenderer spriteRenderer = asteroid.GetComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = isSmall ? 2 : 1; // Small asteroids render trên regular asteroids

        asteroid.tag = tagName;
        asteroid.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        // Đảm bảo thiên thạch nằm trong layer 2D
        asteroid.transform.position = new Vector3(
            asteroid.transform.position.x,
            asteroid.transform.position.y,
            0f
        );

        if (isSmall) _currentSmallCount++;
        else _currentRegularCount++;

        Debug.Log($"[Spawn] {(isSmall ? "Small" : "Regular")} asteroid spawned at {pos}. SpriteRenderer enabled: {spriteRenderer.enabled}");
    }

    public void DecreaseAsteroidCount(string tag)
    {
        if (tag == "Asteroids") _currentRegularCount--;
        else if (tag == "SmallAsteroids") _currentSmallCount--;
    }

    public void ResetAsteroidCounts()
    {
        _currentRegularCount = 0;
        _currentSmallCount = 0;
    }
}
