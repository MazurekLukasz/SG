using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndGamePlayerPanel : MonoBehaviour
{
    public GameObject DeathInfo;

    public Text DeathTurn;
    //public Image SkullIcon;

    public void PlayerDeath(int turn)
    {
        DeathInfo.SetActive(true);
        DeathTurn.text = "" + turn;
    }
}
