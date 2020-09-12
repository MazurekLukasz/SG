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

    public GameObject ConnectionPoint;
    public GameObject Connection;
    public GameObject SpaceSystem;
    // -----------------------------------------
    // -------------------------------------------
    private static int StartedMineChance = 3;
    private int MineChance = StartedMineChance;

    public List<GameObject> SpaceSystemList = new List<GameObject>();
    //private GameObject container;
    //public List<GameObject> PlanetsList = new List<GameObject>();
    public List<GameObject> ConnectionsList = new List<GameObject>();

    public List<GameObject> StartSectors = new List<GameObject>();

    private int XSectorDistance = 15;
    private int YSectorDistance = 8;

    //----------------------------------
    public database db;
    public bool UseDatabase;
    GameLogic gameLogic;

    void Start()
    {
        gameLogic = FindObjectOfType<GameLogic>();
    }

    // Tworzenie mapy gry
    public void Initialize(int playerNumber)
    {
        if (UseDatabase)
        {
            //  odnalezienie w programie obiektu zawierającego klasę "database"
            // i zapisanie tej klasy pod zmienną o nazwie "db" 
            if (gameObject.GetComponent<database>())
            {
                db = GameObject.FindObjectOfType<database>(); //gameObject.GetComponent<database>();
            }
            else
            {
                db = GameObject.FindObjectOfType<database>();

                if (db == null)
                    db = gameObject.AddComponent<database>();
            }
            // Wyczyszczenie bazy danych
            db.ClearAll();
        }

        // pętla o długości liczby graczy, dla każdego gracza musi zostać stworzony sektor
        for (int i = 0; i < playerNumber; i++)
        {

            InitSector(i);
        }
    }

    int Dist= 15;
    /// <summary>
    /// Create sector, return it
    /// </summary>
    /// <returns></returns>
    public GameObject InitSector(int player)
    {
        GameObject obj = null;

        if (NamesHolder.mapMode == MapMode.normal || NamesHolder.mapMode == MapMode.shared)
        {
            Vector3 tmp;
            // utworzenie sektorów dla graczy
            if (NamesHolder.mapMode == MapMode.normal)
            {
                do
                {
                    int xRand = Random.Range(-3, 3);
                    int yRand = Random.Range(-3, 3);
                    tmp = new Vector3(xRand * XSectorDistance, yRand * YSectorDistance, 0);
                }
                while (SpaceSectorAlreadyHere(tmp));
            }
            else
            {
                tmp = new Vector3(0, (Dist * player * YSectorDistance), 0);
            }


            obj = CreateSpaceSystem(tmp);
            obj.GetComponent<SpaceSystem>().CreateSystem();


            if (UseDatabase)
            {
                // utwórz punkt w bazie danych
                db.CreateSpaceSector(obj);
            }
        }
        else if (NamesHolder.mapMode == MapMode.separate)
        {
            GameObject last = null;
            for (int i = 0; i <= 4; i++)
            {
                Vector3 tmp;
                switch (i)
                {
                    case 0:
                    case 4:
                        tmp = new Vector3(0, (Dist * player * YSectorDistance), 0);
                        break;
                    case 1:
                        tmp = new Vector3(0, (Dist * player * YSectorDistance) + YSectorDistance, 0);
                        break;
                    case 2:
                        tmp = new Vector3(0 - XSectorDistance, (Dist * player * YSectorDistance) + YSectorDistance, 0);
                        break;
                    case 3:
                        tmp = new Vector3(0 - XSectorDistance, (Dist * player * YSectorDistance), 0);
                        break;
                    //case 5:
                    //    tmp = new Vector3(0 + XSectorDistance, (Dist * player * YSectorDistance), 0);
                    //    break;
                    //case 6:
                    //    tmp = new Vector3(0 + XSectorDistance, (Dist * player * YSectorDistance) + YSectorDistance, 0);
                    //    break;
                    //case 7:
                    //    tmp = new Vector3(0 , (Dist * player * YSectorDistance) + YSectorDistance, 0);
                    //    break;
                    default:
                        tmp = new Vector3(0, 0, 0);
                        break;
                }


                if (SpaceSectorAlreadyHere(tmp))
                {
                    obj = ReturnSectorAtPos(tmp);
                }
                else
                {
                    obj = CreateSpaceSystem(tmp);
                    obj.GetComponent<SpaceSystem>().CreateSystem();
                }

                if (i == 0) StartSectors.Add(obj);

                if (UseDatabase)
                {
                    // utwórz punkt w bazie danych
                    db.CreateSpaceSector(obj);
                }

                if (last != null)
                {
                    CreateSectorsConnection(last, obj);
                }
                last = obj;
            }
        }

        return obj;
    }

    public void HideAllPlanetVisibility()
    {
        foreach (GameObject item in SpaceSystemList)
        {
            item.GetComponent<SpaceSystem>().HideAllPlanetVisibility();
        }
    }

    public void HideAllSectorVisibility()
    {
        foreach (GameObject item in SpaceSystemList)
        {
            item.SetActive(false);
        }
    }

    public void HideAllConnectionVisibility()
    {
        foreach (GameObject item in ConnectionsList)
        {
            item.GetComponent<Connection>().ActivateConnection(false);
        }
    }

    public bool AddNewSector(Player player)
    {
        bool System = false;
        bool Connection = false;

        SpaceSystem SpaceSys = null;
        SpaceSys = player.SystemList[Random.Range(0, player.SystemList.Count)];
        Vector3 tmp;
        // utworzenie sektorów dla graczy
        do
        {
            int xRand = Random.Range(-1, 2);
            int yRand = Random.Range(-1, 2);
            tmp = new Vector3(SpaceSys.gameObject.transform.position.x + xRand * XSectorDistance, SpaceSys.gameObject.transform.position.y + yRand * YSectorDistance, 0);
        }
        while (tmp == SpaceSys.gameObject.transform.position);

        GameObject obj = null;
        if (SpaceSectorAlreadyHere(tmp))
        {
            obj = ReturnSectorAtPos(tmp);
            System = false;
        }
        else
        {
            obj = CreateSpaceSystem(tmp);
            obj.GetComponent<SpaceSystem>().CreateSystem(false);
            System = true;

            if (UseDatabase)
            {
                db.CreateSpaceSector(obj);
            }
        }

        GameObject conn = null;
        if (obj != null)
        {
            if (!CheckSectorsConnection(SpaceSys, obj.GetComponent<SpaceSystem>()))
            {
                conn = CreateSectorsConnection(SpaceSys.gameObject, obj);
                Connection = true;
            }
            else
            {
                conn = ReturnSectorsConnection(SpaceSys, obj.GetComponent<SpaceSystem>());
                Connection = false;
            }

            if (!player.GetComponent<Player>().SystemList.Contains(obj.GetComponent<SpaceSystem>()))
            {
                player.GetComponent<Player>().SystemList.Add(obj.GetComponent<SpaceSystem>());
                System = true;
            }
        }
        if (conn != null)
        {
            if (!player.GetComponent<Player>().ConnectionList.Contains(conn.GetComponent<Connection>()))
            {
                player.GetComponent<Player>().ConnectionList.Add(conn.GetComponent<Connection>());
                Connection = true;
            }
        }
        return System || Connection;
    }
    //---------------------


    public bool AddNewSectorSeparationMod(Player player)
    {
        bool System = false;
        bool Connection = false;

        // sprawdź planety i połączenia
        foreach (var sys in player.SystemList)
        {
            foreach (var con in sys.ConnectionsList)
            {
                SpaceSystem SystemToCheck = null;

                if (con.GetComponent<Connection>().Sys1 == sys)
                {
                    SystemToCheck = con.GetComponent<Connection>().Sys2;
                }
                else if (con.GetComponent<Connection>().Sys2 == sys)
                {
                    SystemToCheck = con.GetComponent<Connection>().Sys1;
                }


                if (!player.SystemList.Contains(SystemToCheck))
                {
                    if (!System)
                    {
                        player.SystemList.Add(SystemToCheck);
                        System = true;
                        if (!player.ConnectionList.Contains(con.GetComponent<Connection>()))
                        {
                            player.ConnectionList.Add(con.GetComponent<Connection>());
                            Connection = true;
                        }
                        break;
                    }
                }
                else
                {
                    if (!player.ConnectionList.Contains(con.GetComponent<Connection>()))
                    {
                        player.ConnectionList.Add(con.GetComponent<Connection>());
                        Connection = true;
                        System = true;
                        break;
                    }
                }
            }
            if (Connection && System)
            { break; }
        }
        return System || Connection;
    }
    //---------------------


    public void MapGeneration1(int SectorNumber)
    {
        SpaceSystemList.Clear();

        for (int i = 0; i < SectorNumber; i++)
        {
            Vector3 tmp;

            do
            {
                int xRand = Random.Range(-5, 5);
                int yRand = Random.Range(-5, 5);
                tmp = new Vector3(xRand * XSectorDistance, yRand * YSectorDistance, 0);
            }
            while (SpaceSectorAlreadyHere(tmp));

            GameObject obj = CreateSpaceSystem(tmp);

            //Create Planets
            obj.GetComponent<SpaceSystem>().CreateSystem();
        }

        // utworzenie połączeń
        foreach (GameObject Sector in SpaceSystemList)
        {
            int N = Random.Range(1, 3);
            for (int i = 0; i < N; i++)
            {
                GameObject tmp = FindNearestSector(Sector);
                if (tmp != null)
                    CreateSectorsConnection(Sector, tmp);
            }
        }
    }

    public void MapGeneration2(int SectorNumber)
    {
        SpaceSystemList.Clear();
        //Vector3 tmp;
        //do
        //{
        //    int xRand = Random.Range(-5, 5);
        //    int yRand = Random.Range(-5, 5);
        //    tmp = new Vector3(xRand * XSectorDistance, yRand * YSectorDistance, 0);
        //}
        //while (SpaceSectorAlreadyHere(tmp));
        GameObject obj = CreateSpaceSystem(Vector3.zero);
        obj.GetComponent<SpaceSystem>().CreateSystem();

        int i = 1;
        while (i < SectorNumber)
        {
            Vector3 tmp;
            do
            {
                int xRand = Random.Range(-1, 2);
                int yRand = Random.Range(-1, 2);
                tmp = new Vector3(xRand * XSectorDistance, yRand * YSectorDistance, 0);
            }
            while (tmp.x == 0 && tmp.y == 0);

            GameObject newObj;

            if (SpaceSectorAlreadyHere(obj.transform.position + tmp))
            {
                newObj = ReturnSectorAtPos(obj.transform.position + tmp);
            }
            else
            {
                newObj = CreateSpaceSystem(obj.transform.position + tmp);
                newObj.GetComponent<SpaceSystem>().CreateSystem();
                i++;
            }
            //Create Planets
            if (!CheckSectorsConnection(obj.GetComponent<SpaceSystem>(), newObj.GetComponent<SpaceSystem>()))
            {
                CreateSectorsConnection(obj, newObj);
            }
            obj = newObj;

        }
    }

    private GameObject FindNearestSector(GameObject Sector)
    {
        GameObject obj = Sector;

        float dist = float.MaxValue;

        foreach (GameObject item in SpaceSystemList)
        {
            if (item != Sector)
            {
                if (Vector3.Distance(item.transform.position, Sector.transform.position) < dist
                    && !CheckSectorsConnection(item.GetComponent<SpaceSystem>(), Sector.GetComponent<SpaceSystem>()))
                {
                    dist = Vector3.Distance(item.transform.position, Sector.transform.position);
                    obj = item;
                }
            }
        }
        if (obj == Sector)
        {
            return null;
        }
        return obj;
    }

    //// utwórz planetę
    //private GameObject CreatePoint(Vector3 vec)
    //{
    //    GameObject planet;
    //    int Rand = Random.Range(0, 10);
    //    // Create and add a point to the list
    //    //
    //    if (Rand <= MineChance)
    //    {
    //        planet = MinePlanet;
    //        MineChance--;
    //    }
    //    else
    //    {
    //        planet = point1;
    //    }
    //    PlanetsList.Add(Instantiate(planet, vec, new Quaternion(0, 0, 0, 0)));
    //    PlanetsList[PlanetsList.Count - 1].transform.SetParent(gameObject.transform,false);

    //    // to neo4j database
    //    //db.Create((int)vec.x, (int)vec.y);
    //    return PlanetsList[PlanetsList.Count - 1];
    //}

    // utwórz planetę matkę
    private GameObject CreateSpaceSystem(Vector3 vec)
    {
        // Create and add a point to the list
        GameObject tmp = Instantiate(SpaceSystem, vec, new Quaternion(0, 0, 0, 0), gameObject.transform);
        SpaceSystemList.Add(tmp);
        //tmp.transform.SetParent(gameObject.transform, false);

        // to neo4j database
        //db.Create((int)vec.x, (int)vec.y);

        return tmp;
    }

    // utwórz połączenie pomiędzy dwoma planetami
    private GameObject CreateConnection(GameObject loc1, GameObject loc2, GameObject Sector1, GameObject Sector2)
    {
        Vector3 newPos = (loc1.transform.position + loc2.transform.position) / 2;
        GameObject conn = Instantiate(Connection, newPos, new Quaternion(0, 0, 0, 0));
        
        conn.transform.SetParent(gameObject.transform, false);
        //conn.GetComponent<Connection>().Planet1 = loc1;
        //conn.GetComponent<Connection>().Planet2 = loc2;
        conn.GetComponent<Connection>().Init(loc1,loc2, Sector1, Sector2);
        ConnectionsList.Add(conn);
        
        //if(!conn.GetComponent<Connection>().Sys1.ConnectionsList.Contains(conn))
        //    conn.GetComponent<Connection>().Sys1.ConnectionsList.Add(conn);
        //if (!conn.GetComponent<Connection>().Sys2.ConnectionsList.Contains(conn))
        //    conn.GetComponent<Connection>().Sys2.ConnectionsList.Add(conn);

        //loc1.GetComponent<SpaceSystem>().ConnectionsList.Add(conn);
        //loc2.GetComponent<SpaceSystem>().ConnectionsList.Add(conn);
        return conn;
    }

    private GameObject CreateSectorsConnection(GameObject Sector1, GameObject Sector2)
    {
        Vector2 distance = new Vector2(Sector1.transform.position.x - Sector2.transform.position.x, Sector1.transform.position.y - Sector2.transform.position.y);
        distance.Normalize();

        float x = Sector1.transform.position.x - (distance.x * 5.5f);
        float y = Sector1.transform.position.y - distance.y * 2f;

        GameObject conn1 = Instantiate(ConnectionPoint, new Vector3(x, y, 0), new Quaternion(0, 0, 0, 0), Sector1.transform);
        Sector1.GetComponent<SpaceSystem>().PlanetsList.Add(conn1);

        x = Sector2.transform.position.x + (distance.x * 5.5f);
        y = Sector2.transform.position.y + distance.y * 2f;

        GameObject conn2 = Instantiate(ConnectionPoint, new Vector3(x, y, 0), new Quaternion(0, 0, 0, 0), Sector2.transform);
        Sector2.GetComponent<SpaceSystem>().PlanetsList.Add(conn2);

        if (UseDatabase)
        {
            // utwórz punkt w bazie danych
            db.CreateSectorConnection(Sector1, Sector2);
        }
        GameObject conection = CreateConnection(conn1, conn2, Sector1, Sector2);
        //Sector1.GetComponent<SpaceSystem>().ConnectionsList.Add(conection);
        //Sector2.GetComponent<SpaceSystem>().ConnectionsList.Add(conection);
        return conection;
    }


    //private void PutPlanetsOnMap(GameObject parent)
    //{
    //    MDCounter++;
    //    Vector3 parentPos = parent.transform.position;

    //    if (MDCounter <= MapDifficult)
    //    {
    //        int NodeNumber = Random.Range(1, 5);
    //        if (MDCounter == 2)
    //        {
    //            NodeNumber = 4;
    //        }

    //        for (int i = 1; i <= NodeNumber; i++)
    //        {
    //            Vector3 tmp = parentPos + Direction(i);
    //            if (!SpaceSectorAlreadyHere(tmp))
    //            {
    //                GameObject child = CreatePoint(tmp);

    //                CreateConnection(parent, child);
    //                PutPlanetsOnMap(child);
    //            }
    //        }
    //    }
    //    MDCounter--;
    //}

    //private Vector3 Direction(int i)
    //{
    //    Vector3 tmp;
    //    switch (i)
    //    {
    //        case 1:
    //            tmp = new Vector3(AtomicDistance, 0, 0);
    //            break;
    //        case 2:
    //            tmp = new Vector3(0, AtomicDistance, 0);
    //            break;
    //        case 3:
    //            tmp = new Vector3(0, -1 *AtomicDistance, 0);
    //            break;
    //        case 4:
    //            tmp = new Vector3(-1 * AtomicDistance, 0, 0);
    //            break;
    //        default:
    //            tmp = new Vector3(AtomicDistance, 0, 0);
    //            break;
    //    }

    //    return tmp;
    //}

    /// <summary> Method returns first planet from current sector </summary>
    public GameObject GetPlanet(int index)
    {
        return SpaceSystemList[index].GetComponent<SpaceSystem>().PlanetsList[0];
    }

    bool CheckSectorsConnection(SpaceSystem Sector1, SpaceSystem Sector2)
    {
        foreach (var item in ConnectionsList)
        {
            if ((item.GetComponent<Connection>().Sys1 == Sector1 && item.GetComponent<Connection>().Sys2 == Sector2)
                || (item.GetComponent<Connection>().Sys1 == Sector2 && item.GetComponent<Connection>().Sys2 == Sector1))
            {
                return true;
            }
        }
        return false;

    }

    public GameObject ReturnSectorsConnection(SpaceSystem Sector1, SpaceSystem Sector2)
    {
        foreach (var item in ConnectionsList)
        {
            if ((item.GetComponent<Connection>().Sys1 == Sector1 && item.GetComponent<Connection>().Sys2 == Sector2)
                || (item.GetComponent<Connection>().Sys1 == Sector2 && item.GetComponent<Connection>().Sys2 == Sector1))
            {
                return item;
            }
        }
        return null;

    }

    // check if there is a connection beetween this two planets
    public bool CheckConnection(GameObject p1, GameObject p2)
    {
        foreach (var item in ConnectionsList)
        {
            if ((item.GetComponent<Connection>().Planet1 == p1 && item.GetComponent<Connection>().Planet2 == p2) || (item.GetComponent<Connection>().Planet1 == p2 && item.GetComponent<Connection>().Planet2 == p1))
            { return true; }
        }

        if (p1.GetComponentInParent<SpaceSystem>().PlanetsList.Contains(p2))
        {
            return true;
        }

        return false;
    }

    // check if other Space Sector is already in this place
    public bool SpaceSectorAlreadyHere(Vector3 pos)
    {
        string str = db.Query("match (n{x:" + pos.x + ",y:" + pos.y + "}) return n");
        var res = JsonUtility.FromJson<Response>(str);
        if (res.results[0].data.Count > 0)
        {
            return true;

        }
        return false;
        //}
    }

    public GameObject CreateSpaceForNewPlayer()
    {
        string str = db.Query("match (n:SpaceSystem) with min(n.x) as min match (n:SpaceSystem) where n.x = min return n");
        var res = JsonUtility.FromJson<Response>(str);
        str = db.Query("match (n:SpaceSystem) with max(n.x) as max match (n:SpaceSystem) where n.x = max return n");
        var res1 = JsonUtility.FromJson<Response>(str);
        str = db.Query("match (n:SpaceSystem) with min(n.y) as min match (n:SpaceSystem) where n.y = min return n");
        var res2 = JsonUtility.FromJson<Response>(str);
        str = db.Query("match (n:SpaceSystem) with max(n.y) as max match (n:SpaceSystem) where n.y = max return n");
        var res3 = JsonUtility.FromJson<Response>(str);
        // X(left,right), Y(down,top)
        Vector2 X = new Vector2(res.results[0].data[0].graph.nodes[0].properties.x, res1.results[0].data[0].graph.nodes[0].properties.x);
        Vector2 Y = new Vector2(res2.results[0].data[0].graph.nodes[0].properties.y, res3.results[0].data[0].graph.nodes[0].properties.y);
        int val = gameLogic.MeanPlayersWinPoints() / 4;  // 4 - miasto z najwyższym poziomem tyle jest warte
        if (val <= 1) val = 2;

        Vector2 area = new Vector2(Mathf.Abs(X.y - X.x), Mathf.Abs(Y.y - Y.x));// area(width, height)
        Vector3 tmp;
        switch (Random.Range((int)1, (int)5))
        {
            case 1://left,down
                { tmp = new Vector3(X.x - val * XSectorDistance, Y.x - val * YSectorDistance); break; }
            case 2://left, top
                { tmp = new Vector3(X.x - val * XSectorDistance, Y.y + val * YSectorDistance); break; }
            case 3://right, down
                { tmp = new Vector3(X.y + val * XSectorDistance, Y.x - val * YSectorDistance); break; }
            case 4://right, top
                { tmp = new Vector3(X.y + val * XSectorDistance, Y.y + val * YSectorDistance); break; }
            default://left,down
                { tmp = new Vector3(X.x - val * XSectorDistance, Y.x - val * YSectorDistance); break; }
        }
        GameObject obj = CreateSpaceSystem(tmp);
        obj.GetComponent<SpaceSystem>().CreateSystem();
        if (UseDatabase) db.CreateSpaceSector(obj);
        return obj;
    }

    public void CreateSpaceForStrongPlayer()
    {
        string str = db.Query("match (n:SpaceSystem) with min(n.x) as min match (n:SpaceSystem) where n.x = min return n");
        var res = JsonUtility.FromJson<Response>(str);
        str = db.Query("match (n:SpaceSystem) with max(n.x) as max match (n:SpaceSystem) where n.x = max return n");
        var res1 = JsonUtility.FromJson<Response>(str);
        str = db.Query("match (n:SpaceSystem) with min(n.y) as min match (n:SpaceSystem) where n.y = min return n");
        var res2 = JsonUtility.FromJson<Response>(str);
        str = db.Query("match (n:SpaceSystem) with max(n.y) as max match (n:SpaceSystem) where n.y = max return n");
        var res3 = JsonUtility.FromJson<Response>(str);

        Vector2 X = new Vector2(res.results[0].data[0].graph.nodes[0].properties.x, res1.results[0].data[0].graph.nodes[0].properties.x);
        Vector2 Y = new Vector2(res2.results[0].data[0].graph.nodes[0].properties.y, res3.results[0].data[0].graph.nodes[0].properties.y);
        Vector3 tmp;
        switch (Random.Range((int)1, (int)5))
        {
            case 1://left,
                { tmp = new Vector3(X.x - XSectorDistance, Random.Range(-3, 3) * YSectorDistance); break; }
            case 2://top
                { tmp = new Vector3(Random.Range(-3, 3) * XSectorDistance, Y.y + YSectorDistance); break; }
            case 3:// down
                { tmp = new Vector3(Random.Range(-3, 3) * XSectorDistance, Y.x - YSectorDistance); break; }
            case 4://right
                { tmp = new Vector3(X.y + XSectorDistance, Random.Range(-3, 3) * YSectorDistance); break; }
            default://left,down
                { tmp = new Vector3(X.x - XSectorDistance, Random.Range(-3, 3) * YSectorDistance); break; }
        }
        
        int Points = gameLogic.MeanPlayersWinPoints();
        //int Points = gameLogic.MedianaWinPoints();

        Debug.LogError(Points);
        GameObject obj = CreateSpaceSystem(tmp);
        obj.GetComponent<SpaceSystem>().CreateSystem();
        if (UseDatabase) db.CreateSpaceSector(obj);

        gameLogic.namesHolder.SetNewPlayerPosition(obj);
        int WinPoints = 1;

        Player player = gameLogic.namesHolder.PlayerList[gameLogic.namesHolder.PlayerList.Count - 1].GetComponent<Player>();
        SpaceSystem NewSystem = player.SystemList[0];
        if (Points > 1) // jeśli punkty graczy są większe niż 1 to....
        {
            if (WinPoints >= Points) { return; }

            while (Points - WinPoints > 0)
            {
                foreach (GameObject planet in NewSystem.PlanetsList)
                {
                    if (WinPoints >= Points)
                    { break; }

                    if (Points - WinPoints >= 0)
                    {
                        if(planet.GetComponent<Planet>())
                        if (planet.GetComponent<Planet>().OwnerNumber == -1)
                        {
                            if (planet.GetComponent<Planet>().OwnerNumber != player.PlayerNr)
                            {
                                planet.GetComponent<Planet>().ChangeOwner(player.PlayerNr);

                                if (!player.PlanetList.Contains(planet))
                                {
                                    player.PlanetList.Add(planet);
                                }

                                WinPoints++;
                            }
                        }


                        if (planet.GetComponent<MotherPlanet>())
                        {
                            while (planet.GetComponent<MotherPlanet>().civilizationLevel < 3)// maxymalny poziom miasta to 3
                            {
                                if (WinPoints >= Points) { break; }
                                planet.GetComponent<MotherPlanet>().civilizationLevel++;
                                WinPoints++;
                            }
                        }
                        else if (planet.GetComponent<MinePlanet>())
                        {
                            while (planet.GetComponent<MinePlanet>().MineLevel < 3)// maxymalny poziom miasta to 3
                            {
                                if (WinPoints >= Points) { break; }
                                planet.GetComponent<MinePlanet>().MineLevel++;
                                WinPoints++;
                            }
                        }
                        else if (planet.GetComponent<Planet>())
                        {
                            if (WinPoints >= Points) { break; }
                        }

                    }
                }
                //----------------stwórz nowy sektor dla gracza--------------------
                if (WinPoints >= Points)
                { break; }
                while (true)
                {
                    if (AddNewSector(player))
                    {
                        NewSystem = player.SystemList[player.SystemList.Count - 1];
                        break;
                    }
                }//----------------------------------------------------
            }
        }
    }

    private GameObject ReturnPlanet(Vector3 pos)
    {

        Vector2 tmp = new Vector2(pos.x, pos.y);
        RaycastHit2D hit = Physics2D.Raycast(tmp, Vector2.zero);

        if (hit.collider != null)
        {
            return hit.collider.gameObject;
        }
        return null;
    }

    private GameObject ReturnSectorAtPos(Vector3 pos)
    {

        //Vector2 tmp = new Vector2(pos.x, pos.y);
        //RaycastHit2D hit = Physics2D.Raycast(tmp, Vector2.zero);

        //if (hit.collider != null && hit.collider.tag == "SpaceSector")
        //{
        //    return hit.collider.gameObject;
        //}
        foreach (var item in SpaceSystemList)
        {
            if (item.transform.position == pos)
            {
                return item;
            }
        }
        return null;
    }


    //public void ClearLists()
    //{
    //    foreach (GameObject item in PlanetsList)
    //    {
    //        Destroy(item);
    //    }
    //    foreach (GameObject item in ConnectionsList)
    //    {
    //        Destroy(item);
    //    }
    //    PlanetsList.Clear();
    //    ConnectionsList.Clear();
    //}


    //// --------------------------load game---------------------
    //public void RebuildWorld(Save sv)
    //{
    //    for (int i = 0; i < sv.LocationsPoseX.Count; i++)
    //    {
    //        Vector3 pos = new Vector3(sv.LocationsPoseX[i], sv.LocationsPoseY[i], 0);
    //        CreatePoint(pos);
    //    }

    //}

}
