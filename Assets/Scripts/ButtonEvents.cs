using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonEvents : MonoBehaviour
{
    public string buttonType;

    void OnMouseDown()
    {
        if (buttonType == "Start")
        {
            StartGame();
        }
        else if (buttonType == "Exit")
        {
            ExitGame();
        }
        else if (buttonType == "Menu")
        {
            ReturnToMenu();
        }
    }

    void StartGame()
    {
        // Ví dụ chuyển cảnh
        SceneManager.LoadScene("Game"); // Đổi tên scene theo tên thật
    }

    void ExitGame()
    {
        Application.Quit();
        Debug.Log("Game exited.");
    }

    void ReturnToMenu()
    {
        SceneManager.LoadScene("GameMenu");
    }
}
