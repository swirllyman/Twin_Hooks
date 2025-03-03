using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FeedbackHandler : MonoBehaviour
{
    [SerializeField] List<string> FeedbackStates = new List<string>();
    public Text stateText;
    // Start is called before the first frame update
    void Start()
    {
    } 
    public void ToggleStateText(bool _b)
    {
        stateText.enabled = _b;
    }
    public void StateTextSignal()
    {
        stateText.text = FeedbackStates[0];
    }
    public void StateTextUpdate()
    {
        stateText.text = FeedbackStates[1];
    }
    public void StateTextResolve()
    {
        stateText.text = FeedbackStates[2];
    }

    public void StateText(int i)
    {
        stateText.text = FeedbackStates[i];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
