using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphContainer : MonoBehaviour
{
    // -----------PREFABS -------------------
    // -----------------------------------
    public GameObject point1;
    public GameObject MotherPlanet;
    public GameObject MinePlanet;

    public GameObject conn1;

    // -----------------------------------------
    // -------------------------------------------
    private static int StartedMineChance = 3;
    private int MineChance = StartedMineChance;


    //private GameObject container;
    public List<GameObject> PlanetsList = new List<GameObject>();
    public List<GameObject> ConnectionsList = new List<GameObject>();

    private int AtomicDistance = 5;
    private int SectorDistance = 20;

    private int MapDifficult = 3;
    private int MDCounter = 1;

    public database db;

    // Tworzenie mapy gry
    public int[] Initialize(int playerNumber)
    {  
        //  odnalezienie w programie obiektu zawierającego klasę "database"
        // i zapisanie tej klasy pod zmienną o nazwie "db" 
        db = gameObject.AddComponent<database>();
        // Wyczyszczenie bazy danych
        db.ClearAll();
         int[] tab = new int[playerNumber];
         // pętla o długości liczby graczy, dla każdego gracza musi zostać stworzony sektor
         for (int i = 0; i < playerNumber; i++)
         {

            Vector3 tmp;
            // utworzenie pierwszego punktu dla danego sektora
            do
            {
                int xRand = Random.Range(-3, 3);
                int yRand = Random.Range(-3, 3);
                tmp = new Vector3(xRand * SectorDistance, yRand * SectorDistance, 0);

            }
            while (PlanetAlreadyHere(tmp));

            GameObject obj = CreateMotherPoint(tmp);
            // dodajemy indeks danej planety do tablicy indeksów
             tab[i] = PlanetsList.IndexOf(obj);
             // utwórz punkt w bazie danych
             db.Create((int)obj.transform.position.x, (int)obj.transform.position.y);
             // Rekurencyjne dodanie nowych planet do danego sektora zaczynając od utworzonej planety matki
             PutPlanetsOnMap(obj);
            MineChance = StartedMineChance;
         }
        return tab;
    }

    public int AddMap(GameObject planet)
    {
        Vector3 tmp;
        // utworzenie pierwszego punktu dla danego sektora
        do
        {
            int xRand = Random.Range(-3, 3);
            int yRand = Random.Range(-3, 3);
            tmp = new Vector3(xRand * SectorDistance, yRand * SectorDistance, 0);

        }
        while (PlanetAlreadyHere(tmp));

        GameObject obj = CreateMotherPoint(tmp);

        // utwórz punkt w bazie danych
        db.Create((int)obj.transform.position.x, (int)obj.transform.position.y);

        // Rekurencyjne dodanie nowych planet do danego sektora zaczynając od utworzonej planety matki
        PutPlanetsOnMap(obj);
        MineChance = StartedMineChance;

        GameObject obj1 = PlanetsList[PlanetsList.IndexOf(obj) + 2];
        GameObject obj2 = PlanetsList[PlanetsList.IndexOf(planet) + 2];
        CreateConnection(obj1,obj2);

        return PlanetsList.IndexOf(obj);
    }


    // utwórz planetę
    private GameObject CreatePoint(Vector3 vec)
    {
        GameObject planet;
        int Rand = Random.Range(0, 10);
        // Create and add a point to the list
        //
        if (Rand <= MineChance)
        {
            planet = MinePlanet;
            MineChance--;
        }
        else
        {
            planet = point1;
        }
        PlanetsList.Add(Instantiate(planet, vec, new Quaternion(0, 0, 0, 0)));
        PlanetsList[PlanetsList.Count - 1].transform.SetParent(gameObject.transform,false);

        // to neo4j database
        db.Create((int)vec.x, (int)vec.y);
        return PlanetsList[PlanetsList.Count - 1];
    }

    // utwórz planetę matkę
    private GameObject CreateMotherPoint(Vector3 vec)
    {
        // Create and add a point to the list
        PlanetsList.Add(Instantiate(MotherPlanet, vec, new Quaternion(0, 0, 0, 0)));
        PlanetsList[PlanetsList.Count - 1].transform.SetParent(gameObject.transform, false);
        // to neo4j database
        db.Create((int)vec.x, (int)vec.y);

        return PlanetsList[PlanetsList.Count - 1];
    }

    // utwórz połączenie pomiędzy dwoma planetami
    private void CreateConnection(GameObject parent, GameObject child)
    {
        Vector3 newPos = (parent.transform.position + child.transform.position) / 2;
        ConnectionsList.Add(Instantiate(conn1, newPos, new Quaternion(0, 0, 0, 0)));
        ConnectionsList[ConnectionsList.Count - 1].transform.SetParent(gameObject.transform, false);
        ConnectionsList[ConnectionsList.Count - 1].GetComponent<Connection>().Planet1 = parent;
        ConnectionsList[ConnectionsList.Count - 1].GetComponent<Connection>().Planet2 = child;
        ConnectionsList[ConnectionsList.Count - 1].GetComponent<Connection>().Init();
        //return PlanetsList[PlanetsList.Count - 1];
    }




    private void PutPlanetsOnMap(GameObject parent)
    {
        MDCounter++;
        Vector3 parentPos = parent.transform.position;

        if (MDCounter <= MapDifficult)
        {
            int NodeNumber = Random.Range(1, 5);
            if (MDCounter == 2)
            {
                NodeNumber = 4;
            }

            for (int i = 1; i <= NodeNumber; i++)
            {
                Vector3 tmp = parentPos + Direction(i);
                if (!PlanetAlreadyHere(tmp))
                {
                    GameObject child = CreatePoint(tmp);
 
                    CreateConnection(parent, child);
                    PutPlanetsOnMap(child);
                }
            }
        }
        MDCounter--;
    }

    private Vector3 Direction(int i)
    {
        Vector3 tmp;
        switch (i)
        {
            case 1:
                tmp = new Vector3(AtomicDistance, 0, 0);
                break;
            case 2:
                tmp = new Vector3(0, AtomicDistance, 0);
                break;
            case 3:
                tmp = new Vector3(0, -1 *AtomicDistance, 0);
                break;
            case 4:
                tmp = new Vector3(-1 * AtomicDistance, 0, 0);
                break;
            default:
                tmp = new Vector3(AtomicDistance, 0, 0);
                break;
        }

        return tmp;
    }

    public GameObject GetPlanet(int index)
    {
        return PlanetsList[index];
    }



    // check if there is a connection beetween this two planets
    public bool CheckConnection(GameObject p1, GameObject p2)
    {
        foreach (var item in ConnectionsList)
        {
            if ((item.GetComponent<Connection>().Planet1 == p1 && item.GetComponent<Connection>().Planet2 == p2) || (item.GetComponent<Connection>().Planet1 == p2 && item.GetComponent<Connection>().Planet2 == p1))
            { return true; }
        }
        return false;
    }

    // check if other planet is already in this place
    private bool PlanetAlreadyHere(Vector3 pos)
    {
        Vector2 tmp = new Vector2(pos.x, pos.y);
        RaycastHit2D hit = Physics2D.Raycast(tmp, Vector2.zero);
        
        if (hit.collider != null && hit.collider.tag == "Planet")
        {
            return true;
        }else
        return false;
    }

    private GameObject ReturnPlanet(Vector3 pos)
    {

        Vector2 tmp = new Vector2(pos.x, pos.y);
        RaycastHit2D hit = Physics2D.Raycast(tmp, Vector2.zero);

        if (hit.collider != null)
        {
            Debug.Log("Planet !");
            return hit.collider.gameObject;
        }
        return null;
    }


    public void ClearLists()
    {
        foreach (GameObject item in PlanetsList)
        {
            Destroy(item);
        }
        foreach (GameObject item in ConnectionsList)
        {
            Destroy(item);
        }
        PlanetsList.Clear();
        ConnectionsList.Clear();
    }


    // --------------------------load game---------------------
    public void RebuildWorld(Save sv)
    {
        for (int i = 0; i < sv.LocationsPoseX.Count; i++)
        {
            Vector3 pos = new Vector3(sv.LocationsPoseX[i], sv.LocationsPoseY[i], 0);
            CreatePoint(pos);
        }

    }
}
