using UnityEngine;

public class HeathItem : MonoBehaviour
{

    [SerializeField] private float healAmount = 2f;
    [SerializeField] private AudioSource healSound;
    [SerializeField] private GameObject pickupEffectPrefab;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Item heal");

           

            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                if (pickupEffectPrefab != null)
                {
                    GameObject fx = Instantiate(pickupEffectPrefab, player.transform.position, Quaternion.identity);
                    fx.transform.SetParent(player.transform);
                    Destroy(fx, 2f);
                }
                player.Heal(healAmount);
                AudioManager.Instance.PlaySFX(AudioManager.Instance.healSound);
                Destroy(gameObject);
                
            }
        }
        
    }
}
