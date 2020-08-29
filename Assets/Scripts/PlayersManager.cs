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
        foreach (GameObject item in PlayersList)
        {
            bool tmp = item.GetComponent<PlayerPanel>().GetData().LaterStart;
            int bot = item.GetComponent<PlayerPanel>().GetData().bot;
            

            string str = item.GetComponent<PlayerPanel>().GetData().name;
            if (str != "")
            {
                //Debug.LogError(item.GetComponentInChildren<Dropdown>().value == 0 ? false : true);
                if (tmp)
                {
                    namesHolder.AddToActiveLater(item.GetComponent<PlayerPanel>().GetData());
                }
                else
                {
                    namesHolder.Add(str, /*item.GetComponentInChildren<Dropdown>().value*/bot == 0 ? false : true);
                }
            }
            else
            {
                if (tmp)
                {
                    namesHolder.AddToActiveLater(item.GetComponent<PlayerPanel>().GetData());
                }
                else
                    namesHolder.Add("Unknown", bot == 0 ? false : true);

            }
        }

        namesHolder.SetTurnLimit((int)TurnSlider.value);
    }
}
