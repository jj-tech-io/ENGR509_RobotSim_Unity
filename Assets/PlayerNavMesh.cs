using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;


public class PlayerNavMesh : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    //tm pro text
    [SerializeField] private TextMeshProUGUI textMeshPro;


    [SerializeField] private Transform[] navPoints;
    private Transform movePositionTransform;
    private int currentTargetIndex = 0;
    [SerializeField] private NavMeshAgent navMeshAgent;
    bool navMeshReady = false;
    bool shortestPathFound = false;

    bool reachTargetCooldown = false;

    private GameObject lastTarget;
    private TMP_InputField inputField;

    private string tickList = "Mushrooms: \n";
    // Start is called before the first frame update
    void Start()
    {
        //nav mesh agent
        navMeshAgent = GetComponent<NavMeshAgent>();

        currentTargetIndex = 0;
        // Clear the text at the start
        SetTextMeshProText("");
        // call couroutine to wait for navmesh to be ready
        StartCoroutine(WaitForNavMesh());
        // call couroutine to wait for shortest path to be found
        StartCoroutine(WaitForShortestPath());
    }
    void GetTargetName()
    {

    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (navMeshReady && shortestPathFound)
        {
            
            MoveBetweenPoints();
        }
        
        

    }
    //On Stop


    void MoveBetweenPoints()
    {
        //update remaining distance
        if (navMeshAgent.remainingDistance <= 20.0f && !reachTargetCooldown)
        {
            GameObject lastTarget = navPoints[currentTargetIndex].gameObject;
            
        
            string tn = lastTarget.name.ToString();
            char lastChar = tn[tn.Length - 1];

            string nt = navPoints[currentTargetIndex].name.ToString();
            tickList += lastChar + " , ";
            SetTextMeshProText(tickList);

            reachTargetCooldown = true;
            Debug.Log("Reached target");
            //call coroutine to wait for reach target cooldown
            StartCoroutine(WaitForReachTarget());

            if (currentTargetIndex >= navPoints.Length - 1 && navPoints.Length > 0)
            {
                Debug.Log("Reached last target");
                currentTargetIndex = 0;
                tickList += " \n , ";
                SetTextMeshProText(tickList);
            }
            else if (navPoints.Length > 0)
            {
                currentTargetIndex++;
            }
            else if (navPoints.Length == 0)
            {
                Debug.Log("No nav points");
                return;
            }
        }
        else
        {
            //check if null
            if (navPoints[currentTargetIndex] == null)
            {
                Debug.Log("Nav point is null");
                return;
            }
            else 
            {
                //set destination
                navMeshAgent.SetDestination(navPoints[currentTargetIndex].position);
            }
            navMeshAgent.SetDestination(navPoints[currentTargetIndex].position);
        }

    }
    Transform[] FindShortestPath(Transform[] navPoints)
    {
        Transform[] shortestPath = new Transform[navPoints.Length];
        float distance = 0;
        float shortestPathDistance = 0;
        int shortestPathIndex = 0;
        for (int i = 0; i < navPoints.Length; i++)
        {
            for (int j = 0; j < navPoints.Length; j++)
            {
                int nextIndex = (i + j) % navPoints.Length;
                distance += Vector3.Distance(navPoints[i].position, navPoints[nextIndex].position);
            }
            if (distance < shortestPathDistance)
            {
                shortestPathDistance = distance;
                shortestPathIndex = i;
            }
        }
        for (int i = 0; i < navPoints.Length; i++)
        {
            shortestPath[i] = navPoints[(shortestPathIndex + i) % navPoints.Length];
        }
        Debug.Log("Shortest path distance: " + shortestPathDistance);
        Debug.Log("Shortest path index: " + shortestPathIndex);
        //next target
        Debug.Log("Next target: " + shortestPath[currentTargetIndex].name);
        return shortestPath;
    }
    //coroutine to wait for shortest path to be found
    IEnumerator WaitForShortestPath()
    {
        shortestPathFound = false;
        while (!shortestPathFound)
        {

            navPoints = FindShortestPath(navPoints);
            if (navPoints.Length > 0)
            {
                shortestPathFound = true;
                Debug.Log("Shortest path found");

            }

        }
        Debug.Log("Shortest path found");

        yield return null;
        
    }

    //coroutine to wait for navmesh to be ready
    IEnumerator WaitForNavMesh()
    {
        navMeshReady = false;
        while (!navMeshReady)
        {
            if (navMeshAgent.isOnNavMesh)
            {
                navMeshReady = true;
            }
        }
        navMeshAgent.SetDestination(navPoints[currentTargetIndex].position);
        Debug.Log("NavMesh is ready");
        yield return null;
    }

    //courotine to wait for reach target cooldown
    IEnumerator WaitForReachTarget()
    {
        while (reachTargetCooldown)
        {
            yield return new WaitForSeconds(2.0f);
            reachTargetCooldown = false;
        }
        Debug.Log("Reach target cooldown finished");
        yield return null;
    }
    public void SetTextMeshProText(string text)
    {
        if (textMeshPro != null)
        {
            textMeshPro.text = text;
            Debug.Log("TextMeshPro text set to: " + text);

        }
        else
        {
            Debug.LogWarning("TextMeshPro not found.");
            return;
        }


    }


}
