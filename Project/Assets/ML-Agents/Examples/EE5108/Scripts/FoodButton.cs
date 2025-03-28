using UnityEngine;
using System.Collections;

/// <summary>
/// Interactable button that visually animates and changes color when pressed.
/// Triggers food spawning on use.
/// </summary>
public class FoodButton : MonoBehaviour
{
    [Header("Spawner Reference")]
    [SerializeField] private FoodSpawner foodSpawner;

    [Header("Button Settings")]
    [SerializeField] private bool useOnce = true;

    [Header("Visual Feedback")]
    [SerializeField] private float pressDistance = 0.1f;
    [SerializeField] private float pressDuration = 0.1f;
    [SerializeField] private Color pressedColor = Color.red;
    [SerializeField] private Color defaultColor = Color.green;

    private bool isUsed = false;
    private bool isAnimating = false;

    private Vector3 originalPosition;
    private Coroutine pressCoroutine;

    private Renderer buttonRenderer;

    private void Start()
    {
        originalPosition = transform.localPosition;

        buttonRenderer = GetComponent<Renderer>();
        if (buttonRenderer != null)
        {
            buttonRenderer.material.color = defaultColor;
        }
    }

    public bool CanUseButton()
    {
        return !isUsed && !isAnimating;
    }

    public void UseButton()
    {
        if (!CanUseButton())
            return;

        // Spawn food
        if (foodSpawner != null)
        {
            foodSpawner.SpawnFood();
            Debug.Log("FoodButton: Spawned food!");
        }

        // Animate and change color
        if (pressCoroutine != null)
            StopCoroutine(pressCoroutine);

        pressCoroutine = StartCoroutine(AnimatePress());

        if (buttonRenderer != null)
        {
            buttonRenderer.material.color = pressedColor;
        }

        if (useOnce)
        {
            isUsed = true;
        }
    }

    public void ResetButton()
    {
        isUsed = false;
        isAnimating = false;  // Ensure button can be used again
        Debug.Log("Reset Food Button");

        // start training with this
        // transform.localPosition = originalPosition;

        // and replace with this
        transform.localPosition = new Vector3(
        UnityEngine.Random.Range(4f, 5f),
        -0.5f, // Slightly above ground
        UnityEngine.Random.Range(-2.5f, +2.5f)
        );

        if (buttonRenderer != null)
        {
            buttonRenderer.material.color = defaultColor;
        }
    }

    private IEnumerator AnimatePress()
    {
        isAnimating = true;

        // Vector3 pressedPosition = originalPosition - new Vector3(0, pressDistance, 0);
        Vector3 pressedPosition = transform.localPosition - new Vector3(0, pressDistance, 0);

        float t = 0f;
        while (t < pressDuration)
        {
            t += Time.deltaTime;
            // transform.localPosition = Vector3.Lerp(originalPosition, pressedPosition, t / pressDuration);
            transform.localPosition = Vector3.Lerp(transform.localPosition, pressedPosition, t / pressDuration);
            yield return null;
        }

        transform.localPosition = pressedPosition;

        // Optional bounce back for reusable button
        if (!useOnce)
        {
            yield return new WaitForSeconds(0.1f);

            // Animate back to original position
            t = 0f;
            while (t < pressDuration)
            {
                t += Time.deltaTime;
                transform.localPosition = Vector3.Lerp(pressedPosition, originalPosition, t / pressDuration);
                yield return null;
            }

            transform.localPosition = originalPosition;
        }

        isAnimating = false; // FIX: Allow the button to be pressed again
    }
}
