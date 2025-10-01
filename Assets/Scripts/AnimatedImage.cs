using UnityEngine;
using UnityEngine.UI;
public class AnimatedImage : MonoBehaviour
{
    public Sprite[] frames;
    public float frameRate = 0.1f;
    private Image image;
    private int currentFrame;
    private float timer;

    void Start()
    {
        image = GetComponent<Image>();
    }

    void Update()
    {
        if (frames.Length == 0) return;
        timer += Time.deltaTime;
        if (timer >= frameRate)
        {
            currentFrame = (currentFrame + 1) % frames.Length;
            image.sprite = frames[currentFrame];
            timer = 0f;
        }
    }
}
