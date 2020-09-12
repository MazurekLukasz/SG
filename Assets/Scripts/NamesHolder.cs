using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerPanel;

public enum MapMode { normal=0,separate=1, shared=2}

// klasa służy jako pojemnik do przechowywania nazw oraz graczy
public class NamesHolder : MonoBehaviour
{
    // STATYCZNA lista nazw graczy, przez nią w scenie 1 wiemy ile utworzyć graczy oraz jak się nazywają
    public static List<string> PlayerNames = new List<string>();
    public static List<bool> PlayerBot = new List<bool>();
    public static List<Strategy> PlayersStrategy = new List<Strategy>();
    static int TurnLimit;

    public static MapMode mapMode;
    public static List<PlayerData> PlayersToActivateLater = new List<PlayerData>();

    // Lista graczy, tutaj są przechowywane wskaźniki do niech
    public List<GameObject> PlayerList = new List<GameObject>();
    public List<GameObject> ActivePlayerList;

    // Tutaj pole dla Prefab-u gracza, wstawiany w edytorze
    [SerializeField] private GameObject PlayerPref;


    public List<string> GetPlayerNames()
    {
        return PlayerNames;
    }

    public void AddToActiveLater(PlayerData item)
    {
        PlayersToActivateLater.Add(item);
    }
    public List<PlayerData> ReturnLaterPlayersList()
    {
        return PlayersToActivateLater;
    }

    public int PlayerNumber()
    {
        return PlayerNames.Count;
    }

    public void SetTurnLimit(int i)
    {
        TurnLimit = i;
    }
    public int GetTurnLimit()
    {
        return TurnLimit;
    }
    // dodaj nazwę do listy nazw graczy, funkcja jest potrzebna w menu głównym gry
    public void Add(string str, bool bot, Strategy st)
    {
        PlayerNames.Add(str);
        PlayerBot.Add(bot);
        PlayersStrategy.Add(st);
    }

    // test, wypisanie nazw graczy
    public void Test()
    {

        foreach (string item in PlayerNames)
        {
            Debug.Log(item);
        }

    }

    // Dla każdej nazwy gracza tworzony jest gracz
    public void InitPlayers()
    {
        int i = 0;
        foreach (string item in PlayerNames)
        {
            AddPlayer(i,PlayerBot[i],PlayersStrategy[i]);
            i++;
        }
    }

    public void AddPlayer(int i, bool bot, Strategy tat)
    {
        GameObject tmp = Instantiate(PlayerPref);
        tmp.transform.SetParent(gameObject.transform, false);
        tmp.GetComponent<Player>().PlayerNr = i;
        tmp.GetComponent<Player>().Bot = bot;
        tmp.GetComponent<Player>().Tactics = tat;
        PlayerList.Add(tmp);
        ActivePlayerList.Add(tmp);
    }

    // Ustawienie początkowych pozycji dla graczy, jako wartość wejściową potrzebuje Pojemnika na graf, Wywoływana z klasy MASTER
    public void SetStartPositions(GraphContainer graph)
    {
        if (mapMode == MapMode.normal || mapMode == MapMode.shared)
        {
            int i = 0;
            foreach (var player in PlayerList)
            {
                // utwórz pionek dla gracza
                player.GetComponent<Player>().Init();
                player.GetComponent<Player>().CreatePawn(graph.GetPlanet(i).transform.position);
                player.GetComponent<Player>().PlanetList.Add(graph.GetPlanet(i));
                //player.GetComponent<Player>().MotherPlanetList.Add(graph.GetPlanet(i).GetComponent<MotherPlanet>());
                player.GetComponent<Player>().SystemList.Add(graph.GetPlanet(i).GetComponentInParent<SpaceSystem>());
                // ---------------------------------- ustaw pionek na odpowiedniej planecie
                player.GetComponent<Player>().SetStartPlanet(graph.GetPlanet(i));

                i++;
            }
        }
        else if(mapMode == MapMode.separate)
        {
            int i = 0;
            foreach (var player in PlayerList)
            {
                // utwórz pionek dla gracza
                player.GetComponent<Player>().Init();
                player.GetComponent<Player>().CreatePawn(graph.StartSectors[i].GetComponent<SpaceSystem>().PlanetsList[0].transform.position);
                player.GetComponent<Player>().PlanetList.Add(graph.StartSectors[i].GetComponent<SpaceSystem>().PlanetsList[0]);
                //player.GetComponent<Player>().MotherPlanetList.Add(graph.GetPlanet(i).GetComponent<MotherPlanet>());
                player.GetComponent<Player>().SystemList.Add(graph.StartSectors[i].GetComponent<SpaceSystem>());
                // ---------------------------------- ustaw pionek na odpowiedniej planecie
                player.GetComponent<Player>().SetStartPlanet(graph.StartSectors[i].GetComponent<SpaceSystem>().PlanetsList[0]);

                i++;
            }
        }

        
    }

    /// <summary>
    /// Set position to new player
    /// </summary>
    /// <param name="sector">Sector in which player should be created</param>
    public void SetNewPlayerPosition(GameObject sector)
    { 
        PlayerList[PlayerList.Count-1].GetComponent<Player>().Init();
        PlayerList[PlayerList.Count - 1].GetComponent<Player>().CreatePawn(sector.GetComponent<SpaceSystem>().PlanetsList[0] .transform.position);
        PlayerList[PlayerList.Count - 1].GetComponent<Player>().PlanetList.Add(sector.GetComponent<SpaceSystem>().PlanetsList[0]);
        PlayerList[PlayerList.Count - 1].GetComponent<Player>().SystemList.Add(sector.GetComponent<SpaceSystem>());
        // ---------------------------------- ustaw pionek na odpowiedniej planecie
        PlayerList[PlayerList.Count - 1].GetComponent<Player>().SetStartPlanet(sector.GetComponent<SpaceSystem>().PlanetsList[0]);

    }

    // Zwróc pierwszego gracza
    //public GameObject ReturnNextPlayer()
    //{
    //    return PlayerList[0];
    //}


    // Zwróc następnego gracza 
    public GameObject ReturnNextPlayer(GameObject current= null)
    {
        // jeśli wpiszemy null to zwraca pierwszego gracza
        if(current != null) // jeśli nie jest null
        {
            // pobieram indeks poprzedniego gracza, którego podajemy funkcji
            int i = ActivePlayerList.IndexOf(current);

            // sprawdzamy czy kolejny indeks nie wychodzi poza listę graczy, jeśli nie to zwracamy następnego gracza z listy 
            if (i + 1 <= ActivePlayerList.Count-1)
            {
                return ActivePlayerList[i+1];
            }
            // jeśli nie to zwracamy pierwszego gracza z listy
        }
        return ActivePlayerList[0];
    }

    public string GetPlayerName(GameObject current)
    {

        // pobieram indeks obecnego gracza, którego podajemy funkcji
        int i = PlayerList.IndexOf(current);
        // zwracamy jego imię
        return PlayerNames[i];


    }

    // Sprawdzamy czy podany GRACZ jest ostatnim z listy
    public bool CheckForLastPlayer(GameObject curr)
    {
        if (ActivePlayerList.IndexOf(curr) == ActivePlayerList.Count - 1)
        {
            return true;
        }
        return false;
    }

    public GameObject ReturnPlayerOnIndex(int i)
    {
        return PlayerList[i];
    }

    public List<GameObject> ReturnPlayerList()
    {
        return PlayerList;
    }

    public void ClearPlayerList()
    {
        PlayerNames.Clear();

        PlayerBot.Clear();
        PlayersStrategy.Clear();
        PlayersToActivateLater.Clear();
}

    public void SortPlayer()
    {
        for (int i = 0; i < PlayerList.Count-1; i++)
        {
            for (int j = 0; j < PlayerList.Count-1; j++)
            {
                if (PlayerList[j].GetComponent<Player>().WinPoints < PlayerList[j + 1].GetComponent<Player>().WinPoints)
                {
                    GameObject tmp = PlayerList[j+1];
                    PlayerList[j+1] = PlayerList[j];
                    PlayerList[j] = tmp;
                }
            }
        }
    }

    public void SortActivePlayer()
    {
        for (int i = 0; i < ActivePlayerList.Count - 1; i++)
        {
            for (int j = 0; j < ActivePlayerList.Count - 1; j++)
            {
                if (ActivePlayerList[j].GetComponent<Player>().WinPoints < ActivePlayerList[j + 1].GetComponent<Player>().WinPoints)
                {
                    GameObject tmp = ActivePlayerList[j + 1];
                    ActivePlayerList[j + 1] = ActivePlayerList[j];
                    ActivePlayerList[j] = tmp;
                }
            }
        }
    }

    public string GetPlayerNameByNumber(int playerNr)
    {
        return PlayerNames[playerNr];
    }
}
