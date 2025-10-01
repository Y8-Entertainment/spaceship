using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using UnityEngine.EventSystems;

[Serializable]
public class HighScoreEntry
{
    public string playerName;
    public float score;

    public HighScoreEntry(string name, float score)
    {
        this.playerName = name;
        this.score = score;
    }
}

[Serializable]
public class HighScoreList
{
    public List<HighScoreEntry> highScores = new List<HighScoreEntry>();
}

public class HighScoreManager : MonoBehaviour
{
    private static HighScoreManager instance;
    public static HighScoreManager Instance
    {
        get { return instance; }
    }

    private HighScoreList highScoreList;
    private string savePath;
    private const int MAX_HIGH_SCORES = 5;

    [Header("UI References")]
    public GameObject headerRow;
    public GameObject highScorePanel;
    public InputField nameInputField;
    public Button submitButton;
    public Transform highScoreContent;
    public GameObject highScoreEntryPrefab;

    [Header("New High Score UI References")]
    public GameObject highScoreUIPanel;
    public Transform highScoreUIContent;
    public GameObject highScoreUIEntryPrefab;
    public GameObject highScoreUIHeaderRow;

    private Action onCloseCallback;
    private System.Action onHighScoreSavedCallback;

    private void Awake()
    {
        // Singleton pattern, tránh duplicate khi load lại scene
        if (instance == null)
        {
            instance = this;

        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        savePath = Path.Combine(Application.persistentDataPath, "highscores.json");
        LoadHighScores();
    }

    private void Start()
    {
        if (highScorePanel != null)
        {
            highScorePanel.SetActive(false);
        }
        UpdateHighScoreDisplay();
    }
    public void CloseSavingPanel()
    {
        if (highScorePanel != null)
        {
            highScorePanel.SetActive(false);
        }
        // Show game over UI when closing high score panel
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowGameOverUI();
        }
        onCloseCallback?.Invoke();
        onCloseCallback = null;
    }
    public void CheckAndAddHighScore(float score)
    {
        if (IsHighScore(score))
        {
            ShowHighScoreInput();
        }
    }

    public bool IsHighScore(float score)
    {
        if (highScoreList.highScores.Count < MAX_HIGH_SCORES)
            return true;
        return highScoreList.highScores.Any(entry => score > entry.score);
    }

    public void ShowHighScoreInput(System.Action onSaved = null)
    {
        if (highScorePanel != null)
        {
            highScorePanel.SetActive(true);
            nameInputField.text = "";
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(SubmitHighScore);
            onHighScoreSavedCallback = onSaved;
        }
    }


    public void SubmitHighScore()
    {
        // Disable nút để tránh bấm lại
        submitButton.interactable = false;

        string playerName = nameInputField.text.Trim();

        if (string.IsNullOrEmpty(playerName))
        {
            playerName = "Player";
        }
        else if (playerName.Length > 10)
        {
            playerName = playerName.Substring(0, 10);
        }

        float currentScore = ScoreManager.Instance != null ? ScoreManager.Instance.getScore() : 0f;
        AddHighScore(playerName, currentScore);
        SaveHighScores();
        UpdateHighScoreDisplay();

        // Không tắt panel ở đây, để người chơi có thể xem bảng điểm mới
        // Chỉ reset input và sự kiện submit để tránh lỗi
        nameInputField.text = "";
        submitButton.onClick.RemoveAllListeners();
        submitButton.onClick.AddListener(SubmitHighScore);

        // Đặt focus lại input
        EventSystem.current.SetSelectedGameObject(nameInputField.gameObject);
    }


    private void AddHighScore(string playerName, float score)
    {
        highScoreList.highScores.Add(new HighScoreEntry(playerName, score));
        highScoreList.highScores = highScoreList.highScores
            .OrderByDescending(x => x.score)
            .Take(MAX_HIGH_SCORES)
            .ToList();
    }

    private void SaveHighScores()
    {
        try
        {
            string json = JsonUtility.ToJson(highScoreList);
            File.WriteAllText(savePath, json);
            Debug.Log("High scores saved to: " + savePath);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save high scores: " + e.Message);
        }
    }

    private void LoadHighScores()
    {
        if (File.Exists(savePath))
        {
            try
            {
                string json = File.ReadAllText(savePath);
                highScoreList = JsonUtility.FromJson<HighScoreList>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load high scores: " + e.Message);
                highScoreList = new HighScoreList();
            }
        }
        else
        {
            highScoreList = new HighScoreList();
        }
    }

    public void UpdateHighScoreDisplay()
    {
        if (highScoreContent == null || highScoreEntryPrefab == null)
            return;

        if (headerRow != null)
        {
            headerRow.SetActive(highScoreList.highScores.Count > 0);
        }

        // Xóa các dòng cũ, không xóa headerRow nếu có
        foreach (Transform child in highScoreContent)
        {
            if (child.gameObject != headerRow)
            {
                Destroy(child.gameObject);
            }
        }

        for (int i = 0; i < highScoreList.highScores.Count; i++)
        {
            var entry = highScoreList.highScores[i];
            GameObject entryObj = Instantiate(highScoreEntryPrefab, highScoreContent);
            entryObj.transform.SetSiblingIndex(i + 1); // sau header

            Text[] texts = entryObj.GetComponentsInChildren<Text>();
            if (texts.Length >= 3)
            {
                texts[0].text = (i + 1).ToString() + ".";
                texts[1].text = entry.playerName;
                texts[2].text = entry.score.ToString();
            }
        }
    }

    public void ShowHighScoreUI()
    {
        if (highScoreUIPanel == null || highScoreUIContent == null || highScoreUIEntryPrefab == null)
        {
            Debug.LogWarning("High Score UI references are not set!");
            return;
        }

        highScoreUIPanel.SetActive(true);

        // Hiển thị hoặc ẩn header row dựa vào số lượng điểm cao
        if (highScoreUIHeaderRow != null)
        {
            highScoreUIHeaderRow.SetActive(highScoreList.highScores.Count > 0);
        }

        // Clear existing entries in the new UI
        foreach (Transform child in highScoreUIContent)
        {
            if (child.gameObject != highScoreUIHeaderRow)
            {
                Destroy(child.gameObject);
            }
        }

        for (int i = 0; i < highScoreList.highScores.Count; i++)
        {
            var entry = highScoreList.highScores[i];
            GameObject entryObj = Instantiate(highScoreUIEntryPrefab, highScoreUIContent);
            entryObj.transform.SetSiblingIndex(i + 1); // Đặt vị trí sau header

            Text[] texts = entryObj.GetComponentsInChildren<Text>();
            if (texts.Length >= 3)
            {
                texts[0].text = (i + 1).ToString() + ".";
                texts[1].text = entry.playerName;
                texts[2].text = entry.score.ToString();
            }
        }
    }

    public void CloseHighScoreUI()
    {
        if (highScoreUIPanel != null)
        {
            highScoreUIPanel.SetActive(false);
        }
    }
}
