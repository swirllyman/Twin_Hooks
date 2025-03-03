using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuAndDebugControls : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            MyGameManager.RestartScene();
            //SceneManager.LoadScene(SceneManager.GetSceneAt(0).name);
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        if(Input.GetKey(KeyCode.LeftShift))
            Time.timeScale = 0.5f;
        else if (Input.GetKeyUp(KeyCode.LeftShift))
            Time.timeScale = 1f;
            

    }
}
