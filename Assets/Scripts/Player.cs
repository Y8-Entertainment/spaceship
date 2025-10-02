using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UI;
using static PlayModeSelector;
using static UnityEngine.GraphicsBuffer;

public class Player : MonoBehaviour
{
    public InputAction moveAction;
    private Vector2 moveInput;
    private Rigidbody2D rb;
    [SerializeField]
    private float _speed = 5.0f;
    [SerializeField]
    private float _baseCursorSpeed = 10.0f;
    private float _cursorSpeed;

    [SerializeField]
    Transform _attackPoint;
    [SerializeField] private float maxMana = 100f;
    [SerializeField] private float manaRegenRate = 20f;   // per second
    [SerializeField] private float manaDrainRate = 30f;   // per second
    [SerializeField] private float manaCooldown = 2f;     // time to wait when mana hits 0
    [SerializeField] private Image manaFillImage;

    [SerializeField] private float maxHealth = 100f;

    [SerializeField] private Image healthImage;

    [SerializeField] private Image flame1;
    [SerializeField] private Image flame2;
    [SerializeField] private Image flame3;
   

    private float _currentHealth;
    private float _currentMana;
    private bool manaDepleted = false;
    private float cooldownTimer = 100.0f;


    [SerializeField]
    private float _attackTime = 0.5f;
    private float _nextBullet;

    //take dame effect
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashInterval = 0.1f;
    [SerializeField] private float flashDuration = 0.1f;
    private Coroutine flashCoroutine;

    [SerializeField]
    private GameObject _bullet;
    [SerializeField]
    private GameObject _explosion;
    private int attackMode;
    private bool isMouse = true;


    private GameManager gameManager;
    private void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
    }


    private float originalBulletDamage;
    private float originalBulletSize;
    private float originalBulletSpeed;


    [Header("Cursor Settings")]
    [SerializeField] private Texture2D cursorTexture;  // Kéo texture con trỏ chuột vào đây
    [SerializeField] private Vector2 cursorHotspot = new Vector2(16, 16);  // Điểm click của con trỏ, mặc định là giữa texture
   
   

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        moveAction.Enable();
        rb = GetComponent<Rigidbody2D>();

        // Khởi tạo tốc độ chuột
        _cursorSpeed = _baseCursorSpeed * SettingsManager.GetMouseSensitivity();

        if (manaFillImage == null)
        {
            GameObject manaObject = GameObject.Find("Mana");
            if (manaObject != null)
            {
                manaFillImage = manaObject.GetComponent<Image>();
            }
        }
        if (healthImage == null)
        {
            GameObject healthObject = GameObject.Find("Health");
            if (healthObject != null)
            {
                healthImage = healthObject.GetComponent<Image>();
            }
        }
        _nextBullet = Time.time + _attackTime;
        _currentHealth = maxHealth;
        _currentMana = maxMana;

        originalBulletDamage = bulletDamage;
        originalBulletSize = bulletSize;
        originalBulletSpeed = _attackTime;

        UpdateControlMode();

    }

    private void UpdateControlMode()
    {
        isMouse = PlayModeSelector.CurrentPlayMode == PlayModeSelector.PlayMode.Cursor;
    }

    void Attack()
    {
        bool isHoldingFire;
        if (isMouse)
        {
            isHoldingFire = Mouse.current.leftButton.isPressed;
        }
        else
        {
            isHoldingFire = Keyboard.current.spaceKey.isPressed;
        }
        float currentAttackTime = (!manaDepleted && isHoldingFire) ? _attackTime * 0.1f : _attackTime;

        if (Time.time > _nextBullet)
        {
            if (attackMode == 0)
            {
                GameObject bulletObj = Instantiate(_bullet, _attackPoint.position, _bullet.transform.rotation);
            }
            else if (attackMode == 1)
            {
                GameObject bulletObj = Instantiate(_bullet, _attackPoint.position, _bullet.transform.rotation);
                if (bulletObj.TryGetComponent(out Bullet bullet))
                {
                    bullet.setDamage(bulletDamage);
                    bullet.setSize(bulletSize);
                }
            }

            else if (attackMode == 2)
            {
                float offset = 0.15f;

                // Tạo hai vị trí bắn lệch trái/phải (theo trục X)
                Vector3 leftPos = _attackPoint.position + Vector3.left * offset;
                Vector3 rightPos = _attackPoint.position + Vector3.right * offset;

                // Bắn đạn thẳng lên (hướng mặc định lên trên)
                GameObject leftBullet = Instantiate(_bullet, leftPos, Quaternion.Euler(0, 0, 90f));
                GameObject rightBullet = Instantiate(_bullet, rightPos, Quaternion.Euler(0, 0, 90f));

                if (leftBullet.TryGetComponent(out Bullet lb))
                {
                    lb.setDamage(bulletDamage);
                    lb.setSize(bulletSize);
                }

                if (rightBullet.TryGetComponent(out Bullet rb))
                {
                    rb.setDamage(bulletDamage);
                    rb.setSize(bulletSize);
                }
            }
            else if (attackMode == 3)
            {
                float spreadAngle = 15f; // adjust for how wide the spread is

                // Array of angles: center (0), left (+spread), right (-spread)
                float[] angles = { 0f, spreadAngle, -spreadAngle };

                foreach (float angle in angles)
                {
                    Quaternion rotation = _attackPoint.rotation * Quaternion.Euler(0, 0, 90f + angle);
                    GameObject bulletObj = Instantiate(_bullet, _attackPoint.position, rotation);

                    if (bulletObj.TryGetComponent(out Bullet bullet))
                    {
                        bullet.setDamage(bulletDamage);
                        bullet.setSize(bulletSize);
                    }
                }
            }


            AudioManager.Instance.PlaySFX(AudioManager.Instance.fireSound);
            _nextBullet += currentAttackTime;
        }


    }



    // Update is called once per frame
    void Update()
    {
        HandleHealth();
        HandleMana();
        Attack();
        if (gameManager.IsGameOver()) return;

        // 👉 Cheat: bấm C để set máu = 99999
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            if (_currentHealth < 1000f)
            {
                _currentHealth = 99999f;
                maxHealth = 99999f; // để thanh máu hiển thị đúng
                Debug.Log("Cheat: Player health boosted to 99999!");
            }
            else
            {
                _currentHealth = 10f;
            }
        }

        if (Keyboard.current.xKey.wasPressedThisFrame)
        {
            if (_attackTime < 0.0001f) _attackTime = 0.5f;
            else _attackTime = 0.00001f;

            Debug.Log("Cheat: Player health boosted to 99999!");
        }

        // Cập nhật tốc độ chuột khi có thay đổi
        _cursorSpeed = _baseCursorSpeed * SettingsManager.GetMouseSensitivity();

        // Kiểm tra thay đổi mode
        if ((PlayModeSelector.CurrentPlayMode == PlayModeSelector.PlayMode.Cursor && !isMouse) ||
            (PlayModeSelector.CurrentPlayMode == PlayModeSelector.PlayMode.Keyboard && isMouse))
        {
            UpdateControlMode();
        }

        
    }

    void FixedUpdate()
    {
        if (PlayModeSelector.CurrentPlayMode == PlayModeSelector.PlayMode.Keyboard)
        {
            MoveByKeyBoard();
        }
        else
        {
            MoveByCursor();
        }
    }


    void MoveByKeyBoard()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        Vector2 newPos = rb.position + moveInput * _speed * Time.fixedDeltaTime;
        newPos = ClampPosition(newPos);
        rb.MovePosition(newPos);
    }

    void MoveByCursor()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPos.z = 0f;

        Vector2 target = Vector2.MoveTowards(rb.position, mouseWorldPos, _cursorSpeed * Time.fixedDeltaTime);
        target = ClampPosition(target);
        rb.MovePosition(target);
    }

    Vector2 ClampPosition(Vector2 pos)
    {
        pos.x = Mathf.Clamp(pos.x, -8.47f, 8.47f);
        pos.y = Mathf.Clamp(pos.y, -4.47f, 4.47f);
        return pos;
    }

    void OnTriggerEnter2D(Collider2D target)
    {
        if (target.CompareTag("Asteroids") || target.CompareTag("SmallAsteroids"))
        {
            if (isShieldActive)
            {
                Debug.Log("Damage blocked by shield!");
                DisableShield();
                return;
            }

            if (target.TryGetComponent(out Asteroids big))
            {
                _currentHealth = big.DealDamage(_currentHealth);
            }
            else if (target.TryGetComponent(out SmallAsteroids small))
            {
                // Giảm máu ít hơn khi va chạm thiên thạch nhỏ
                _currentHealth = small.DealDamage(_currentHealth);
            }

            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(TakedamageEffect(flashDuration));
            AudioManager.Instance.PlaySFX(AudioManager.Instance.takeDamgeSound);

            if (_currentHealth <= 0f)
            {
                HandleHealth();
                Die();
                gameManager.GameOver();
            }

           
        }
    }

    void HandleHealth()
    {
        healthImage.fillAmount = _currentHealth / maxHealth;
    }

    void HandleMana()
    {
        if (manaDepleted)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= manaCooldown)
            {
                manaDepleted = false;
                cooldownTimer = 0f;
            }
            return;
        }

        bool isHoldingFire;
        if (isMouse)
        {
            isHoldingFire = Mouse.current.leftButton.isPressed;
        }
        else
        {
            isHoldingFire = Keyboard.current.spaceKey.isPressed;
        }

        if (isHoldingFire)
        {
            _currentMana -= manaDrainRate * Time.deltaTime;
            if (_currentMana <= 0f)
            {
                _currentMana = 0f;
                manaDepleted = true;
            }
        }
        else
        {
            _currentMana += manaRegenRate * Time.deltaTime;
            _currentMana = Mathf.Min(_currentMana, maxMana);
        }

        manaFillImage.fillAmount = _currentMana / maxMana;
    }
    private IEnumerator TakedamageEffect(float duration)
    {
        float elapsed = 0f;
        bool toggle = false;

        while (elapsed < duration)
        {
            spriteRenderer.color = toggle ? flashColor : originalColor;
            toggle = !toggle;
            elapsed += flashInterval;
            yield return new WaitForSeconds(flashInterval);
        }

        spriteRenderer.color = originalColor;
        flashCoroutine = null;
    }

    public void Die()
    {
        var explo = Instantiate(_explosion, transform.position, Quaternion.identity);
        Destroy(explo, 0.85f);
        Destroy(gameObject);
    }

    ///item heal
    public void Heal(float amount)
    {
        _currentHealth += amount;
        AudioManager.Instance.PlaySFX(AudioManager.Instance.healSound);
        _currentHealth = Mathf.Min(_currentHealth, maxHealth); // Maxium
    }


    ///item shield

    [SerializeField] private GameObject shieldEffect;
    [SerializeField] private float shieldDuration = 5f;

    private bool isShieldActive = false;
    private Coroutine shieldCoroutine;

    private void DisableShield()
    {
        isShieldActive = false;
        shieldEffect.SetActive(false);
        Debug.Log("Shield deactivated");

        if (shieldEffect != null)
        {
            StopCoroutine(shieldCoroutine);
            shieldCoroutine = null;
        }
    }

    //public void EnableShield()
    //{
    //    if (isShieldActive) return; //bat 1 lan
    //    AudioManager.Instance.PlaySFX(AudioManager.Instance.gainShieldSound);
    //    isShieldActive = true;
    //    shieldEffect.SetActive(true);
    //    Debug.Log("Shield activated");

    //    // tat
    //    shieldCoroutine = StartCoroutine(ShieldTimer());
    //}
    public void EnableShield()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.gainShieldSound);

        if (isShieldActive)
        {
            // Nếu khiên đang bật, reset lại coroutine cũ
            if (shieldCoroutine != null)
            {
                shieldEffect.SetActive(true);
                StopCoroutine(shieldCoroutine);
            }
        }
        else
        {
            isShieldActive = true;
            shieldEffect.SetActive(true);
            Debug.Log("Shield activated");
        }

        // Bắt đầu lại thời gian khiên
        shieldCoroutine = StartCoroutine(ShieldTimer());
    }



    public bool IsShieldActive()
    {
        return isShieldActive;
    }
    private IEnumerator ShieldTimer()
    {
        float warningTime = 1f;
        float normalTime = shieldDuration - warningTime;


        yield return new WaitForSeconds(normalTime);


        float elapsed = 0f;
        bool visible = true;

        while (elapsed < warningTime)
        {
            if (!isShieldActive) yield break;

            visible = !visible;
            shieldEffect.SetActive(visible);

            elapsed += 0.1f;
            yield return new WaitForSeconds(0.2f);
        }


        if (isShieldActive)
        {
            DisableShield();
        }
    }

    ///item tang damage

    [SerializeField] private float bulletDamage = 1f;
    [SerializeField] private float bulletSize = 1f;

    private Coroutine upgradeCoroutine;
    private Coroutine fireRateCoroutine;
    //public void UpgradeBullet(float damage, float size, float duration)
    //{
    //    // stop buff
    //    if (upgradeCoroutine != null)
    //    {
    //        StopCoroutine(upgradeCoroutine);
    //    }

    //    // reset
    //    bulletDamage = originalBulletDamage + damage;
    //    bulletSize = originalBulletSize + size;

    //    // duration
    //    upgradeCoroutine = StartCoroutine(ResetBulletStats(duration));
    //}
    public void SpecialUpgrade(float damageBonus, float sizeBonus, float duration)
    {
        if (upgradeCoroutine != null)
        {
            StopCoroutine(upgradeCoroutine);
        }

        if (attackMode == 0)
        {
            // Lần đầu buff đạn thường
            attackMode = 1;
            bulletDamage += damageBonus;
            bulletSize += sizeBonus;
            upgradeCoroutine = StartCoroutine(ResetUpgrade(duration));
            Debug.Log($"Damage: {bulletDamage}");
        }
        else if (attackMode == 1)
        {
            // Lần đầu buff đạn thường
            attackMode = 2;
            upgradeCoroutine = StartCoroutine(ResetUpgrade(duration));
            Debug.Log($"Damage: {bulletDamage}");
        }
        else if (attackMode == 2)
        {
            // Nếu ăn thêm khi đang buff, chuyển sang 3 tia
            attackMode = 3;
            upgradeCoroutine = StartCoroutine(ResetUpgrade(duration));
            Debug.Log($"Damage: {bulletDamage}");
        }
        else if (attackMode == 3)
        {
            // Nếu đã là 3 tia, thì reset lại thời gian buff
            StopCoroutine(upgradeCoroutine);
            upgradeCoroutine = StartCoroutine(ResetUpgrade(duration));
            Debug.Log($"Damage: {bulletDamage}");
        }
    }

    private IEnumerator ResetUpgrade(float delay)
    {
        yield return new WaitForSeconds(delay);

        bulletDamage = originalBulletDamage;
        bulletSize = originalBulletSize;
        attackMode = 0;
        upgradeCoroutine = null;
    }

    ///update bullet speed
    public void IncreaseFireRate(float rateMultiplier, float duration)
    {
        if (fireRateCoroutine != null)
            StopCoroutine(fireRateCoroutine);

        _attackTime /= rateMultiplier;
        fireRateCoroutine = StartCoroutine(ResetFireRate(duration));
    }

    private IEnumerator ResetFireRate(float duration)
    {
        yield return new WaitForSeconds(duration);
        _attackTime = originalBulletSpeed;
        fireRateCoroutine = null;
    }

    public void PlayEffect(GameObject fx)
    {
        if (fx == null) return;
        GameObject go = Instantiate(fx, transform.position, Quaternion.identity);
        Destroy(go, 1.5f);
    }
}
