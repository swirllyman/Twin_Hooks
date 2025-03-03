using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableBlock : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("PhysObj") && 
           CompareTag("Breakable"))
        {
            Break();
        }        
    }

    private void Break()
    {
        GetComponent<Rigidbody2D>().isKinematic = false;
        GameObject.Destroy(gameObject, 0.1f);
    }
}
