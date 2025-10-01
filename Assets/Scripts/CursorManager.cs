using UnityEngine;
using UnityEngine.SceneManagement;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private Vector2 cursorHotspot = new Vector2(0, 0);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetCustomCursor();

            // Đăng ký sự kiện khi scene thay đổi
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Hủy đăng ký sự kiện khi object bị destroy
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Kiểm tra nếu là scene game thì ẩn cursor
        if (scene.name == "Game")
        {
            HideCursor();
        }
        else
        {
            ShowCursor();
        }
    }

    private void SetCustomCursor()
    {
        Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.ForceSoftware);

        // Kiểm tra scene hiện tại
        if (SceneManager.GetActiveScene().name == "Game")
        {
            HideCursor();
        }
        else
        {
            ShowCursor();
        }
    }

    public void ShowCursor()
    {
        Cursor.visible = true;
    }

    public void HideCursor()
    {
        Cursor.visible = false;
    }
}