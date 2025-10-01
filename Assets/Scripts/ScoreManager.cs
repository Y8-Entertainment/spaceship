using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    [SerializeField] 
    private Text scoreText;
    private float score = 0;
    private float timer = 0f;
    public float increaseInterval = 1f;
    public float pointsPerInterval = 10;

    private GameManager gameManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ResetScore();
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Nếu muốn giữ điểm qua các scene, mở dòng dưới:
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogWarning("GameManager not found in scene!");
        }
    }

    public void AddScore(float amount)
    {
        if(!gameManager.IsGameOver())
        {
            score += amount;
            UpdateScoreText();
        }    
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager == null || gameManager.IsGameOver())
            return;

        timer += Time.deltaTime;
        if (timer >= increaseInterval)
        {
            score += pointsPerInterval;
            timer = 0f;
            UpdateScoreText();
        }
    }
    public float getScore()
    {
        return score;
    }
    void UpdateScoreText()
    {
        scoreText.text = score.ToString();
    }

    public float getScorce()
    {
        return score;
    }

    public void ResetScore()
    {
        score = 0;
        UpdateScoreText();
    }

}
