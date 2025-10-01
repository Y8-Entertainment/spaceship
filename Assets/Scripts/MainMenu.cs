using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private bool _isAsteroidNoDamage = false;
    void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(AudioManager.Instance.backgroundMusic);
        }
    }
    public void StartGame()
    {
        // Chuyển đến scene game
        //SceneManager.LoadSceneAsync(1);
        AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClickSound);
        SceneTransitionManager.Instance.TransitionToScene("Game");

    }

    public void ExitGame()
    {
        // Thoát game
        AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClickSound);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        Debug.Log("Game exited.");
    }

    public void SettingsGame()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClickSound);
        SceneManager.LoadScene(2);

    }

    public void ToggleAsteroidDamage()
    {
        _isAsteroidNoDamage = !_isAsteroidNoDamage;

        Asteroids[] asteroids = FindObjectsOfType<Asteroids>();
        foreach (Asteroids asteroid in asteroids)
        {
            asteroid.SetAsteroidDamage(_isAsteroidNoDamage ? 0f : 5f);
        }

        Debug.Log("Cheat " + (_isAsteroidNoDamage ? "ON (damage=0)" : "OFF (damage normal)"));
    }
}
