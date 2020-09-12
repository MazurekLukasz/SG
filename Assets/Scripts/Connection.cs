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

    public void Init(GameObject loc1, GameObject loc2, GameObject Sector1, GameObject Sector2)
    {
        lineRend = GetComponent<LineRenderer>();
        lineRend.positionCount = 2;

        Planet1 = loc1;
        Planet2 = loc2;
        Sys1 = Sector1.GetComponent<SpaceSystem>(); //Planet1.GetComponentInParent<SpaceSystem>();

        Sys2 = Sector2.GetComponent<SpaceSystem>(); //Planet2.GetComponentInParent<SpaceSystem>();

        if (!Sys1.ConnectionsList.Contains(this.gameObject))
            Sys1.ConnectionsList.Add(this.gameObject);
        if (!Sys2.ConnectionsList.Contains(this.gameObject))
            Sys2.ConnectionsList.Add(this.gameObject);
        //Sys1 = Planet1.GetComponent<Location>().ReturnSector();
        //Sys2 = Planet2.GetComponent<Location>().ReturnSector();
        lineRend.SetPosition(0, Planet1.transform.position);
        lineRend.SetPosition(1, Planet2.transform.position);

    }

    public void ActivateConnection(bool activate)
    {
        gameObject.SetActive(activate);
        Planet1.SetActive(activate);
        Planet2.SetActive(activate);
    }

}
