using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PlayerPanel : MonoBehaviour
{
    public struct PlayerData
    {
        public string name;
        public int bot;
        public bool LaterStart;
        public int ActivationTurn;
        public int Method;
        public Strategy Tactic;
    }

    public GameObject BotPanel;
    public GameObject LaterStartPanel;
    public Dropdown SelectMode;
    public Text PlayerName;

    public Dropdown Method;
    public Text ActivationTurn;
    public Dropdown StrategyDrop;

    void Start()
    {
        SelectMode.onValueChanged.AddListener(delegate { ToggleBotPanel(); });
        ToggleBotPanel();
    }

    public void TooglePlayerStart()
    {
        if (LaterStartPanel.activeInHierarchy)
        {
            LaterStartPanel.SetActive(false);
        }
        else
        {
            LaterStartPanel.SetActive(true);
        }
    }

    void ToggleBotPanel()
    {
        if (SelectMode.value == 0)
        {
            BotPanel.SetActive(false);
        }
        else if (SelectMode.value == 1)
        {
            BotPanel.SetActive(true);
        }
    }

    public PlayerData GetData()
    {
        PlayerData data = new PlayerData();

        if (PlayerName.text == "")
        { PlayerName.text = "Unknown"; }

        data.name = PlayerName.text;

        data.bot = SelectMode.value;

        if (BotPanel.activeInHierarchy)
        {
            data.LaterStart = BotPanel.GetComponent<Toggle>().isOn;

            switch (StrategyDrop.value)
            {
                case 0:
                    data.Tactic = Strategy.casual;
                    break;
                case 1:
                    data.Tactic = Strategy.builder;
                    break;
                case 2:
                    data.Tactic = Strategy.explorer;
                    break;
                case 3:
                    data.Tactic = Strategy.warrior;
                    break;
                default:
                    data.Tactic = Strategy.casual;
                    break;
            }
        }
        else
        {
            data.LaterStart = false;
            data.Tactic = Strategy.casual;
        }

        if (LaterStartPanel.activeInHierarchy)
        {
            data.Method = Method.value;

            int tmp;
            int.TryParse(ActivationTurn.text, out tmp);
            data.ActivationTurn = tmp;
        }
        else
        {
            data.Method = 0;
            data.ActivationTurn = -1;
        }
        return data;
    }
}
