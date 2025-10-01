using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class Background : MonoBehaviour
{

    [SerializeField]
    private float _speed = 1.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    private void Move()
    {
        Vector2 offset = new Vector2(0f, Time.time * _speed);
        GetComponent<MeshRenderer>().sharedMaterial.SetTextureOffset("_MainTex", offset);


    }

   
}
