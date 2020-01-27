using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : MonoBehaviour
{
    [SerializeField] private GameObject HL;
    private GameObject CurrentPlanet;
    [SerializeField] float Speed = 5f;
    private int Power = 1;

    public int power
    {
        get { return Power; }
        set { Power = value; }
    }
    public void IncrementPower()
    {
        Power++;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    public void Move()
    {
        if (CurrentPlanet != null)
        {
            if (transform.position != CurrentPlanet.transform.position)
                transform.position = Vector3.MoveTowards(transform.position, CurrentPlanet.transform.position, Speed * Time.deltaTime);
        }
    }
    public void SetCurrentPlanet(GameObject obj)
    {
        CurrentPlanet = obj;
    }

    public GameObject ReturnCurrentPlanet()
    {
        return CurrentPlanet;
    }

    public void Death()
    {
        Destroy(gameObject);
    }



    public void TurnLightOn()
    {
        HL.SetActive(true);
    }
    public void TurnLightOff()
    {
        HL.SetActive(false);
    }
}
