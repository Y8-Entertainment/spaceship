using UnityEngine;

public class Star : MonoBehaviour
{
    [SerializeField] private float addScoreByStar = 150f;
    [SerializeField] private AudioSource healSound;
    [SerializeField] private GameObject pickupEffectPrefab;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Picked up a Star!");



            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                if (pickupEffectPrefab != null)
                {
                    GameObject fx = Instantiate(pickupEffectPrefab, player.transform.position, Quaternion.identity);
                    fx.transform.SetParent(player.transform);
                    Destroy(fx, 2f);
                }
                ScoreManager.Instance.AddScore(addScoreByStar);
                AudioManager.Instance.PlaySFX(AudioManager.Instance.healSound);
                Destroy(gameObject);

            }
        }

    }
}
