using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionManager : MonoBehaviour
{
    public Text text;
    public Pipe pipe;


    private string lastPipeStatus;


    public void Connect()
    {
        pipe.Connect();
    }


    private void Update() 
    {
        string pipeStatus = pipe.status;
        if (pipeStatus != lastPipeStatus)
        {
            text.text += "Pipe status: " + pipeStatus + "\r\n";
            lastPipeStatus = pipeStatus;
        }


    }
}
