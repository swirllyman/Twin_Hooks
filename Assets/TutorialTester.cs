using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialTester : MonoBehaviour
{
    public  List <KeyCode> keysToWatch = new List<KeyCode>();
    List<int> keyPressCount = new List<int>();
    public int keyPressesNeeded = 3;
    [TextAreaAttribute]
    public string tutString;
    public TextMeshProUGUI tutText;


    //public string textToDisplay;
    float time; 
    public float timeUntilShowText = 3;
    public float timePressedNeeded = 1f;
    public float timePressedTotal = 0;
    bool isCounting = false;
    AudioSource m_MyAudio;


    // Start is called before the first frame update
    void Awake()
    {
        for(int i = 0; i < keysToWatch.Count; i++)
        {
            keyPressCount.Add(i);
        }
        m_MyAudio = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            isCounting = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isCounting = false;
        }
    }
    void DisplayText()
    {
        tutText.text = tutString;
        isCounting = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isCounting)
        {
            if (keyPressesNeeded > 0)
            {
                for (int j = 0; j < keysToWatch.Count; j++)
                {
                    if (Input.GetKeyDown(keysToWatch[j]))
                        keyPressCount[j]++;
                    if (keyPressCount[j] >= 5)
                    {
                        //DisplayText();
                        isCounting = false;
                    }
                }
                if (isCounting)
                {
                    if (time < timeUntilShowText)
                    {
                        time += Time.deltaTime;
                    }
                    else
                    {
                        DisplayText();
                        time = 0;
                    }
                }
            }
            else if (timePressedNeeded > 0)
            {
                for (int j = 0; j < keysToWatch.Count; j++)
                {
                    if (Input.GetKeyDown(keysToWatch[j]))
                        timePressedTotal += 1;
                    if (timePressedTotal >= timePressedNeeded)
                    {
                        isCounting = false;
                    }
                }
                if (isCounting)
                {
                    if (time < timeUntilShowText)
                    {
                        time += Time.deltaTime;
                    }
                    else
                    {
                        if (m_MyAudio && m_MyAudio.clip)
                            m_MyAudio.Play();
                        DisplayText();
                        time = 0;
                    }
                }
            }
        }
    }
}
