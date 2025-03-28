using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

/// <summary>
/// ML-Agents compatible agent that can move, press a FoodButton,
/// and collect spawned Food objects.
/// </summary>
public class FoodAgent : Agent
{
    [Header("Dependencies")]
    [SerializeField] private FoodSpawner foodSpawner;
    [SerializeField] private FoodButton foodButton;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody agentRigidbody;

    public event EventHandler OnAteFood;
    public event EventHandler OnEpisodeBeginEvent;

    private void Start()
    {
        agentRigidbody = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        // Reset agent's position and velocity
        transform.localPosition = new Vector3(
            UnityEngine.Random.Range(0f, 0f),
            0.5f, // Slightly above ground
            UnityEngine.Random.Range(-2.0f, +2.0f)
        );
        agentRigidbody.velocity = Vector3.zero;

        // Reset button and spawner
        foodSpawner.ResetSpawner();
        foodButton.ResetButton();


        // Fire event (optional for training monitoring)
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
            sensor.AddObservation(0f); // x
            sensor.AddObservation(0f); // z
        }

        // Total: 1 (button) + 2 (button direction) + 1 (food exists) + 2 (food direction) = 6 observations
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int moveX = actions.DiscreteActions[0]; // 0 = none, 1 = left, 2 = right
        int moveZ = actions.DiscreteActions[1]; // 0 = none, 1 = back, 2 = forward
        int useAction = actions.DiscreteActions[2]; // 0 = no, 1 = use

        Vector3 direction = Vector3.zero;

        switch (moveX)
        {
            case 1: direction.x = -1f; break; // left
            case 2: direction.x = +1f; break; // right
        }

        switch (moveZ)
        {
            case 1: direction.z = -1f; break; // back
            case 2: direction.z = +1f; break; // forward
        }

        // Move agent
        agentRigidbody.velocity = direction.normalized * moveSpeed + new Vector3(0, agentRigidbody.velocity.y, 0);

        // Attempt to use button when action is triggered
        if (useAction == 1)
        {
            Collider[] nearby = Physics.OverlapBox(transform.position, Vector3.one * 0.5f);
            foreach (var hit in nearby)
            {
                if (hit.TryGetComponent<FoodButton>(out FoodButton fb))
                {
                    if (fb.CanUseButton())
                    {
                        fb.UseButton();
                        AddReward(1f); // Reward for successful button press
                        break;
                    }
                }
            }
        }

        if (MaxStep > 0)
        {
            AddReward(-1f / MaxStep);  // step penalty
        }

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discrete = actionsOut.DiscreteActions;

        // Move X
        int horizontal = Mathf.RoundToInt(Input.GetAxisRaw("Horizontal"));
        discrete[0] = horizontal switch
        {
            < 0 => 1,
            > 0 => 2,
            _ => 0
        };

        // Move Z
        int vertical = Mathf.RoundToInt(Input.GetAxisRaw("Vertical"));
        discrete[1] = vertical switch
        {
            < 0 => 1,
            > 0 => 2,
            _ => 0
        };

        // Use Button
        discrete[2] = Input.GetKey(KeyCode.E) ? 1 : 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entered trigger with: " + other.name);

        if (other.TryGetComponent<Food>(out Food food))
        {
            AddReward(1f); // Reward for collecting food
            Destroy(food.gameObject);
            OnAteFood?.Invoke(this, EventArgs.Empty);
            EndEpisode(); // Success: reset
        }
    }
}
