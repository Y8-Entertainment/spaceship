using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public Slider sfxVolumeSlider;
    public Slider musicVolumeSlider;
    public Text sfxVolumeText;
    public Text musicVolumeText;

    [Header("Mouse Settings")]
    public Slider mouseSensitivitySlider;
    public Text mouseSensitivityText;

    [Header("Game Mode Settings")]
    public Toggle toggleKeyboard;
    public Toggle toggleCursor;

    public GameObject settingsPanel;

    private void Start()
    {
        // Khởi tạo audio settings
        if (AudioManager.Instance != null)
        {
            sfxVolumeSlider.value = AudioManager.Instance.GetSFXVolume();
            musicVolumeSlider.value = AudioManager.Instance.GetMusicVolume();
            UpdateVolumeTexts();
        }

        // Khởi tạo mouse sensitivity từ PlayerPrefs
        float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
        mouseSensitivitySlider.value = savedSensitivity;
        UpdateMouseSensitivityText();

        // Thêm listeners cho audio
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);

        // Khởi tạo game mode settings từ PlayerPrefs
        int savedMode = PlayerPrefs.GetInt("PlayMode", 1); // Mặc định là Cursor (1)
        SetupToggles(savedMode);

        // Thêm listeners cho game mode toggles
        toggleKeyboard.onValueChanged.AddListener(OnKeyboardToggleChanged);
        toggleCursor.onValueChanged.AddListener(OnCursorToggleChanged);
    }

    private void OnEnable()
    {
        PlayModeSelector.OnPlayModeChanged += UpdatePlayModeUI;
    }

    private void OnDisable()
    {
        PlayModeSelector.OnPlayModeChanged -= UpdatePlayModeUI;
    }

    private void UpdatePlayModeUI(PlayModeSelector.PlayMode mode)
    {
        toggleKeyboard.isOn = mode == PlayModeSelector.PlayMode.Keyboard;
        toggleCursor.isOn = mode == PlayModeSelector.PlayMode.Cursor;
    }

    private void SetupToggles(int mode)
    {
        toggleKeyboard.isOn = mode == 0;
        toggleCursor.isOn = mode == 1;
        SaveGameMode(mode);
    }

    private void OnKeyboardToggleChanged(bool isOn)
    {
        if (isOn)
        {
            toggleCursor.isOn = false;
            SaveGameMode(0);
        }
        else if (!toggleCursor.isOn) // Prevent unselecting if the other toggle is not selected
        {
            toggleKeyboard.isOn = true;
        }
    }

    private void OnCursorToggleChanged(bool isOn)
    {
        if (isOn)
        {
            toggleKeyboard.isOn = false;
            SaveGameMode(1);
        }
        else if (!toggleKeyboard.isOn) // Prevent unselecting if the other toggle is not selected
        {
            toggleCursor.isOn = true;
        }
    }

    private void SaveGameMode(int mode)
    {
        PlayModeSelector.SetCurrentPlayMode(mode);
    }

    public void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
            UpdateVolumeTexts();
            AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClickSound);
        }
    }

    public void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
            UpdateVolumeTexts();
        }
    }

    public void OnMouseSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        PlayerPrefs.Save();
        UpdateMouseSensitivityText();
    }

    private void UpdateMouseSensitivityText()
    {
        mouseSensitivityText.text = $"{(mouseSensitivitySlider.value * 100):0}%";
    }

    private void UpdateVolumeTexts()
    {
        sfxVolumeText.text = $"{(sfxVolumeSlider.value * 100):0}%";
        musicVolumeText.text = $"{(musicVolumeSlider.value * 100):0}%";
    }

    public void ToggleSettingsPanel()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public static float GetMouseSensitivity()
    {
        return PlayerPrefs.GetFloat("MouseSensitivity", 1f);
    }
}