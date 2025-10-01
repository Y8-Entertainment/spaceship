using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public bool isGameOver = false;
    public bool isPaused = false;

    [Header("UI References")]
    public GameObject gameOverPanel;
    public GameObject pausePanel;
    public GameObject settingPanel;
    public Text finalScoreText;
    public GameObject SaveScorePanel;
    public GameObject highScorePanel;

    [SerializeField]
    private GameObject _asteroids;
    [SerializeField]
    private GameObject _player;

    [SerializeField]
    private Transform _spawnAsteroidsPos;
    [SerializeField]
    private float _spwanTime = 2f;

    private void Awake()
    {
        // Singleton chuẩn, KHÔNG dùng DontDestroyOnLoad để tránh duplicate khi restart
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Ẩn UI khi bắt đầu
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingPanel != null) settingPanel.SetActive(false);
        if (SaveScorePanel != null) SaveScorePanel.SetActive(false);
        if (highScorePanel != null) highScorePanel.SetActive(false);

        // Tạo player mới
        Instantiate(_player, new Vector3(0f, -3.8f, 0f), Quaternion.identity);

        // Bắt đầu spawn asteroid theo thời gian
        Invoke("SpawnAsteroids", _spwanTime);

        // Play nhạc nền nếu có AudioManager
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusic(AudioManager.Instance.gameMusic);
    }

    void Update()
    {
        // Kiểm tra nếu game chưa kết thúc
        if (!isGameOver)
        {
            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                if (isPaused)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }

           
        }

    }

    void SpawnAsteroids()
    {
        Instantiate(_asteroids, new Vector3(Random.Range(-3.5f, 3.5f), transform.position.y, 0f), Quaternion.identity);
        Invoke("SpawnAsteroids", _spwanTime);
    }

    private IEnumerator DelayedGameOver()
    {
        isGameOver = true;
        yield return new WaitForSecondsRealtime(1f);
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        float score = ScoreManager.Instance.getScore();

        // Kiểm tra điểm có thuộc top 10 không
        if (HighScoreManager.Instance != null && HighScoreManager.Instance.IsHighScore(score))
        {
            // Nếu thuộc top 10, chỉ hiện UI nhập tên, KHÔNG hiện GameOverUI
            HighScoreManager.Instance.ShowHighScoreInput(OnHighScoreSaved);
        }
        else
        {
            // Nếu không thuộc top 10, hiện luôn GameOverUI
            ShowGameOverUI();
        }
    }
    public void GameOver()
    {
        StartCoroutine(DelayedGameOver());

    }

    private void OnHighScoreSaved()
    {
        ShowGameOverUI();
    }
    public void ShowGameOverUI()
    {
        // Ensure time scale is 0 during game over
        Time.timeScale = 0f;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (finalScoreText != null && ScoreManager.Instance != null)
            {
                finalScoreText.text = ScoreManager.Instance.getScore().ToString();
            }
        }
        if (SaveScorePanel != null)
        {
            SaveScorePanel.SetActive(false);
        }
    }

    public void RestartGame()
    {
        isGameOver = false;
        isPaused = false;
        Time.timeScale = 1f;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (SaveScorePanel != null) SaveScorePanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (highScorePanel != null) highScorePanel.SetActive(false);

        ScoreManager.Instance.ResetScore();
        AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClickSound);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null)
        {
            Cursor.visible = true;
            pausePanel.SetActive(true);
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null)
        {
            Cursor.visible = false;
            pausePanel.SetActive(false);
        }
    }
    public void ReturnToMenu()
    {
        // Reset game state
        Time.timeScale = 1f;
        isGameOver = false;
        isPaused = false;

        // Reset UI if needed
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (SaveScorePanel != null) SaveScorePanel.SetActive(false);
        if (highScorePanel != null) highScorePanel.SetActive(false);

        // Reset score if needed
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }
        Cursor.visible = true;
        // Load menu scene
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClickSound);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ShowSettings()
    {
        // Pause game when showing settings
        Time.timeScale = 0f;
        isPaused = true;

        // Enable cursor for UI interaction
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Hide other panels if they're visible
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (SaveScorePanel != null) SaveScorePanel.SetActive(false);

        // Show settings panel
        if (settingPanel != null)
        {
            settingPanel.SetActive(true);
        }
    }

    public void HideSettings()
    {
        // Hide settings panel
        if (settingPanel != null)
        {
            settingPanel.SetActive(false);
        }

        // Show pause panel back
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        // Keep game paused since we're showing pause menu
        Time.timeScale = 0f;
        isPaused = true;

        // Keep cursor visible for pause menu interaction
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ToggleAsteroidNoDamage()
    {
        // Duyệt tất cả thiên thạch trong scene
        Asteroids[] asteroids = FindObjectsOfType<Asteroids>();

        foreach (Asteroids asteroid in asteroids)
        {
            asteroid.SetAsteroidDamage(0f); // Set damage = 0
        }

        Debug.Log("✅ Cheat Activated: All asteroids damage set to 0");
    }
}
