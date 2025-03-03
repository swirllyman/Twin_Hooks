using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class StopWatch : MonoBehaviour
{
    float timer, seconds, minutes, hours;

    [SerializeField] Text m_StopwatchText;

    private void Awake()
    {
        timer = 0;
    }

    void StopwatchCalc()
    {
        timer += Time.deltaTime;
        seconds = (int)(timer % 60);
        minutes = (int)(timer / 60);
        hours = (int)(timer / 3600);

        m_StopwatchText.text = hours + ":" + minutes + ":" + seconds;
    }
    private void Update()
    {
        StopwatchCalc();
    }
}
