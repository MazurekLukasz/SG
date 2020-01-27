using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotherPlanet : Location
{

    public override string Type { get; set; } = "City";
    public int civilizationLevel { get; set; } = 0;
    private int militaryLevel = 0;

    public int ReturnCivilizationCost()
    {
        int cost;
        switch (civilizationLevel)
        {
            case 0:
                cost = 100;
                break;
            case 1:
                cost = 250;
                break;
            case 2:
                cost = 500;
                break;
            default:
                cost = 0;
                break;

        }
        return cost;
    }
    public void UpgradeCivilization()
    {
        // ulepszenie poziomu cywilizacji planety 
        if (civilizationLevel <= 2)
        {
            civilizationLevel++;
        }

    }
    public bool maxCivilLevel()
    {
        if (civilizationLevel == 3)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public override int ReturnMoney()
    {
            int money;
        switch (civilizationLevel)
        {
            case 0:
                money = 50;
                break;
            case 1:
                money = 100;
                break;
            case 2:
                money = 150;
                break;
            case 3:
                money = 250;
                break;
            default:
                money = 50;
                break;

        }
        return money;
    }
}
