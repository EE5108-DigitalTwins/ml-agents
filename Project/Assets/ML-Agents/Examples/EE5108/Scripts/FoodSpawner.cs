using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [Header("Prefab Settings")]
    [SerializeField] private GameObject foodPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private Vector3 center = Vector3.zero;

    [Tooltip("Range (half-extents) around center to spawn food randomly")]
    [SerializeField] private Vector2 spawnRange = new Vector2(4f, 4f); // X, Z range

    [Tooltip("Height (Y) offset to spawn above the ground")]
    [SerializeField] private float spawnHeight = 0.5f;

    private GameObject currentFood;

    public void SpawnFood()
    {
        // Generate random position within bounds
        float randomX = Random.Range(-spawnRange.x, spawnRange.x);
        float randomZ = Random.Range(-spawnRange.y, spawnRange.y);

        Vector3 spawnPos = center + new Vector3(randomX, spawnHeight, randomZ);

        // Spawn food
        if (currentFood != null)
        {
            Destroy(currentFood); // Prevent lingering
        }

        currentFood = Instantiate(foodPrefab, spawnPos, Quaternion.identity);
    }

    public bool HasFoodSpawned()
    {
        return currentFood != null;
    }

    public Transform GetLastFoodTransform()
    {
        return currentFood != null ? currentFood.transform : null;
    }

    public void ResetSpawner()
    {
        if (currentFood != null)
        {
            Destroy(currentFood);
            currentFood = null;
        }
    }
}
