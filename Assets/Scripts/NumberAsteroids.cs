using System;
using UnityEngine;

public class NumberAsteroids : MonoBehaviour
{
    public GameObject[] asteroidPrefabs; 

    public float spawnRangeX = 12f;  
    public float spawnY = 10f;       

    void SpawnAsteroids()
    {
        int count = UnityEngine.Random.Range(1, 4); // 1 đến 3 viên mỗi lần rơi

        for (int i = 0; i < count; i++)
        {
            int index = UnityEngine.Random.Range(0, asteroidPrefabs.Length);

            // Vị trí spawn hơi lệch nhau theo x (để không trùng)
            float offsetX = UnityEngine.Random.Range(-1.5f, 1.5f);

            // Vị trí spawn rộng hơn theo spawnRangeX đã tăng
            Vector3 spawnPos = new Vector3(UnityEngine.Random.Range(-spawnRangeX, spawnRangeX) + offsetX, spawnY, 0);

            Instantiate(asteroidPrefabs[index], spawnPos, Quaternion.identity);
        }
    }
}
