using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connection : MonoBehaviour
{
    private GameObject _Planet1 = null;
    private GameObject _Planet2 = null;
    private LineRenderer lineRend;

    public GameObject Planet1
        {
        get
        {
            return _Planet1;
        }
        set
        {
            if(value != _Planet2)
            _Planet1 = value;
        }
    }

    public GameObject Planet2
    {
        get
        {
            return _Planet2;
        }
        set
        {
            if (value != _Planet1)
                _Planet2 = value;
        }
    }

    public void Init()
    {
        lineRend = GetComponent<LineRenderer>();
        lineRend.positionCount = 2;
        lineRend.SetPosition(0, _Planet1.transform.position);
        lineRend.SetPosition(1, _Planet2.transform.position);
    }

}
