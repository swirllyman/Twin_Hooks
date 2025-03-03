using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BugtestWarpManager : MonoBehaviour
{
    public List<Transform> warpPoints = new List<Transform>();
    [SerializeField] GameObject PlayerObject;

    private void Awake()
    {
        
    }

    void WarpToPoint(int i)
    {
        if(warpPoints[i] != null)
        {
            PlayerObject.transform.position = warpPoints[i].transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            WarpToPoint(0);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            WarpToPoint(1);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            WarpToPoint(2);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            WarpToPoint(3);
        if (Input.GetKeyDown(KeyCode.Alpha5))
            WarpToPoint(4);
        if (Input.GetKeyDown(KeyCode.Alpha6))
            WarpToPoint(5);
    }
}
