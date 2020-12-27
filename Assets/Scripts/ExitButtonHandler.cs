using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitButtonHandler : MonoBehaviour
{
    public Pipe pipe;


    public void Exit()
    {
        pipe.Stop();
        Application.Quit();
    }
}
