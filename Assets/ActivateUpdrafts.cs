using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ActivateUpdrafts : MonoBehaviour
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
        if(other.gameObject.name == "Collider")
        {
            other.gameObject.transform.parent.GetComponent<Updraft>().Activate();
        }
    }
}
