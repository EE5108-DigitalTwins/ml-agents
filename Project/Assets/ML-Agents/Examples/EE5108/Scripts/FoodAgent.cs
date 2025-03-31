using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class FoodAgent : Agent
{
    [Header("Dependencies")]
    [SerializeField] private FoodSpawner foodSpawner;
    [SerializeField] private FoodButton foodButton;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;

    [Header("Spawn Location Settings")]
    [SerializeField] private Vector3 minSpawnBounds = new Vector3(-2f, 0f, -2f);
    [SerializeField] private Vector3 maxSpawnBounds = new Vector3(2f, 0f, 2f);
    [SerializeField] private float spawnHeight = 0.5f;

    [Header("Reward Settings")]
    [SerializeField] private float buttonPressReward = 1f;
    [SerializeField] private float foodCollectionReward = 2f;
    [SerializeField] private float wallCollisionPenalty = -1f;
    [SerializeField] private float stepPenalty = -0.001f;

    private Rigidbody agentRigidbody;
    private Vector3 originalPosition;

    public event EventHandler OnAteFood;
    public event EventHandler OnEpisodeBeginEvent;

    private void Start()
    {
        agentRigidbody = GetComponent<Rigidbody>();
        originalPosition = transform.localPosition;
    }

    public override void OnEpisodeBegin()
    {
        // Reset agent's position within defined bounds
        transform.localPosition = new Vector3(
            UnityEngine.Random.Range(minSpawnBounds.x, maxSpawnBounds.x),
            spawnHeight,
            UnityEngine.Random.Range(minSpawnBounds.z, maxSpawnBounds.z)
        );
        agentRigidbody.velocity = Vector3.zero;

        // Reset button and spawner
        foodSpawner.ResetSpawner();
        foodButton.ResetButton();

        OnEpisodeBeginEvent?.Invoke(this, EventArgs.Empty);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Button availability (1 or 0)
        sensor.AddObservation(foodButton.CanUseButton() ? 1f : 0f);

        // Direction to FoodButton
        Vector3 dirToButton = (foodButton.transform.localPosition - transform.localPosition).normalized;
        sensor.AddObservation(dirToButton.x);
        sensor.AddObservation(dirToButton.z);

        // Whether food exists
        bool foodExists = foodSpawner.HasFoodSpawned();
        sensor.AddObservation(foodExists ? 1f : 0f);

        // Direction to Food if spawned
        if (foodExists)
        {
            Vector3 dirToFood = (foodSpawner.GetLastFoodTransform().localPosition - transform.localPosition).normalized;
            sensor.AddObservation(dirToFood.x);
            sensor.AddObservation(dirToFood.z);
        }
        else
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Movement handling
        int moveX = actions.DiscreteActions[0];
        int moveZ = actions.DiscreteActions[1];
        int useAction = actions.DiscreteActions[2];

        Vector3 direction = new Vector3(
            moveX == 1 ? -1f : (moveX == 2 ? 1f : 0f),
            0f,
            moveZ == 1 ? -1f : (moveZ == 2 ? 1f : 0f)
        );

        agentRigidbody.velocity = direction.normalized * moveSpeed + new Vector3(0, agentRigidbody.velocity.y, 0);

        // Button press handling
        if (useAction == 1)
        {
            Collider[] nearby = Physics.OverlapBox(transform.position, Vector3.one * 1f);
            foreach (var hit in nearby)
            {
                if (hit.TryGetComponent<FoodButton>(out FoodButton fb) && fb.CanUseButton())
                {
                    fb.UseButton();
                    AddReward(buttonPressReward);
                    break;
                }
            }
        }

        // Step penalty
        if (MaxStep > 0)
        {
            AddReward(stepPenalty);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Food>(out Food food))
        {
            AddReward(foodCollectionReward);
            Destroy(food.gameObject);
            OnAteFood?.Invoke(this, EventArgs.Empty);
            EndEpisode();
        }
        else if (other.TryGetComponent<Wall>(out Wall wall))
        {
            AddReward(wallCollisionPenalty);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discrete = actionsOut.DiscreteActions;
        discrete[0] = Mathf.RoundToInt(Input.GetAxisRaw("Horizontal")) switch { < 0 => 1, > 0 => 2, _ => 0 };
        discrete[1] = Mathf.RoundToInt(Input.GetAxisRaw("Vertical")) switch { < 0 => 1, > 0 => 2, _ => 0 };
        discrete[2] = Input.GetKey(KeyCode.E) ? 1 : 0;
    }

    // Visualize spawn area in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Vector3 center = new Vector3(
            (minSpawnBounds.x + maxSpawnBounds.x) / 2,
            spawnHeight,
            (minSpawnBounds.z + maxSpawnBounds.z) / 2
        );
        Vector3 size = new Vector3(
            maxSpawnBounds.x - minSpawnBounds.x,
            0.1f,
            maxSpawnBounds.z - minSpawnBounds.z
        );
        Gizmos.DrawCube(center, size);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, size);
    }
}
