using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// CREDIT:
// https://www.youtube.com/watch?v=EwHiMQ3jdHw
public class WaypointMover : MonoBehaviour
{
    private Waypoints waypoints;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float distanceThreshold = 0.1f;
    
    private Transform currentWaypoint;

    private void Start()
    {
        waypoints = FindObjectOfType<Waypoints>();
        if (waypoints == null)
        {
            Debug.Log("No waypoints found");
            Destroy(this.gameObject);
        }
        
        // we pass our current transform to find the current waypoint for us to move towards.
        currentWaypoint = waypoints.FindClosestWaypoint(transform);
    }

    private void Update()
    {
        // Move towards the currently set waypoint to move towards.
        // When we're within a certain margin of it, find the next one. (the lower the margin the smoother the curves are?
        transform.position = Vector3.MoveTowards(transform.position, currentWaypoint.position, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, currentWaypoint.position) < distanceThreshold)
        {
            // Set the next waypoint target
            currentWaypoint = waypoints.GetNextWaypoint(currentWaypoint);
            if (currentWaypoint == null)
            {
                // if its ever null, destroy the guide. this means that the last waypoint was reached.
                Debug.Log("End reached for guide");
                Destroy(this.gameObject);
            }
        }
    }
}
