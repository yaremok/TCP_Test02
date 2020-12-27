using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteController : MonoBehaviour
{
    public float cameraDistance;

    private Pipe pipe;


    void Start()
    {
        pipe = GameObject.FindObjectOfType<Pipe>();
    }

    class ObjectWithPosition
    {
        public string name;
        public float x;
        public float y;
    }


    void Update()
    {
        Vector2 pos = new Vector2();
        
        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began || Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                pos = Input.GetTouch(0).position;
            }
        }
        else
        {
            pos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }

        Vector3 position = Camera.main.ScreenToWorldPoint(
            new Vector3(pos.x, pos.y, cameraDistance)
        );

        ObjectWithPosition owp = new ObjectWithPosition();
        owp.name = "Mouse";
        owp.x = position.x;
        owp.y = position.y;

        string mousePos = JsonUtility.ToJson(owp);
        //Debug.Log(">>> " + mousePos);

        if (pipe.ready)
        {
            pipe.pipeSendData(mousePos);
        }


        if (pipe.isIncomingData)
        {
            string data = pipe.pipeReceiveData();
            ObjectWithPosition myOwp = JsonUtility.FromJson<ObjectWithPosition>(data);
            Debug.Log("<<< " + data);
            if (myOwp.name == "Mouse")
            {
                gameObject.transform.position = new Vector3(myOwp.x, myOwp.y, 0);
            }
        }
    }
}
