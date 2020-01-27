using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// klasa służy jako pojemnik do przechowywania nazw oraz graczy
public class NamesHolder : MonoBehaviour
{
    // STATYCZNA lista nazw graczy, przez nią w scenie 1 wiemy ile utworzyć graczy oraz jak się nazywają
    private static List<string> PlayerNames = new List<string>();

    // Lista graczy, tutaj są przechowywane wskaźniki do niech
    private List<GameObject> PlayerList = new List<GameObject>();
    
    // Tutaj pole dla Prefab-u gracza, wstawiany w edytorze
    [SerializeField] private GameObject PlayerPref;

    public int PlayerNumber()
    {
        return PlayerNames.Count;
    }

    // dodaj nazwę do listy nazw graczy, funkcja jest potrzebna w menu głównym gry
    public void Add(string str)
    {
        PlayerNames.Add(str);
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
            PlayerList.Add(Instantiate(PlayerPref));
            PlayerList[PlayerList.Count - 1].transform.SetParent(gameObject.transform, false);
            PlayerList[PlayerList.Count - 1].GetComponent<Player>().PlayerNr1 = i;
            // utwórz pionek dla gracza
            // PlayerList[PlayerList.Count - 1].GetComponent<Player>().CreatePawn(i);
            i++;
        }
        Debug.Log(PlayerList.Count);
    }

    // Ustawienie początkowych pozycji dla graczy, jako wartość wejściową potrzebuje Pojemnika na graf, Wywoływana z klasy MASTER
    public void SetStartPositions(GraphContainer graph, int[] tab)
    {
        int i = 0;
        foreach (var player in PlayerList)
        {
            // utwórz pionek dla gracza
            player.GetComponent<Player>().CreatePawn(graph.GetPlanet(tab[i]).transform.position);
            // ---------------------------------- ustaw pionek na odpowiedniej planecie
            player.GetComponent<Player>().SetStartPlanet(graph.GetPlanet(tab[i]));

            i++;
        }
        
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
            int i = PlayerList.IndexOf(current);

            // sprawdzamy czy kolejny indeks nie wychodzi poza listę graczy, jeśli nie to zwracamy następnego gracza z listy 
            if (i + 1 <= PlayerList.Count-1)
            {
                return PlayerList[i+1];
            }
            // jeśli nie to zwracamy pierwszego gracza z listy
        }
        return PlayerList[0];
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
        if (PlayerList.IndexOf(curr) == PlayerList.Count-1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public GameObject ReturnPlayerOnIndex(int i)
    {
        return PlayerList[i];
    }

    public List<GameObject> ReturnPlayerList()
    {
        return PlayerList;
    }
}
