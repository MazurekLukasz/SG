using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayersManager : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private GameObject PlayPanel;

    [SerializeField] private GameObject PlayerPanel;

    [SerializeField] private GameObject AddButton;
    [SerializeField] private GameObject DelButton;

    [SerializeField] private NamesHolder namesHolder;
    private  List<GameObject> PlayersList = new List<GameObject>();

    private Vector3 dist;

    void Start()
    {
        dist = new Vector3(0f, 220f, 0f);
        AddPlayer();
        AddPlayer();

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddPlayer()
    {
        if (PlayersList.Count < 8)
        {
            PlayersList.Add(Instantiate(PlayerPanel));
            PlayersList[PlayersList.Count - 1].transform.SetParent(PlayPanel.transform, false);
            PlayersList[PlayersList.Count - 1].transform.localPosition = dist;

            SetButtonsPos();
            dist -= new Vector3(0f, 60f, 0f);
        }
    }

    public void DelPlayer()
    {
        if (PlayersList.Count > 2)
        {
            Destroy(PlayersList[PlayersList.Count - 1]);
            PlayersList.RemoveAt(PlayersList.Count - 1);

            SetButtonsPos();
            dist += new Vector3(0f, 60f, 0f);
        }
    }


    void SetButtonsPos()
    {
        float dist = -50;
        Vector3 vec = PlayersList[PlayersList.Count - 1].transform.localPosition;
        Vector3 d = new Vector3(45f, 0f, 0f);
        AddButton.transform.localPosition = ((vec - d) + new Vector3(0f, dist, 0f));
        DelButton.transform.localPosition = ((vec + d) + new Vector3(0f, dist, 0f));

    }
    public void InitPlayersNames()
    {
        foreach (GameObject item in PlayersList)
        {
            string str = item.GetComponentInChildren<InputField>().text;
            namesHolder.Add(str);
        }
    }
}
