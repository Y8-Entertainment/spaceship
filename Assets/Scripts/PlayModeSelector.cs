using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;
using System;

public class PlayModeSelector : MonoBehaviour
{
    public Toggle toggleKeyboard;
    public Toggle toggleCursor;
    [SerializeField] private UnityEngine.UI.Button saveButton;
    [SerializeField] private UnityEngine.UI.Button closeButton;
    private PlayMode confirmedPlayMode;
    [SerializeField] private GameObject panel;

    public static PlayMode CurrentPlayMode { get; private set; }

    // Add static event
    public static event Action<PlayMode> OnPlayModeChanged;

    public static PlayModeSelector Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public enum PlayMode
    {
        Keyboard = 0,
        Cursor = 1
    }

    // Thêm phương thức static để thay đổi mode từ bên ngoài
    public static void SetCurrentPlayMode(int mode)
    {
        PlayMode newMode = (PlayMode)mode;
        CurrentPlayMode = newMode;
        PlayerPrefs.SetInt("PlayMode", mode);
        PlayerPrefs.Save();

        // Update UI if instance exists
        if (Instance != null)
        {
            Instance.UpdateToggleStates(newMode);
        }

        OnPlayModeChanged?.Invoke(newMode);
        Debug.Log($"Play mode changed to: {CurrentPlayMode}");
    }

    private void Start()
    {
        // Load saved mode
        int savedMode = PlayerPrefs.GetInt("PlayMode", (int)PlayMode.Cursor); // Default to Cursor
        confirmedPlayMode = (PlayMode)savedMode;
        SetMode(confirmedPlayMode);

        // Set initial toggle states
        toggleKeyboard.isOn = confirmedPlayMode == PlayMode.Keyboard;
        toggleCursor.isOn = confirmedPlayMode == PlayMode.Cursor;

        // Toggle behavior
        toggleKeyboard.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                toggleCursor.isOn = false;
            }
            else if (!toggleCursor.isOn) // Prevent unselecting if the other toggle is not selected
            {
                toggleKeyboard.isOn = true;
            }
        });

        toggleCursor.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                toggleKeyboard.isOn = false;
            }
            else if (!toggleKeyboard.isOn) // Prevent unselecting if the other toggle is not selected
            {
                toggleCursor.isOn = true;
            }
        });

        saveButton.onClick.AddListener(SavePlayMode);
        closeButton.onClick.AddListener(ClosePanel);
    }

    private void SetMode(PlayMode mode)
    {
        CurrentPlayMode = mode;
        PlayerPrefs.SetInt("PlayMode", (int)mode);
        PlayerPrefs.Save();
        OnPlayModeChanged?.Invoke(mode); // Invoke the event
    }

    public void ClosePanel()
    {
        // Reset toggles based on last confirmed state
        toggleKeyboard.isOn = confirmedPlayMode == PlayMode.Keyboard;
        toggleCursor.isOn = confirmedPlayMode == PlayMode.Cursor;

        panel.SetActive(false); // Close the panel
    }

    private void SavePlayMode()
    {
        if (toggleKeyboard.isOn)
            SetMode(PlayMode.Keyboard);
        else if (toggleCursor.isOn)
            SetMode(PlayMode.Cursor);
        panel.SetActive(false);
        Debug.Log("Play mode saved: " + CurrentPlayMode);
    }

    public void UpdateToggleStates(PlayMode mode)
    {
        toggleKeyboard.isOn = mode == PlayMode.Keyboard;
        toggleCursor.isOn = mode == PlayMode.Cursor;
        confirmedPlayMode = mode;
    }
}
