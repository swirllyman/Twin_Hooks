using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Bridge : MonoBehaviour
{
    [SerializeField] AltarHelper[] altarHelpers;
    public CinemachineVirtualCamera vCam;
    // Start is called before the first frame update
    void Start()
    {
        vCam.enabled = false;
        for (int i = 0; i < altarHelpers.Length; i++)
        {
            altarHelpers[i].myAltar.onPoweredUp += MyAltar_onPoweredUp;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            vCam.enabled = !vCam.enabled;
        }
    }

    private void MyAltar_onPoweredUp()
    {
        if (AllPoweredUp())
        {
            StartCoroutine(PlayOpenRoutine());
        }
    }
    
    bool AllPoweredUp()
    {
        for (int i = 0; i < altarHelpers.Length; i++)
        {
            if (!altarHelpers[i].myAltar.poweredUp)
            {
                return false;
            }
        }

        return true;
    }

    IEnumerator PlayOpenRoutine()
    {
        vCam.enabled = true;
        yield return new WaitForSeconds(2.0f);
        for (int i = 0; i < altarHelpers.Length; i++)
        {
            LeanTween.rotate(altarHelpers[i].bridgeTransform.gameObject, new Vector3(0, 0, altarHelpers[i].rotationAmount), altarHelpers[i].rotationSpeed).setEaseOutBounce();
        }
        yield return new WaitForSeconds(2.0f);
        vCam.enabled = false;
    }
}

[System.Serializable]
public struct AltarHelper
{
    public Altar myAltar;
    public Transform bridgeTransform;
    public float rotationAmount;
    public float rotationSpeed;
}