using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using TMPro;

public class MoveToGoalAgent : Agent
{
    [SerializeField] private Transform targetTransform;
    //floor mesh renderer
    [SerializeField] private MeshRenderer floorMeshRenderer;
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;
    private float totalReward = 0f;
    public override void OnEpisodeBegin()
    {
        // transform.localPosition = Vector3.zero;
        //set to random localPosition +/- 2f
        transform.localPosition = new Vector3(Random.Range(-2f, 2f), 0.5f, Random.Range(-2f, 2f));
    }
    //observations
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
    }
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float moveSpeed = 10f;
        float moveX = actionBuffers.ContinuousActions[0];
        float moveZ = actionBuffers.ContinuousActions[1];
        //zero

        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime*moveSpeed;

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Target>(out Target goal))
        {
            SetReward(1.5f);
            float cumulativeReward = GetCumulativeReward();
            totalReward += cumulativeReward;
            // Debug.Log("Goal" + goal.name + "was reached" + "Reward: " + totalReward);
            string message = "Goal" + goal.name + "was reached" + "Reward: " + totalReward;
            PrintScreenMessage(message);
            floorMeshRenderer.material = winMaterial;
            EndEpisode();
            
        }
        //wall
        if (other.TryGetComponent<Wall>(out Wall wall))
        {
            SetReward(-0.2f);
            float cumulativeReward = GetCumulativeReward();
            totalReward += cumulativeReward;
            // Debug.Log("Wall" + wall.name + "was hit" + "Reward: " + totalReward);
            string message = "Wall" + wall.name + "was hit" + "Reward: " + totalReward;
            PrintScreenMessage(message);
            floorMeshRenderer.material = loseMaterial;
            EndEpisode();
            
        }
    }
    //huristic override
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }
    [SerializeField] private TextMeshProUGUI rewardText;
    void PrintScreenMessage(string message)
    {
        rewardText.text = message;
    }
}
