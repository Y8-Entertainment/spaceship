using UnityEngine;

public class SpawnItems : MonoBehaviour
{
    public static SpawnItems Instance { get; private set; }

    [SerializeField] private GameObject[] spawnItems;
    [SerializeField] private float spawnHeight = 10f;
    [SerializeField] private float initialSpawnRate = 2f;
    [SerializeField] private float minSpawnRate = 2f;
    [SerializeField] private float spawnAreaWidth = 10f;
    [SerializeField] private float decayInterval = 10f;
    [SerializeField] private float decayAmount = 0.1f;
    [SerializeField] private float dropChance = 0.1f; // 10% chance for asteroid drops

    private float timer = 0f;
    private float decayTimer = 0f;
    private float currentSpawnRate;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentSpawnRate = initialSpawnRate;
    }

    void Update()
    {
        float currentScore = ScoreManager.Instance.getScorce();
        timer += Time.deltaTime;
        if (currentScore > 11000f)
            decayTimer += Time.deltaTime;
        // Decrease spawn rate over time
        if (decayTimer >= decayInterval && currentSpawnRate > minSpawnRate)
        {
            currentSpawnRate = Mathf.Max(minSpawnRate, currentSpawnRate - decayAmount);
            decayTimer = 0f;
            Debug.Log($"[SpawnItems] Spawn rate decreased to {currentSpawnRate:F1} seconds");
        }

        if (timer >= currentSpawnRate)
        {
            SpawnRandomItem();
            timer = 0f;
        }
    }

    void SpawnRandomItem()
    {
        if (spawnItems.Length == 0) return;

        int randomIndex = Random.Range(0, spawnItems.Length);
        float randomX = Random.Range(-spawnAreaWidth / 2f, spawnAreaWidth / 2f);

        Vector3 spawnPosition = transform.position + new Vector3(randomX, spawnHeight, 0f);

        Instantiate(spawnItems[randomIndex], spawnPosition, Quaternion.identity);
    }

    public void TryDropItem(Vector3 position)
    {
        if (Random.value <= dropChance && spawnItems.Length > 0)
        {
            int randomIndex = Random.Range(0, spawnItems.Length);
            GameObject item = Instantiate(spawnItems[randomIndex], position, Quaternion.identity);
            
            // Ensure the ItemDrop component is attached
            if (!item.GetComponent<ItemDrop>())
            {
                item.AddComponent<ItemDrop>();
            }
        }
    }
}