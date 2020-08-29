using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private BattleHandler battle;
    [SerializeField] private GraphContainer graph;

    public bool IsActive { set; get; }
    public bool isInGame { set; get; } = true;

    public int WinPoints { get;set;} = 0;
    private float GoldAmount = 500;
    public bool Bonus = false;
    public int BonusTime = 0;
    public int AdditionalQuestPoints = 0;

    //------------------bocie sprawy
    public bool Bot = false;
    public bool NeedResearch = false;


    public void ActivateBonus(int bon)
    {
        Bonus = true;
        BonusTime = bon *2;
        GoldAmount *= (bon);
        AdditionalQuestPoints = bon / 2;
    }

    public float goldAmount
    { get
        {
            return GoldAmount;
        }
       set
        {
            GoldAmount = GoldAmount + value; 
        }
    }

    // Lista statków - flota
    public List<GameObject> pawnList = new List<GameObject>();
    public List<GameObject> PlanetList { get; private set; }  = new List<GameObject>();
    public List<MotherPlanet> MotherPlanetList = new List<MotherPlanet>();
    public List<SpaceSystem> SystemList { get; private set; } = new List<SpaceSystem>();
    public List<Connection> ConnectionList { get; private set; } = new List<Connection>();

    private int questPoints = 2;
    public int QuestPoints { set { questPoints = value; } get { return questPoints; } }

    public int PlayerNr { get; set; }
    public int TurnToStart = 0;
    public int ActivationMethod = 0;

    private GameObject StartPlanet;
    public GameObject GetStartPlanet { get => MotherPlanetList[0].gameObject; set => StartPlanet = value; }
    
    public void Init()
    {
        graph = FindObjectOfType<GraphContainer>();
        battle = FindObjectOfType<BattleHandler>();
    }

    public void CreatePawn(Vector3 Pos)
    {
        // utworzenie pionka gracza
        GameObject objPrefab = Resources.Load("Prefabs/Pawn1") as GameObject;

        pawnList.Add(Instantiate(objPrefab, new Vector3(Pos.x,Pos.y,1f), new Quaternion(0, 0, 0, 0)));
        pawnList[pawnList.Count-1].GetComponent<Renderer>().material.color = PlayerColor(PlayerNr);
        pawnList[pawnList.Count - 1].GetComponent<Pawn>().Player = this.gameObject;
    }
    public Color MyColor()
    {
        return PlayerColor(PlayerNr);
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

    public void RestartPoints()
    {
        if (BonusTime <= 1)
        {
            Bonus = false;
            AdditionalQuestPoints = 0;
            BonusTime = 0;
        }
        else BonusTime--;

        if (!Bonus)
            questPoints = 2;
        else
        {
            questPoints = 2 + AdditionalQuestPoints;
        }

        foreach (var pawn in pawnList)
        {
            pawn.GetComponent<Pawn>().RestartPoints(AdditionalQuestPoints);
        }
    }

    public void SetPawnOnPlanet(GameObject planet)
    {
        //if (!PlanetList.Contains(planet))
        //{
        //    PlanetList.Add(planet);
        //    planet.GetComponent<Location>().ShowPlanet();
        //}

        pawnList[pawnList.Count - 1].GetComponent<Pawn>().GetComponent<Pawn>().SetCurrentPlanet(planet);
        pawnList[pawnList.Count - 1].GetComponent<Pawn>().transform.SetParent(planet.gameObject.transform, true);

    }

    public void SetStartPlanet(GameObject planet)
    {
        StartPlanet = planet;
        Color PlayerCol = PlayerColor(PlayerNr);
        planet.GetComponent<Planet>().ChangeOwner(PlayerNr);

        pawnList[pawnList.Count - 1].GetComponent<Pawn>().GetComponent<Pawn>().SetCurrentPlanet(planet);
        pawnList[pawnList.Count - 1].GetComponent<Pawn>().transform.SetParent(planet.transform, true);
    }

    public GameObject ReturnPawn()
    {
        return pawnList[pawnList.Count - 1];
    }
}
