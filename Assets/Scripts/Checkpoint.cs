using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    
    private Transform respawnPoint;
    public bool isTriggered;

    [SerializeField] private Animator flagAnimator;
    
    void Start()
    {
        respawnPoint = transform.GetChild(0);
        flagAnimator = transform.GetChild(1).GetComponent<Animator>();
        isTriggered = false;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == 6) // 6 = player
        {
            // set new respawnpoint
            if (!isTriggered)
            {
                isTriggered = true; 
                spawnPoint.position = respawnPoint.position;
            }
            
            if(flagAnimator!= null)
                flagAnimator.SetBool("isTriggered", isTriggered);
        }   
    }

}
