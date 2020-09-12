using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayersManager : MonoBehaviour
{
    [SerializeField] private GameObject PlayPanel;

    [SerializeField] private GameObject PlayerPanel;

    [SerializeField] private GameObject AddButton;
    [SerializeField] private GameObject DelButton;

    [SerializeField] private NamesHolder namesHolder;
    [SerializeField] ScrollRect rect;
    [SerializeField] Slider TurnSlider;
    [SerializeField] Dropdown MapMode;

    private  List<GameObject> PlayersList = new List<GameObject>();

    void Start()
    {
        AddPlayer();
        AddPlayer();
    }


    public void AddPlayer()
    {
        if (PlayersList.Count < 8)
        {
            GameObject obj =  Instantiate(PlayerPanel, rect.content.transform,false) as GameObject;
            PlayersList.Add(obj);
            rect.content.ForceUpdateRectTransforms();
        }
    }

    public void DelPlayer()
    {
        if (PlayersList.Count > 2)
        {
            Destroy(PlayersList[PlayersList.Count - 1]);
            PlayersList.RemoveAt(PlayersList.Count - 1);
        }
    }


    //void SetButtonsPos()
    //{
    //    float dist = -50;
    //    Vector3 vec = PlayersList[PlayersList.Count - 1].transform.localPosition;
    //    Vector3 d = new Vector3(45f, 0f, 0f);
    //    AddButton.transform.localPosition = ((vec - d) + new Vector3(0f, dist, 0f));
    //    DelButton.transform.localPosition = ((vec + d) + new Vector3(0f, dist, 0f));

    //}
    public void InitPlayersNames()
    {
        int LaterStartCounter = 0;
        foreach (GameObject item in PlayersList)
        {
            bool tmp = item.GetComponent<PlayerPanel>().GetData().LaterStart;
            int bot = item.GetComponent<PlayerPanel>().GetData().bot;
            Strategy st = item.GetComponent<PlayerPanel>().GetData().Tactic;

            string str = item.GetComponent<PlayerPanel>().GetData().name;
            if (str != "")
            {
                //Debug.LogError(item.GetComponentInChildren<Dropdown>().value == 0 ? false : true);
                if (tmp)
                {
                    LaterStartCounter++;
                    namesHolder.AddToActiveLater(item.GetComponent<PlayerPanel>().GetData());
                }
                else
                {
                    namesHolder.Add(str, /*item.GetComponentInChildren<Dropdown>().value*/bot == 0 ? false : true,st);
                }
            }
            else
            {
                if (tmp)
                {
                    LaterStartCounter++;
                    namesHolder.AddToActiveLater(item.GetComponent<PlayerPanel>().GetData());
                }
                else
                    namesHolder.Add("Unknown", bot == 0 ? false : true,st);

            }
        }

        namesHolder.SetTurnLimit((int)TurnSlider.value);

        if (MapMode.value == 0)
            NamesHolder.mapMode = global::MapMode.normal;
        else if (MapMode.value == 1) NamesHolder.mapMode = global::MapMode.separate;
        else NamesHolder.mapMode = global::MapMode.shared;

    }


    public Text Warning;
    public InputField ip;
    public InputField user;
    public InputField pass;
    public GameObject LogInPanel;
    public GameObject MenuPanel;
    public Toggle def;

    public void LogIn()
    {
        string txt;
        database db = FindObjectOfType<database>();

        string ans = "";

        if (!def.isOn)
            ans = db.TestConnection(ip.textComponent.text,user.textComponent.text, pass.textComponent.text, out txt);
        else
            ans = db.TestConnection("localhost:7474", "neo4j", "1", out txt);


        if (ans == "")
        {
            Warning.text = "Error. Try again later."; // = txt;
 
        }
        else
        {
            Warning.text =   "ok";
            LogInPanel.SetActive(false);
            MenuPanel.SetActive(true);
        }
    }

}
