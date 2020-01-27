using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Location : MonoBehaviour
{
    public int OwnerNumber { get => _ownerNumber; set => _ownerNumber = value; }


    private int _ownerNumber = -1;

    [SerializeField] private GameObject Flag;
    [SerializeField] private GameObject HL;
    SpriteRenderer FlagSpriteRend;

    public virtual string Type { get; set; } = "Basic";

    public void TurnLightOn()
    {
        HL.SetActive(true);
    }
    public void TurnLightOff()
    {
        HL.SetActive(false);
    }

    public void ChangeOwner(int i)
    {
        _ownerNumber = i;
        Flag.GetComponent<SpriteRenderer>().color = PlayerColor(i);
    }

    public bool IsPawnHere()
    {
        Transform[] ts = GetComponentsInChildren<Transform>();

        foreach (Transform t in ts)
        {
            if (t.tag == "Pawn")
             {
                Debug.Log("Pawn is here:" + gameObject.name);
                return true;
            }
        }
        return false;
    }

    // sprawdź czy na planecie jest pionek
    public Pawn GetPawn()
    {
        // lista transformacji (pusta), do tej listy weź wszystkie obiekty w przestrzeni planety
        Transform[] ts = GetComponentsInChildren<Transform>();

        //sprawdź wszystkie obiekty z listy 
        foreach (Transform t in ts)
        {
            // jeśli obiekte posiada tag "Pawn" to go zwróć, planeta powinna mieć jednego pionka
            if (t.tag == "Pawn")
            {
                return t.GetComponent<Pawn>();
            }
        }
        return null;
    }
    public GameObject GetPawnObject()
    {
        // lista transformacji (pusta), do tej listy weź wszystkie obiekty w przestrzeni planety
        Transform[] ts = GetComponentsInChildren<Transform>();

        //sprawdź wszystkie obiekty z listy 
        foreach (Transform t in ts)
        {
            // jeśli obiekte posiada tag "Pawn" to go zwróć, planeta powinna mieć jednego pionka
            if (t.tag == "Pawn")
            {
                return t.gameObject;
            }
        }
        return null;
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

    public virtual int ReturnMoney()
    {
        return 50;
    }
}
