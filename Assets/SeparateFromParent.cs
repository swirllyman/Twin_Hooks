using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeparateFromParent : MonoBehaviour
{
    [SerializeField] bool separateOnAwake = true;
    // Start is called before the first frame update
    private void Awake()
    {
        if (separateOnAwake)
            Separate();
    }

    public void Separate()
    {
        if (transform.parent != null)
            transform.parent = null;
    }

}
