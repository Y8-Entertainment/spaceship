using UnityEngine;

public class ItemDrop : MonoBehaviour
{
    [SerializeField] private float _fallSpeed = 2.5f;

    void Update()
    {
        transform.Translate(Vector3.down * _fallSpeed * Time.deltaTime);

        if (transform.position.y < -5.3f)
        {
            Destroy(gameObject);    
        }
    }
}