using UnityEngine;
using UnityEngine.InputSystem; // thêm namespace mới

public class CheatCode : MonoBehaviour
{
    [SerializeField] private float defaultAsteroidDamage = 5f;
    private bool isCheatActive = false;

    void Update()
    {
        if (Keyboard.current.cKey.wasPressedThisFrame) // ✅ Input System mới
        {
            isCheatActive = !isCheatActive;

            var asteroids = FindObjectsOfType<Asteroids>();
            foreach (var a in asteroids)
            {
                a.SetAsteroidDamage(isCheatActive ? 0f : defaultAsteroidDamage);
            }

            Debug.Log("Cheat " + (isCheatActive ? "ON (damage=0)" : "OFF (damage=5)"));
        }
    }
}
