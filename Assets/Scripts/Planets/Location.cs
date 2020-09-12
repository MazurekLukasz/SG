using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Location : MonoBehaviour
{

    public SpaceSystem ReturnSector()
    {
        return GetComponentInParent<SpaceSystem>();
    }

    public bool IsPawnHere()
    {
        Pawn[] ts = GetComponentsInChildren<Pawn>();

        foreach (Pawn t in ts)
        {
                Debug.Log("Pawn is here");
                return true;
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

    public int ReturnPawnCount()
    {
        Pawn[] pawns = gameObject.GetComponentsInChildren<Pawn>();
        return pawns.Length;
    }
}
