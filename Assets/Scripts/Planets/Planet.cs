using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : Location
{
    public int OwnerNumber { get; set; } = -1;

    [SerializeField] private GameObject Flag;
    [SerializeField] private GameObject HL;
    [SerializeField] private GameObject Coat;

    SpriteRenderer FlagSpriteRend;

    public string Type { get; set; } = "Basic";

    public void TurnLightOn()
    {
        HL.SetActive(true);
    }
    public void TurnLightOff()
    {
        HL.SetActive(false);
    }

    public void ShowPlanet()
    {
        Coat.SetActive(false);
    }
    public void HidePlanet()
    {
        Coat.SetActive(true);
    }

    public void ChangeOwner(int i)
    {
        NamesHolder namesHolder = FindObjectOfType<NamesHolder>();
        
        if (OwnerNumber != -1)
        {
            if (gameObject.GetComponent<MotherPlanet>())
            {
                Debug.LogError("liczba miast przed usunięciem: " + namesHolder.PlayerList[OwnerNumber].GetComponent<Player>().MotherPlanetList.Count);
                namesHolder.PlayerList[OwnerNumber].GetComponent<Player>().MotherPlanetList.Remove(gameObject.GetComponent<MotherPlanet>());
                Debug.LogError("loose city");
                Debug.LogError("liczba miast: " + namesHolder.PlayerList[OwnerNumber].GetComponent<Player>().MotherPlanetList.Count);
            }
        }

        OwnerNumber = i;
        Debug.LogError("New owner is: "+ OwnerNumber);
        if (this.gameObject.GetComponent<MotherPlanet>())
        {
            if (namesHolder.PlayerList[OwnerNumber])
            {
                Debug.LogError("gracz "+ OwnerNumber+ " istnieje na liście" );
                if (namesHolder.PlayerList[OwnerNumber].GetComponent<Player>())
                {
                    Debug.LogError("player");
                }
            }
            
            namesHolder.PlayerList[OwnerNumber].GetComponent<Player>().MotherPlanetList.Add(gameObject.GetComponent<MotherPlanet>());
            Debug.LogError("dodaje matkę do listy planet gracza o numerze: "+ OwnerNumber);
        }

        Flag.GetComponent<SpriteRenderer>().color = PlayerColor(i);
    }

    public Color PlayerColor(int i)
    {
        switch (i)
        {
            case 0:
                return Color.red;
            case 1:
                return Color.blue;
            case 2:
                return Color.green;
            case 3:
                return Color.yellow;
            case 4:
                return Color.cyan;
            case 5:
                return Color.magenta;
            case 6:
                return Color.white;
            case 7:
                return Color.gray;
            default:
                return Color.black;
        }
    }

    public virtual int ReturnMoney()
    {
        return 50;
    }
}
