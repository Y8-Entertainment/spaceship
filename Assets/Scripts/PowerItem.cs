using UnityEngine;



public class PowerItem : MonoBehaviour
{
    [SerializeField] private GameObject pickupEffectPrefab;
    [SerializeField] private float damageBonus = 5f;
    [SerializeField] private float sizeBonus = 0.5f;
    [SerializeField] private float duration = 3f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Power item picked up");

            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                if (pickupEffectPrefab != null)
                {
                    GameObject fx = Instantiate(pickupEffectPrefab, player.transform.position, Quaternion.identity);
                    fx.transform.SetParent(player.transform);
                    Destroy(fx, 1.5f);
                }
                player.SpecialUpgrade(damageBonus, sizeBonus, duration);
                Destroy(gameObject);
            }
        }
    }
}
