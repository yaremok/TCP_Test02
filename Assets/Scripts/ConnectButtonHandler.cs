using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ConnectButtonHandler : MonoBehaviour
{
    public ConnectionManager connectionManager;


    public void Connect()
    {
        gameObject.GetComponent<Button>().interactable = false;
        connectionManager.Connect();
    }
}
