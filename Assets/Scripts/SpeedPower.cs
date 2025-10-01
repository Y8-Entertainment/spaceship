using UnityEngine;

public class SpeedPower : MonoBehaviour
{
    [SerializeField] private float rate = 2f;
    [SerializeField] private float duration = 5f;
    [SerializeField] private GameObject pickupEffectPrefab;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Speed item picked up");

            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                if (pickupEffectPrefab != null)
                {
                    GameObject fx = Instantiate(pickupEffectPrefab, player.transform.position, Quaternion.identity);
                    fx.transform.SetParent(player.transform);
                    Destroy(fx, 2f);
                }
                player.IncreaseFireRate(rate, duration);
                Destroy(gameObject);
            }
        }
    }
}
