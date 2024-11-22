using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Mechanics;

public class GoalZone : MonoBehaviour
{
    void Start()
    {
        
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == 6) // 6 = player
        {
            // win
            // schedule event for that..?
        }   
    }

}
