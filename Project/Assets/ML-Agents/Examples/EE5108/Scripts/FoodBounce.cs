using UnityEngine;

/// <summary>
/// Makes the food object bounce once and then freeze.
/// </summary>
public class FoodBounce : MonoBehaviour
{
    [Tooltip("Time before freezing the Rigidbody in seconds")]
    [SerializeField] private float freezeDelay = 0.8f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Start freeze timer
        Invoke(nameof(FreezePhysics), freezeDelay);
    }

    private void FreezePhysics()
    {
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true; // Disables physics after bounce
        }
    }
}
