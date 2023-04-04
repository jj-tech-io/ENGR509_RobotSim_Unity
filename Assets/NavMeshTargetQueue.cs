using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshTarget
{
    public Vector3 Position { get; set; }
    public float DistanceFromAgent { get; set; }

    public GameObject TargetObject;

    public NavMeshTarget(Vector3 position, GameObject targetObject)
    {
        Position = position;
        TargetObject = targetObject;
    }
    
}

public class NavMeshTargetQueue : MonoBehaviour
{
    [SerializeField] List<GameObject> targetObjects = new List<GameObject>();
    [SerializeField] private GameObject particleSystemPrefab;
    //sound effect
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;

    private Queue<NavMeshTarget> targets = new Queue<NavMeshTarget>();
    private Queue<NavMeshTarget> reachedTargets = new Queue<NavMeshTarget>();
    private NavMeshAgent agent;

    [SerializeField] private Canvas canvas;
    //tm pro text
    [SerializeField] private TextMeshProUGUI textMeshPro;
    private bool allMusghroomsCollected = false;
    private void Start()
    {
        textMeshPro = canvas.GetComponentInChildren<TextMeshProUGUI>();
        agent = GetComponent<NavMeshAgent>();
        textMeshPro.text = "Mushrooms: \n";
        
        // Enqueue targets from the targetObjects list
        foreach (GameObject targetObject in targetObjects)
        {
            AddTarget(targetObject.transform.position, targetObject);
        }

        // Set the destination to the first target, if available
        if (targets.Count > 0)
        {
            agent.SetDestination(targets.Peek().Position);
        }
    }

    private void FixedUpdate()
    {
        if (agent.pathPending || allMusghroomsCollected) return;

        float distanceThreshold = 15.5f;
        if (targets.Count == 0 || Vector3.Distance(agent.transform.position, targets.Peek().Position) <= distanceThreshold)
        {
            if (targets.Count > 0)
            {
                 // Play the particle system on target reached
                PlayParticleSystemOnTargetReached(targets.Peek().TargetObject);
                // Destroy the target GameObject and dequeue the target
                StartCoroutine(TargetReachedCoroutine(targets.Peek()));
                reachedTargets.Enqueue(targets.Peek());
                
                targets.Dequeue();
                // Print the updated queue
                PrintUpdatedQueue();

            }

            // Set the destination to the next target, if available
            if (targets.Count > 0)
            {
                agent.SetDestination(targets.Peek().Position);
            }
            if (targets.Count == 0)
            {
                allMusghroomsCollected = true;
                textMeshPro.text = "Collected all mushrooms! \n";
                PrintUpdatedQueue();
                //stop navmesh agent
                agent.isStopped = true;
            }
        }
    }

    public void AddTarget(Vector3 position, GameObject targetObject)
    {
        NavMeshTarget target = new NavMeshTarget(position, targetObject);
        targets.Enqueue(target);
        SortTargetsByShortestPath();
    }

    private void SortTargetsByShortestPath()
    {
        List<NavMeshTarget> sortedTargets = new List<NavMeshTarget>(targets);
        sortedTargets.Sort((a, b) => a.DistanceFromAgent.CompareTo(b.DistanceFromAgent));

        targets.Clear();

        foreach (NavMeshTarget target in sortedTargets)
        {
            targets.Enqueue(target);
        }
    }

    private void CalculateDistancesFromAgent()
    {
        foreach (NavMeshTarget target in targets)
        {
            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(agent.transform.position, target.Position, NavMesh.AllAreas, path))
            {
                target.DistanceFromAgent = GetPathLength(path);
            }
            else
            {
                target.DistanceFromAgent = float.MaxValue;
            }
        }
    }

    private float GetPathLength(NavMeshPath path)
    {
        if (path.corners.Length < 2) return 0;

        float distance = 0;

        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            distance += Vector3.Distance(path.corners[i], path.corners[i + 1]);
        }

        return distance;
    }
    void PrintUpdatedQueue()
    {
        string remaining = "Remaining Queue: ";
        string reached = "Reached Queue: ";
        foreach (NavMeshTarget target in reachedTargets)
        {
            if (target.TargetObject != null)
            {
                reached += target.TargetObject.name + ", ";
            }
        }
        foreach (NavMeshTarget target in targets)
        {
            if (target.TargetObject != null)
            {
                remaining += target.TargetObject.name + ", ";
            }
        }
        textMeshPro.text = reached + "\n" + remaining + "\n";
        Debug.Log(reached);
    }
    private void PlayParticleSystemOnTargetReached(GameObject targetObject)
    {
    if (targetObject != null)
    {
        ParticleSystem particleSystem = targetObject.GetComponent<ParticleSystem>();
        // Use the AudioSource and AudioClip attached to the car
        if (audioSource != null && audioClip != null)
        {
            audioSource.PlayOneShot(audioClip);
        }

        if (particleSystem != null)
        {
            particleSystem.Play();
        }
    }
    }
    private IEnumerator TargetReachedCoroutine(NavMeshTarget target)
    {
        // Make the target object invisible
        Renderer targetRenderer = target.TargetObject.GetComponent<Renderer>();
        if (targetRenderer != null)
        {
            targetRenderer.enabled = false;
        }

        // Instantiate and play the particle system
        GameObject particleSystemInstance = Instantiate(particleSystemPrefab, target.Position, Quaternion.identity);
        ParticleSystem particleSystem = particleSystemInstance.GetComponent<ParticleSystem>();
        particleSystem.Play();

        // Wait for the particle system to finish playing
        yield return new WaitForSeconds(particleSystem.main.duration);

        // Destroy the particle system and target object
        Destroy(particleSystemInstance);
        Destroy(target.TargetObject);
    }

}






