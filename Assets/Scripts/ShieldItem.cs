using UnityEngine;

public class ShieldItem : MonoBehaviour
{

    [SerializeField] private GameObject pickupEffectPrefab;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Shield item picked up");

            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                if (pickupEffectPrefab != null)
                {
                    Object fx = Instantiate(pickupEffectPrefab, player.transform.position, Quaternion.identity);
                    Destroy(fx, 1.5f);
                }
                player.EnableShield();
                Destroy(gameObject);  
            }
        }
    }



}
