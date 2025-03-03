using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]

public class AddCamToCanvas : MonoBehaviour
{
    void Start()
    {
        //Find and Assign the first camera
        GetComponent<Canvas>().worldCamera = FindObjectOfType<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
