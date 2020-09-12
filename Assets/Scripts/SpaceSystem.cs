using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceSystem : MonoBehaviour
{
    // -----------PREFABS -------------------
    // -----------------------------------
    public GameObject point1;
    public GameObject MotherPlanet;
    public GameObject MinePlanet;
    public GameObject conn1;
    // -----------------------------------------
    // -------------------------------------------
    public GameObject Circle;
    //private GameObject container;
    public List<GameObject> PlanetsList = new List<GameObject>();
    public List<GameObject> ConnectionsList = new List<GameObject>();
    public List<GameObject> Circles = new List<GameObject>();
    public Circle Cir;

    float Xradius = 5.5f;
    float Yradius = 2f;

    void Initialize()
    {
        Cir = gameObject.GetComponentInChildren<Circle>();
        Cir.Create(40, Xradius, Yradius,0.2f);
    }

    int CityChance = 0;
    int MineChance = 3;
    public void CreateSystem(bool BasicSector = true)
    {
        Initialize();

        if (NamesHolder.mapMode == MapMode.normal)
        {
            CityChance = 0;
            MineChance = 3;
            int PlanetsNumber = Random.Range(5, 11); // od 3  do 8 planet

            for (int i = 1; i < PlanetsNumber; i++)
            {
                if (i == PlanetsNumber - 1)
                    continue;

                int Position = Random.Range(0, 8);

                float xrad = Xradius * ((float)(i + 1) / (float)PlanetsNumber);
                float yrad = Yradius * ((float)(i + 1) / (float)PlanetsNumber);

                float x = Mathf.Sin(Mathf.Deg2Rad * (Position * 40f)) * xrad;
                float y = Mathf.Cos(Mathf.Deg2Rad * (Position * 40f)) * yrad;

                Vector3 pos = new Vector3(x, y, 0f);

                GameObject planet;

                planet = CreatePlanet(pos, i, xrad, yrad, BasicSector);
            }
        }
        else if (NamesHolder.mapMode == MapMode.separate || NamesHolder.mapMode == MapMode.shared)
        {
            for (int i = 1; i < 10; i++)
            {
                if (i == 9)
                    continue;

                int Position = Random.Range(0, 8);
                float xrad = Xradius * ((float)(i+1 ) / (float)10);
                float yrad = Yradius * ((float)(i +1) / (float)10);

                float x = Mathf.Sin(Mathf.Deg2Rad * (Position * 40f)) * xrad;
                float y = Mathf.Cos(Mathf.Deg2Rad * (Position * 40f)) * yrad;

                Vector3 pos = new Vector3(x, y, 0f);

                GameObject planet;

                planet = CreatePlanet(pos, i, xrad, yrad, BasicSector);
            }
        }

    }

    // check if other planet is already in this place
    private bool PlanetIsThere(Vector3 pos)
    {
        Vector2 tmp = new Vector2(pos.x, pos.y);
        RaycastHit2D hit = Physics2D.Raycast(tmp, Vector2.zero);

        if (hit.collider != null && hit.collider.tag == "Planet")
        {
            return true;
        }
        else
        { 
            return false;
        }
    }

    public void HideAllPlanetVisibility()
    {
        foreach (var item in PlanetsList)
        {
            if(item.GetComponent<Planet>())
            item.GetComponent<Planet>().HidePlanet();
        }
    }

    // utwórz planetę
    private GameObject CreatePlanet(Vector3 vec,int i, float x, float y,bool basic)
    {
        GameObject planet = null;
        if (NamesHolder.mapMode == MapMode.normal)
        {
            int Rand = Random.Range(0, 10);
            if (basic)
            {
                if (i == 1)
                {
                    planet = MotherPlanet;
                }
                else if (Rand <= MineChance)
                {
                    planet = MinePlanet;
                    MineChance--;
                }
                else
                {
                    planet = point1;
                }
            }
            else
            {
                if (Rand == CityChance)
                {
                    planet = MotherPlanet;
                    CityChance--;
                }
                else if (Rand <= MineChance)
                {
                    planet = MinePlanet;
                    MineChance--;
                }
                else
                {
                    planet = point1;
                }
            }

            GameObject obj = Instantiate(planet, vec, new Quaternion(0, 0, 0, 0));
            obj.transform.SetParent(gameObject.transform, false);

            GameObject cir = Instantiate(Circle, transform);
            cir.transform.SetParent(gameObject.transform, false);
            cir.GetComponent<Circle>().Create(40, x, y, 0.05f);
            Circles.Add(cir);
            PlanetsList.Add(obj);
            return obj;
        }
        else if (NamesHolder.mapMode == MapMode.separate || NamesHolder.mapMode == MapMode.shared)
        {
            switch (i)
            {
                case 1:
                    planet = MotherPlanet;
                    break;
                case 5:
                case 3:
                    planet = MinePlanet;
                    break;
                default:
                    planet = point1;
                    break;
            }

            GameObject obj = Instantiate(planet, vec, new Quaternion(0, 0, 0, 0));
            obj.transform.SetParent(gameObject.transform, false);

            GameObject cir = Instantiate(Circle, transform);
            cir.transform.SetParent(gameObject.transform, false);
            cir.GetComponent<Circle>().Create(40, x, y, 0.05f);
            Circles.Add(cir);
            PlanetsList.Add(obj);
            return obj;
        }

        return null;
    }

    public SpaceSystem Last;

    public void CheckPath(SpaceSystem last, SpaceSystem Endsys, List<SpaceSystem> done, List<GameObject> doneConn, Player player)
    {
        if (!player.SystemList.Contains(this))
        { return; }

        if (!done.Contains(this))
        { done.Add(this); }
        else
        {
            return;
        }

        Last = last;

        //if (done.Contains(Endsys))
        if(this == Endsys)
        { return; }

        foreach (var conn in ConnectionsList)
        {
            if (!player.ConnectionList.Contains(conn.GetComponent<Connection>()))
            { continue; }

            if (!doneConn.Contains(conn))
            {
                doneConn.Add(conn);
                if (this == conn.GetComponent<Connection>().Sys1)
                {
                    //Debug.LogError("let's go to another space system"); // ----------HERE ----
                    conn.GetComponent<Connection>().Sys2.CheckPath(this, Endsys, done,doneConn,player);
                }
                else if (this == conn.GetComponent<Connection>().Sys2)
                {
                    //Debug.LogError("let's go to another space system 2");
                    conn.GetComponent<Connection>().Sys1.CheckPath(this, Endsys, done, doneConn, player);
                }
            }
        }
    }
}
