using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonClick : MonoBehaviour
{
    public void debugPrint(string message)
    {
        Debug.Log(message);
    }
    public void jumpScene(string name)
    {
        SceneManager.LoadScene(name);
    }
    public void quit()
    {
        Application.Quit();
    }
}