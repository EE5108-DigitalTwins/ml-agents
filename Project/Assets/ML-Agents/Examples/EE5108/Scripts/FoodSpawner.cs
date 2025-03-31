using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [Header("Prefab Settings")]
    [SerializeField] private GameObject foodPrefab;
    [SerializeField] private bool spawnAsChild = true;

    [Header("Spawn Area Settings")]
    [SerializeField] private Vector3 localCenter = Vector3.zero;

    [Tooltip("Minimum spawn bounds (local space)")]
    [SerializeField] private Vector3 minSpawnBounds = new Vector3(-2f, 0f, -2f);

    [Tooltip("Maximum spawn bounds (local space)")]
    [SerializeField] private Vector3 maxSpawnBounds = new Vector3(2f, 0f, 2f);

    [Tooltip("Fixed height above ground")]
    [SerializeField] private float spawnHeight = 0.5f;

    private GameObject currentFood;

    public void SpawnFood()
    {
        if (foodPrefab == null)
        {
            Debug.LogError("Food prefab not assigned in FoodSpawner!", this);
            return;
        }

        // Generate random position within defined bounds
        Vector3 localSpawnPos = new Vector3(
            Random.Range(minSpawnBounds.x, maxSpawnBounds.x),
            spawnHeight, // Use fixed height
            Random.Range(minSpawnBounds.z, maxSpawnBounds.z)
        ) + localCenter;

        Vector3 worldSpawnPos = transform.TransformPoint(localSpawnPos);

        ResetSpawner();

        currentFood = Instantiate(
            foodPrefab,
            worldSpawnPos,
            Quaternion.identity,
            spawnAsChild ? transform : null
        );

        currentFood.name = $"Food_{Random.Range(1000, 9999)}";
    }

    public bool HasFoodSpawned() => currentFood != null;

    public Transform GetLastFoodTransform() => currentFood?.transform;

    public void ResetSpawner()
    {
        if (currentFood == null) return;

        if (Application.isPlaying)
            Destroy(currentFood);
        else
            DestroyImmediate(currentFood);

        currentFood = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 worldCenter = transform.TransformPoint(localCenter);

        // Calculate spawn area dimensions
        Vector3 size = new Vector3(
            Mathf.Abs(minSpawnBounds.x) + Mathf.Abs(maxSpawnBounds.x),
            0.1f,
            Mathf.Abs(minSpawnBounds.z) + Mathf.Abs(maxSpawnBounds.z)
        );

        // Draw spawn area
        Gizmos.DrawWireCube(
            worldCenter + Vector3.up * spawnHeight,
            size
        );

        // Draw min/max markers
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.TransformPoint(localCenter + minSpawnBounds + Vector3.up * spawnHeight), 0.15f);
        Gizmos.DrawSphere(transform.TransformPoint(localCenter + maxSpawnBounds + Vector3.up * spawnHeight), 0.15f);
    }
}
