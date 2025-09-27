using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class BackGroundInf : MonoBehaviour
{
    [Header("Auto Scroll (units/second)")]
    public Vector2 scrollSpeed = new Vector2(0f, 0.2f); // ví dụ cuộn dọc

    [Header("Camera Parallax")]
    public bool followCamera = false;          // bật nếu muốn parallax theo camera
    public Transform cam;                      // kéo Main Camera vào đây
    [Range(0f, 1f)] public float parallax = 0.2f;

    private Renderer rend;
    private Vector2 uvOffset;                  // tích lũy offset
    private Vector3 lastCamPos;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (followCamera && cam != null)
            lastCamPos = cam.position;
    }

    void LateUpdate()
    {
        // Auto scroll theo thời gian
        uvOffset += scrollSpeed * Time.deltaTime;

        // Parallax theo camera (tuỳ chọn)
        if (followCamera && cam != null)
        {
            Vector3 delta = cam.position - lastCamPos;
            uvOffset += new Vector2(delta.x, delta.y) * parallax;
            lastCamPos = cam.position;

            // Giữ quad "đi theo" camera để luôn che kín
            transform.position = new Vector3(cam.position.x, cam.position.y, transform.position.z);
        }

        // Gán offset (vòng về nhờ Wrap = Repeat)
        // Dùng sharedMaterial để tránh instancing material mỗi frame nếu có nhiều renderer dùng chung
        var mat = rend.material;
        mat.mainTextureOffset = new Vector2(
            Mathf.Repeat(uvOffset.x, 1f),
            Mathf.Repeat(uvOffset.y, 1f)
        );
    }
}

