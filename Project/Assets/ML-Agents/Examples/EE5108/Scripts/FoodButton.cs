using UnityEngine;
using System.Collections;

public class FoodButton : MonoBehaviour
{
    [Header("Spawner Reference")]
    [SerializeField] private FoodSpawner foodSpawner;

    [Header("Button Settings")]
    [SerializeField] private bool useOnce = true;

    [Header("Food Spawn Area Settings")]
    [SerializeField] private Vector3 localCenter = Vector3.zero;
    [SerializeField] private Vector3 minSpawnBounds = new Vector3(-2f, 0f, -2f);
    [SerializeField] private Vector3 maxSpawnBounds = new Vector3(2f, 0f, 2f);
    [SerializeField] private float spawnHeight = 0.5f;

    [Header("Visual Feedback")]
    [SerializeField] private float pressDistance = 0.1f;
    [SerializeField] private float pressDuration = 0.1f;
    [SerializeField] private Color pressedColor = Color.red;
    [SerializeField] private Color defaultColor = Color.green;

    private bool isUsed = false;
    private bool isAnimating = false;
    private Vector3 originalPosition;
    private Renderer buttonRenderer;

    private void Start()
    {
        originalPosition = transform.localPosition;
        buttonRenderer = GetComponent<Renderer>();
        buttonRenderer.material.color = defaultColor;
    }

    public bool CanUseButton() => !isUsed && !isAnimating;

    public void UseButton()
    {
        if (!CanUseButton()) return;

        // Set random food spawn position
        Vector3 foodSpawnPos = new Vector3(
            Random.Range(minSpawnBounds.x, maxSpawnBounds.x),
            spawnHeight,
            Random.Range(minSpawnBounds.z, maxSpawnBounds.z)
        ) + localCenter;

        if (foodSpawner != null)
        {
            foodSpawner.transform.localPosition = foodSpawnPos;
            foodSpawner.SpawnFood();
        }

        // Visual feedback
        buttonRenderer.material.color = pressedColor;
        StartCoroutine(AnimatePress());

        if (useOnce) isUsed = true;
    }

    public void ResetButton()
    {
        isUsed = false;
        transform.localPosition = originalPosition; // Always return to original position
        buttonRenderer.material.color = defaultColor;
    }

    private IEnumerator AnimatePress()
    {
        isAnimating = true;
        Vector3 pressedPosition = originalPosition - Vector3.up * pressDistance;

        // Press down
        float timer = 0f;
        while (timer < pressDuration)
        {
            timer += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(originalPosition, pressedPosition, timer / pressDuration);
            yield return null;
        }

        // Return up (if reusable)
        if (!useOnce)
        {
            timer = 0f;
            while (timer < pressDuration)
            {
                timer += Time.deltaTime;
                transform.localPosition = Vector3.Lerp(pressedPosition, originalPosition, timer / pressDuration);
                yield return null;
            }
        }

        isAnimating = false;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw button's original position
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.TransformPoint(originalPosition), Vector3.one * 0.2f);

        // Draw food spawn area
        Gizmos.color = Color.green;
        Vector3 worldCenter = transform.TransformPoint(localCenter);
        Vector3 size = new Vector3(
            Mathf.Abs(minSpawnBounds.x) + Mathf.Abs(maxSpawnBounds.x),
            0.1f,
            Mathf.Abs(minSpawnBounds.z) + Mathf.Abs(maxSpawnBounds.z)
        );
        Gizmos.DrawWireCube(worldCenter + Vector3.up * spawnHeight, size);
    }
}
