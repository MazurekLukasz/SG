using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : MonoBehaviour
{
    [SerializeField] private GameObject HL;
    [SerializeField] private GameObject CurrentPlanet;
    [SerializeField] float Speed = 5f;
    public GameObject Player { get; set; }
    public int power = 1;

    private int ActionPoint;
    [SerializeField] GameObject MovementPrefab;
    [SerializeField] List<GameObject> VisualPoints = new List<GameObject>();
    Vector3 DistanceOffset;

    public List<GameObject> TargetPath = new List<GameObject>();

    void Start()
    {
        RestartPoints(Player.GetComponent<Player>().AdditionalQuestPoints);
        ChangeDistanceOffset(1);
        //GameObject[] Points = GetComponentsInChildren<GameObject>();
        //foreach (var item in Points)
        //{
        //    if (item.tag == "MovePoints")
        //        VisualPoints.Add(item);
        //}
    }

    void Update()
    {
        Move();
    }

    public void Move()
    {
        if (CurrentPlanet != null)
        {
            if (transform.position != CurrentPlanet.transform.position + DistanceOffset)
                transform.position = Vector3.MoveTowards(transform.position, CurrentPlanet.transform.position + DistanceOffset, Speed * Time.deltaTime);
        }
    }

    public int ReturnUpgradeCost()
    {
        int cost = 0;

        switch (power)
        {
            case 1:
                {
                    cost = 250;
                    break;
                }
            case 2:
                {
                    cost = 300;
                    break;
                }
            case 3:
                {
                    cost = 350;
                    break;
                }
            case 4:
                {
                    cost = 450;
                    break;
                }
        }
        return cost;
    }

    public bool CheckIfPawnMove()
    {
        if (transform.position != CurrentPlanet.transform.position + DistanceOffset)
        {
            return true;
        }
        return false;
    }

    public void ChangeDistanceOffset(int i)
    {
        float range = 0.4f;
        float x, y;

        float angle = i * 360f/8f;

        x = Mathf.Sin(Mathf.Deg2Rad * angle) * range;
        y = Mathf.Cos(Mathf.Deg2Rad * angle) * range;

        DistanceOffset = new Vector3(x, y, 0f);
    }

    public void Teleport(Vector3 newPos)
    {
        this.gameObject.transform.position = newPos;
    }

    public void SetCurrentPlanet(GameObject obj)
    {
        CurrentPlanet = obj;
    }

    public GameObject ReturnCurrentPlanet()
    {
        return CurrentPlanet;
    }

    public int GetActionPoints()
    {
        return ActionPoint;
    }
    int activeMoveBlocks;
    public void DecreaseActionPoints()
    {
        if (ActionPoint > 0)
        {
            

            int i = 0;

            if (ActionPoint <= 3 && activeMoveBlocks > 0)
            {
                foreach (var item in VisualPoints)
                {
                    if (item.GetComponentInChildren<SpriteRenderer>().gameObject.activeSelf == true)
                    {
                        i++;
                    }
                }
                VisualPoints[i - 1].GetComponentInChildren<SpriteRenderer>().gameObject.SetActive(false);
                activeMoveBlocks--;
            }
            ActionPoint--;
        }
    }

    public void TurnLightOn()
    {
        HL.SetActive(true);
    }
    public void TurnLightOff()
    {
        HL.SetActive(false);
    }

    public void RestartPoints(int bon)
    {
        activeMoveBlocks = 3;
        ActionPoint = VisualPoints.Count + bon;

        foreach (var item in VisualPoints)
        {
            item.GetComponentInChildren<SpriteRenderer>().gameObject.SetActive(true);
        }
    }
}
