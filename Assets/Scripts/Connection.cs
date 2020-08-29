using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connection : MonoBehaviour
{
    public GameObject Planet1;
    public GameObject Planet2;

    private LineRenderer lineRend;

    public SpaceSystem Sys1;
    public SpaceSystem Sys2;

    public void Init()
    {
        lineRend = GetComponent<LineRenderer>();
        lineRend.positionCount = 2;
        lineRend.SetPosition(0, Planet1.transform.position);
        lineRend.SetPosition(1, Planet2.transform.position);
        //  Sys1 = Planet1.GetComponentInParent<SpaceSystem>();
        //    Sys2 = Planet2.GetComponentInParent<SpaceSystem>();
        Sys1 = Planet1.GetComponent<Location>().ReturnSector();
        Sys2 = Planet2.GetComponent<Location>().ReturnSector();

    }

    public void ActivateConnection(bool activate)
    {
        gameObject.SetActive(activate);
        Planet1.SetActive(activate);
        Planet2.SetActive(activate);
    }

}
