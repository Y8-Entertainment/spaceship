using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("Input")]
    public InputAction moveAction;
    private Vector2 moveInput;
    private Rigidbody2D rb;
    private bool isMouse = true;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float cursorSpeed = 10f;
    [SerializeField] private Vector2 minBounds = new Vector2(-8.47f, -4.47f);
    [SerializeField] private Vector2 maxBounds = new Vector2(8.47f, 4.47f);

    [Header("Attack")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float attackCooldown = 0.5f;
    private float nextShoot;

    [Header("Health & Mana")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Image healthImage;
    [SerializeField] private float maxMana = 100f;
    [SerializeField] private Image manaImage;

    private float currentHealth;
    private float currentMana;
    private bool manaDepleted = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        currentMana = maxMana;
    }

    void OnEnable()
    {
        moveAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
    }

    void Update()
    {
        HandleAttack();
        HandleHealthUI();
        HandleManaUI();

        // Chuyển chế độ input
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            isMouse = !isMouse;
            Debug.Log("Switch control mode: " + (isMouse ? "Mouse" : "Keyboard"));
        }
    }

    void FixedUpdate()
    {
        if (isMouse) MoveByCursor();
        else MoveByKeyboard();
    }

    private void MoveByKeyboard()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        Vector2 newPos = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        newPos = ClampPosition(newPos);
        rb.MovePosition(newPos);
    }

    private void MoveByCursor()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorld.z = 0f;

        Vector2 target = Vector2.MoveTowards(rb.position, mouseWorld, cursorSpeed * Time.fixedDeltaTime);
        target = ClampPosition(target);
        rb.MovePosition(target);
    }

    private Vector2 ClampPosition(Vector2 pos)
    {
        pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
        pos.y = Mathf.Clamp(pos.y, minBounds.y, maxBounds.y);
        return pos;
    }

    private void HandleAttack()
    {
        bool shoot = isMouse ? Mouse.current.leftButton.isPressed : Keyboard.current.spaceKey.isPressed;

        if (shoot && Time.time >= nextShoot)
        {
            Instantiate(bulletPrefab, attackPoint.position, Quaternion.identity);
            nextShoot = Time.time + attackCooldown;
        }
    }

    private void HandleHealthUI()
    {
        if (healthImage != null)
            healthImage.fillAmount = currentHealth / maxHealth;
    }

    private void HandleManaUI()
    {
        if (manaImage != null)
            manaImage.fillAmount = currentMana / maxMana;
    }

    // Public methods để các item khác gọi
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player died!");
        Destroy(gameObject);
    }
}
