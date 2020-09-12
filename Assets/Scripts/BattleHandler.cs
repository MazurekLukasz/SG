using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHandler : MonoBehaviour
{
    [SerializeField] private Image img1;
    [SerializeField] private Image img2;

    [SerializeField] private Text txt1;
    [SerializeField] private Text txt2;

    [SerializeField] private Text PowTxt1;
    [SerializeField] private Text PowTxt2;

    [SerializeField] private Text sum1;
    [SerializeField] private Text sum2;

    [SerializeField] private Text Result;

    [SerializeField] private GameObject Panel;
    public NamesHolder namesHolder;

    [SerializeField] private Text name1;
    [SerializeField] private Text name2;

    [SerializeField] private CameraMovement cameraMovement;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Battle(Pawn pawn1, Pawn pawn2,bool bot)
    {

        
        int p1 = 0; 
        int p2 = 0;

        do
        {
            p1 = Random.Range(1, 7);
            p2 = Random.Range(1, 7);
        }
        while (p1 == p2);

        if (!bot)
        {
            OpenPanel();
            name1.text = namesHolder.GetPlayerName(pawn1.Player);
            name2.text = namesHolder.GetPlayerName(pawn2.Player);
            txt1.text = "" + p1;
            txt2.text = "" + p2;

            PowTxt1.text = "" + pawn1.power;
            PowTxt2.text = "" + pawn2.power;
            img1.color = pawn1.Player.GetComponent<Player>().MyColor();
            img2.color = pawn2.Player.GetComponent<Player>().MyColor();
        }

 
        p1 = p1 + pawn1.power;
        p2 = p2 + pawn2.power;

        if (!bot)
        {
            sum1.text = "" + p1;
            sum2.text = "" + p2;
        }


        if (p1 > p2)
        {// p1 won
            Result.text = namesHolder.GetPlayerName(pawn1.Player) + " won the battle!";

            // jeśli ta planeta jest należy do pionka 2, to przejmmie ją pionek 1
            if (pawn2.ReturnCurrentPlanet().GetComponent<Planet>().OwnerNumber == pawn2.Player.GetComponent<Player>().PlayerNr)
            {
                pawn2.ReturnCurrentPlanet().GetComponent<Planet>().ChangeOwner(pawn1.Player.GetComponent<Player>().PlayerNr);
                Debug.LogError("zmiana właściciela");
            }

            //jeśli gracz ten ma jeszcze planety miasta
            if (pawn2.Player.GetComponent<Player>().MotherPlanetList.Count > 0)
            {
                pawn2.SetCurrentPlanet(pawn2.Player.GetComponent<Player>().GetStartPlanet);
                pawn2.Teleport(pawn2.Player.GetComponent<Player>().GetStartPlanet.transform.position);
            }
            else
            {//jeśli nie ma
                //odpada z gry
                Debug.LogError("odpadł");
                pawn2.Player.GetComponent<Player>().isInGame = false;
                if(!bot) gameLogic.ShowMessage(3f, "Player " + namesHolder.GetPlayerName(pawn2.Player) + " is defeated!");
                pawn2.gameObject.SetActive(false);

                if (gameLogic.currentPlayer == pawn2.Player)
                {
                    gameLogic.EndTurn();
                }
                pawn2.Player.GetComponent<Player>().DeathTurn = (int)gameLogic.turnsCounter;
                namesHolder.ActivePlayerList.Remove(pawn2.Player);
            }
            
            //Destroy(pawn2.gameObject);
        }
        else
        {//p2 won
            Result.text = namesHolder.GetPlayerName(pawn2.Player) + " won the battle!";
            pawn1.SetCurrentPlanet(pawn1.Player.GetComponent<Player>().GetStartPlanet);
            pawn1.Teleport(pawn1.Player.GetComponent<Player>().GetStartPlanet.transform.position);
            //Destroy(pawn1.gameObject);
        }
    }

    private void OpenPanel()
    {
        if (Panel != null)
        {
            Panel.SetActive(true);
        }
        gameLogic.Pause = true;
        cameraMovement.Pause = true;
    }

    public GameLogic gameLogic;
    public void ClosePanel()
    {
        if (Panel != null)
        {
            Panel.SetActive(false);
        }
        gameLogic.Pause = false;
        cameraMovement.Pause = false;
    }
}
