using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BackgroundManager : MonoBehaviour
{
    public static BackgroundManager Instance { get; private set; }

    [System.Serializable]
    public class BackgroundStage
    {
        public GameObject quad;
        public float triggerScore;
    }

    [Header("Background Settings")]
    [SerializeField] private BackgroundStage[] stages;
    [SerializeField] private float scrollSpeed = 5.0f;
    [SerializeField] private float quadHeight = 10.03889f;

    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private Color transitionColor = new Color(1f, 1f, 1f, 0.8f);
    [SerializeField] private float maxBrightness = 1.5f;
    [SerializeField] private float maxBlurAmount = 5f;

    private int currentIndex = 0;
    private int nextIndex = 1;
    private bool transitioning = false;
    private float transitionTimer = 0f;
    private bool isTransitioning = false;
    private Image transitionOverlay;
    private Material blurMaterial;
    private Player player;
   
    // for hard mode alert
    private bool isBlinking = false;
    [SerializeField] private float SCORE_THRESHOLD = 10000f;
    [SerializeField] private float pulseDuration = 2f;
    [SerializeField] private float minAlpha = 0.1f;
    [SerializeField] private float maxAlpha = 0.4f;
    private Coroutine currentEffect;
    private bool hasShownEffect = false;
   
   
  
    private readonly Color DARK_RED = new Color(0.5f, 0f, 0f, 0.3f);
   
    private bool hasDestroyedAsteroids = false;
    private bool hasStartedEffect = false;
    
    public bool IsPulsingActive => isBlinking;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (blurMaterial != null)
        {
            Destroy(blurMaterial);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateTransitionOverlay();
        InitializeBackgroundQuads();
        CreateBlurMaterial();
        player = FindObjectOfType<Player>();
    }

    private void CreateBlurMaterial()
    {
        // Create a new material with the blur shader
        Shader blurShader = Shader.Find("UI/Blur");
        if (blurShader == null)
        {
            Debug.LogError("Blur shader not found! Make sure you have the UI/Blur shader in your project.");
            return;
        }
        blurMaterial = new Material(blurShader);
        transitionOverlay.material = blurMaterial;
    }

    private void CreateTransitionOverlay()
    {
        GameObject overlayObj = new GameObject("TransitionOverlay");
        overlayObj.transform.SetParent(transform);

        // Setup Canvas
        Canvas canvas = overlayObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        // Setup Canvas Scaler
        CanvasScaler scaler = overlayObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Setup Overlay Image
        transitionOverlay = overlayObj.AddComponent<Image>();
        transitionOverlay.color = new Color(transitionColor.r, transitionColor.g, transitionColor.b, 0f);

        // Setup RectTransform
        RectTransform rect = transitionOverlay.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void InitializeBackgroundQuads()
    {
        // Set initial positions
        for (int i = 0; i < stages.Length; i++)
        {
            if (i == 0)
            {
                // First quad starts at the bottom
                stages[i].quad.transform.position = Vector3.zero;
            }
            else
            {
                // Other quads start above
                stages[i].quad.transform.position = new Vector3(0, quadHeight, 0);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleTransitionEffect();
        CheckForStageTransition();
        UpdateBackgroundPosition();
        
        float currentScore = ScoreManager.Instance.getScorce();
        if (currentScore >= SCORE_THRESHOLD && !hasShownEffect)
        {
            hasShownEffect = true;
            currentEffect = StartCoroutine(HardModeEffect());
        }
        else if (currentScore < SCORE_THRESHOLD)
        {
            hasShownEffect = false;
            if (currentEffect != null)
            {
                StopCoroutine(currentEffect);
                currentEffect = null;
                ResetEffect();
            }
        }
    }

    private void ResetEffect()
    {
        isBlinking = false;
        transitionOverlay.color = new Color(transitionColor.r, transitionColor.g, transitionColor.b, 0f);
        if (blurMaterial != null)
        {
            blurMaterial.SetFloat("_BlurAmount", 0f);
        }
    }

    private IEnumerator HardModeEffect()
    {
        Debug.Log("[HardModeEffect] Starting effect");
        isBlinking = true;

        // Destroy asteroids
        GameObject[] regularAsteroids = GameObject.FindGameObjectsWithTag("Asteroids");
        GameObject[] smallAsteroids = GameObject.FindGameObjectsWithTag("SmallAsteroids");
        
        foreach (GameObject asteroid in regularAsteroids)
        {
            if (asteroid != null) Destroy(asteroid);
        }
        
        foreach (GameObject asteroid in smallAsteroids)
        {
            if (asteroid != null) Destroy(asteroid);
        }
        
        if (AsteroidSpawner.Instance != null)
        {
            AsteroidSpawner.Instance.ResetAsteroidCounts();
        }

        float elapsedTime = 0f;
        while (elapsedTime < pulseDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / pulseDuration;
            float pulseProgress = (Mathf.Sin(progress * Mathf.PI * 2) + 1f) * 0.5f;
            
            float currentAlpha = Mathf.Lerp(minAlpha, maxAlpha, pulseProgress);
            Color pulseColor = new Color(0.7f, 0f, 0f, currentAlpha);
            
            if (blurMaterial != null)
            {
                float blurAmount = Mathf.Lerp(1f, maxBlurAmount, pulseProgress);
                blurMaterial.SetFloat("_BlurAmount", blurAmount);
            }
            
            transitionOverlay.color = pulseColor;
            yield return null;
        }

        Debug.Log("[HardModeEffect] Effect finished");
        currentEffect = null;
        ResetEffect();
    }

    private void HandleTransitionEffect()
    {
        if (!isTransitioning) return;

        transitionTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(transitionTimer / transitionDuration);

        if (transitioning)
        {
            // Fade in with increased brightness and blur
            float alpha = Mathf.Lerp(0f, transitionColor.a, progress);
            float brightness = Mathf.Lerp(1f, maxBrightness, progress);
            float blur = Mathf.Lerp(0f, maxBlurAmount, progress);

            transitionOverlay.color = new Color(
                transitionColor.r * brightness,
                transitionColor.g * brightness,
                transitionColor.b * brightness,
                alpha
            );

            if (blurMaterial != null)
            {
                blurMaterial.SetFloat("_BlurAmount", blur);
            }
            // Activate shield at the start of transition
            if (progress < 0.1f && player != null)
            {
                player.EnableShield();
            }
        }
        else
        {
            // Fade out with increased brightness and blur
            float alpha = Mathf.Lerp(transitionColor.a, 0f, progress);
            float brightness = Mathf.Lerp(maxBrightness, 1f, progress);
            float blur = Mathf.Lerp(maxBlurAmount, 0f, progress);

            transitionOverlay.color = new Color(
                transitionColor.r * brightness,
                transitionColor.g * brightness,
                transitionColor.b * brightness,
                alpha
            );

            if (blurMaterial != null)
            {
                blurMaterial.SetFloat("_BlurAmount", blur);
            }

            if (progress >= 1f)
            {
                isTransitioning = false;
            }
        }
    }

    private void CheckForStageTransition()
    {
        if (transitioning) return;

        float currentScore = ScoreManager.Instance.getScorce();
        for (int i = currentIndex + 1; i < stages.Length; i++)
        {
            if (currentScore >= stages[i].triggerScore)
            {
                nextIndex = i;
                transitioning = true;
                isTransitioning = true;
                transitionTimer = 0f;
                break;
            }
        }
    }

    private void UpdateBackgroundPosition()
    {
        if (!transitioning || nextIndex == -1) return;

        var current = stages[currentIndex].quad.transform;
        var next = stages[nextIndex].quad.transform;

        // Move both quads down at the same speed
        float moveAmount = scrollSpeed * Time.deltaTime;
        current.Translate(Vector3.down * moveAmount);
        next.Translate(Vector3.down * moveAmount);

        // When the current quad has moved completely off screen
        if (current.position.y <= -quadHeight)
        {
            // Reset the current quad position to above
            current.position = new Vector3(0, quadHeight, 0);
            currentIndex = nextIndex;
            nextIndex = -1;
            transitioning = false;
            isTransitioning = true;
            transitionTimer = 0f;
        }
    }
}
