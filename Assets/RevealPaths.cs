using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevealPaths : MonoBehaviour
{
    [SerializeField] private float deactivateTime = 0.1f;
    private float deactivateTimer;
    
    private void OnEnable()
    {
        deactivateTimer = deactivateTime;
    }

    void Update()
    {
        if(deactivateTimer < 0)
            this.gameObject.SetActive(false);
        
        deactivateTimer -= Time.deltaTime;
    }

    
    private void OnCollisionEnter2D(Collision2D other)
    {
        // if the colliding object is an updraft.. activate it!
        if(other.gameObject.layer == 10) // is a hidden path..
        {
            other.gameObject.GetComponent<HiddenPath>().Reveal();
        }
    }
    
}
