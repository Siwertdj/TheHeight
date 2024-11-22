using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// CREDIT:
// https://www.youtube.com/watch?v=EwHiMQ3jdHw
public class Waypoints : MonoBehaviour
{
    [Range(0f,2f)]
    [SerializeField] private float waypointSize = 1f;
    
    private void OnDrawGizmos()
    {
        // for all children (wapoints)
        foreach (Transform t in transform)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(t.position, 0.2f);
        }

        Gizmos.color = Color.red;
        for (int i = 0; i < transform.childCount -1; i++)
        {
            Gizmos.DrawLine(transform.GetChild(i).position, transform.GetChild(i+1).position);
        }
    }

    public Transform FindClosestWaypoint(Transform currentPoint)
    {
        Transform output = null;    
        float closestDistance = Single.PositiveInfinity;
        
        foreach (Transform waypoint in transform)
        {
            float thisDistance = Vector3.Distance(currentPoint.position, waypoint.position);
            if (thisDistance < closestDistance)
            {
                output = waypoint;
                closestDistance = thisDistance;
            }
        }

        return output;
    }

    public Transform GetNextWaypoint(Transform currentWaypoint)
    {
        if (currentWaypoint == null)    // if null was passed, return first waypoint.
            return transform.GetChild(0);

        if (currentWaypoint.GetSiblingIndex() < transform.childCount - 1)
        {
            // if this is NOT the last waypoint, pass the next waypoint in line.
            return transform.GetChild(currentWaypoint.GetSiblingIndex() + 1);
        }
        
        // else if not a waypoint in the list.. find the closest one..?
        
        // if it was the last sibling in the list, we return null
        return null;
    }
}
