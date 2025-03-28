using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

/// <summary>
/// ML-Agents implementation for training an agent to navigate to a target goal.
/// Handles observations, actions, rewards, and episode resets.
/// </summary>
public class MoveToGoalAgent : Agent
{
    // ========== SERIALIZED FIELDS (Editable in Unity Inspector) ==========
    [Header("References")]
    [Tooltip("Transform of the target goal object")]
    [SerializeField] private Transform targetTransform;

    [Header("Movement Settings")]
    [Tooltip("Movement speed multiplier for the agent")]
    [SerializeField] private float moveSpeed = 3f;

    [Header("Visual Feedback")]
    [Tooltip("Material to show when agent succeeds")]
    [SerializeField] private Material winMaterial;

    [Tooltip("Material to show when agent fails")]
    [SerializeField] private Material loseMaterial;

    [Tooltip("Floor mesh renderer for success/failure feedback")]
    [SerializeField] private MeshRenderer floorMeshRenderer;

    // ========== ML-AGENTS CORE METHODS ==========

    /// <summary>
    /// Called at the start of each training episode.
    /// Resets agent and target positions randomly.
    /// </summary>
    public override void OnEpisodeBegin()
    {
        // Reset agent position (X fixed, Z random)
        transform.localPosition = new Vector3(
            Random.Range(0f, 0f),  // X: Fixed at 0 for consistent starting line
            0f,                   // Y: Ground level
            Random.Range(-3f, 3f)  // Z: Random position between -3 and 3
        );

        // Reset target position (X ahead, Z random)
        targetTransform.localPosition = new Vector3(
            Random.Range(3f, 4f),  // X: 3-4 units ahead
            0f,                   // Y: Ground level
            Random.Range(-3f, 3f)  // Z: Random position between -3 and 3
        );
    }

    /// <summary>
    /// Collects environment observations for the neural network.
    /// Provides agent's position and target's position as vector observations.
    /// </summary>
    /// <param name="sensor">The vector sensor to add observations to</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent's position (3 values: x,y,z)
        sensor.AddObservation(transform.localPosition);

        // Target's position (3 values: x,y,z)
        sensor.AddObservation(targetTransform.localPosition);

        // Total observations: 6 float values
    }

    /// <summary>
    /// Executes agent movement based on neural network output.
    /// Converts continuous actions into movement vectors.
    /// </summary>
    /// <param name="actions">Action buffers containing movement values</param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Get continuous actions (output from neural network)
        float moveX = actions.ContinuousActions[0]; // Horizontal movement (-1 to 1)
        float moveZ = actions.ContinuousActions[1]; // Vertical movement (-1 to 1)

        // Apply movement
        transform.localPosition += new Vector3(moveX, 0f, moveZ) * Time.deltaTime * moveSpeed;

        // Optional: Small negative reward per step to encourage efficiency
        // AddReward(-0.001f);
    }

    /// <summary>
    /// Allows manual control of the agent for testing (Human-in-the-loop).
    /// Maps keyboard input to continuous actions.
    /// </summary>
    /// <param name="actionsOut">Output action buffer to modify</param>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;

        // Map keyboard input to actions:
        continuousActions[0] = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        continuousActions[1] = Input.GetAxisRaw("Vertical");   // W/S or Up/Down
    }

    // ========== UNITY COLLISION HANDLING ==========

    /// <summary>
    /// Handles trigger collisions with goal and walls.
    /// Provides rewards/punishments and visual feedback.
    /// </summary>
    /// <param name="other">The collider entered</param>
    private void OnTriggerEnter(Collider other)
    {
        // Goal reached
        if (other.TryGetComponent<Goal>(out Goal goal))
        {
            Debug.Log("GOAL HIT!"); // Training monitor

            SetReward(+1f); // Positive reward
            floorMeshRenderer.material = winMaterial; // Visual feedback
            EndEpisode(); // Reset environment
        }

        // Wall collision
        if (other.TryGetComponent<Wall>(out Wall wall))
        {
            Debug.Log("Wall HIT!"); // Training monitor

            SetReward(-1f); // Negative reward
            floorMeshRenderer.material = loseMaterial; // Visual feedback
            EndEpisode(); // Reset environment
        }
    }
}
