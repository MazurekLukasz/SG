using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// klasa służy jako pojemnik do przechowywania nazw oraz graczy
public class NamesHolder : MonoBehaviour
{
    // STATYCZNA lista nazw graczy, przez nią w scenie 1 wiemy ile utworzyć graczy oraz jak się nazywają
    public static List<string> PlayerNames = new List<string>();
    public static List<bool> PlayerBot = new List<bool>();
    static int TurnLimit;

    // Lista graczy, tutaj są przechowywane wskaźniki do niech
    public List<GameObject> PlayerList = new List<GameObject>();
    public List<GameObject> ActivePlayerList;

    // Tutaj pole dla Prefab-u gracza, wstawiany w edytorze
    [SerializeField] private GameObject PlayerPref;

    public List<string> GetPlayerNames()
    {
        return PlayerNames;
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
    public void Add(string str, bool bot)
    {
        PlayerNames.Add(str);
        PlayerBot.Add(bot);
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
            AddPlayer(i,PlayerBot[i]);
            i++;
        }
    }

    public void AddPlayer(int i, bool bot)
    {
        GameObject tmp = Instantiate(PlayerPref);
        tmp.transform.SetParent(gameObject.transform, false);
        tmp.GetComponent<Player>().PlayerNr = i;
        tmp.GetComponent<Player>().Bot = bot;
        PlayerList.Add(tmp);
        ActivePlayerList.Add(tmp);
    }

    // Ustawienie początkowych pozycji dla graczy, jako wartość wejściową potrzebuje Pojemnika na graf, Wywoływana z klasy MASTER
    public void SetStartPositions(GraphContainer graph)
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
    }

    public void SortPlayer()
    {
        for (int i = 0; i < ActivePlayerList.Count-1; i++)
        {
            for (int j = 0; j < ActivePlayerList.Count-1; j++)
            {
                if (ActivePlayerList[j].GetComponent<Player>().WinPoints < ActivePlayerList[j + 1].GetComponent<Player>().WinPoints)
                {
                    GameObject tmp = ActivePlayerList[j+1];
                    ActivePlayerList[j+1] = ActivePlayerList[j];
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
