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

    [SerializeField] private Text Result;

    [SerializeField] private GameObject Panel;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Battle(Pawn pawn1, Pawn pawn2)
    {
        OpenPanel();
        int p1 = Random.Range(1, 6);
        int p2 = Random.Range(1, 6);
        txt1.text = "" + p1;
        txt2.text = "" + p2;
        p1 = p1 + pawn1.power;
        p2 = p2 + pawn2.power;

        if (p1 > p2)
        {
            Result.text = pawn1.name + " won the battle !";
        }
        else if (p2 == p1)
        {
            Result.text ="Draw !";
        }
        else
        {
            Result.text = pawn2.name + " won the battle !";
        }
    }

    private void OpenPanel()
    {
        if (Panel != null)
        {
            Panel.SetActive(true);
        }
    }

    public void ClosePanel()
    {
        if (Panel != null)
        {
            Panel.SetActive(false);
        }
    }
}
