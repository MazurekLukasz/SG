using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private BattleHandler battle;
    [SerializeField] private GraphContainer graph;

    public bool IsActive { set; get; }

    private float GoldAmount = 500;

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

    [SerializeField]  public GameObject pawn;
    [SerializeField] private Pawn PawnScript;
    private int questPoints = 2;
    public int QuestPoints { set { questPoints = value; } get { return questPoints; } }

    public int PlayerNr1 { get => PlayerNr; set => PlayerNr = value; }
 

    private int PlayerNr;

    private GameObject StartPlanet;
    public GameObject GetStartPlanet { get => StartPlanet; set => StartPlanet = value; }



    // Start is called before the first frame update
    public void Start()
    {
        // battle = FindObjectOfType<BattleHandler>();
        // graph = FindObjectOfType<GraphContainer>();
 

    }

    public void CreatePawn(Vector3 Pos)
    {
        graph = FindObjectOfType<GraphContainer>();
        battle = FindObjectOfType<BattleHandler>();


        // utworzenie pionka gracza
        GameObject objPrefab = Resources.Load("Pawn1") as GameObject;
        pawn = Instantiate(objPrefab, Pos, new Quaternion(0, 0, 0, 0));

        Color PlayerCol = PlayerColor(PlayerNr);
        pawn.GetComponent<Renderer>().material.color = PlayerCol;
        // skrót do skrpytu pionka
        PawnScript = pawn.GetComponent<Pawn>();
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
    
    void FixedUpdate()
    {
      //  if (IsActive)
       // {
       //     HandleInputs();
      //  }
  
    }


    void HandleInputs()
    {
        if (Input.GetMouseButtonDown(0)) {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            // Przemieszczanie się po planszy
            // jeśli klikam na planetę która nie jest null i mam punnkty zadań,
            if (hit.collider != null && questPoints != 0)
            {
                // jeśli planeta na którą klikam jest różna od tej na któej jestem, sprawdzenie czy planety są połączone
                if (PawnScript.ReturnCurrentPlanet() != hit.collider.gameObject
                    && graph.CheckConnection(PawnScript.ReturnCurrentPlanet(), hit.collider.gameObject))
                {
                    // sprawdź cczy na planecie na którą klikam jest pionek innego gracza
                    if (hit.collider.GetComponent<Location>().IsPawnHere())
                    {
                        Debug.Log("Gracz tu jest");
                        // rozpoczynam walkę pomiedzy aktualnym graczem, a graczem ktory jest na planecie
                        battle.Battle(PawnScript, hit.collider.GetComponent<Location>().GetPawn());
                        hit.collider.GetComponent<Location>().GetPawn().Death();
                    }

                    // Ustaw gracza na nowej planecie 
                    SetPawnOnPlanet(hit.collider.gameObject);

                    // zmniejsz pulę punktów zadań gracza
                    questPoints -= 1;
                }
            }
        }
    }


    public void RestartPoints()
    {
        questPoints = 2;
    }

    public void SetPawnOnPlanet(GameObject planet)
    {
        //pawn.transform.position = planet.gameObject.transform.position;
        pawn.GetComponent<Pawn>().SetCurrentPlanet(planet);
        pawn.transform.SetParent(planet.gameObject.transform, true);
        //pawn.GetComponent<Pawn>().Move();

    }

    public void SetStartPlanet(GameObject planet)
    {
        StartPlanet = planet;
        Color PlayerCol = PlayerColor(PlayerNr);
        planet.GetComponent<Location>().ChangeOwner(PlayerNr);
        //SetPawnOnPlanet(StartPlanet);
        pawn.GetComponent<Pawn>().SetCurrentPlanet(planet);
        pawn.transform.SetParent(planet.transform, true);
    }

    public GameObject ReturnPawn()
    {
        return pawn;
    }
}
