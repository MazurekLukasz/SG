using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinePlanet : Location
{
    public override string Type { get; set; } = "Mine";
    public int MineLevel { get; set; } = 0;

    public int ReturnUpgradeCost()
    {
        int cost;
        switch (MineLevel)
        {
            case 0:
                cost = 150;
                break;
            case 1:
                cost = 300;
                break;
            case 2:
                cost = 600;
                break;
            default:
                cost = 0;
                break;

        }
        return cost;
    }

    public void UpgradeMine()
    {
        if (MineLevel <= 2)
        {
            MineLevel++;
        }

    }
    public bool maxMineLevel()
    {
        if (MineLevel == 3)
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
        switch (MineLevel)
        {
            case 0:
                money = 80;
                break;
            case 1:
                money = 140;
                break;
            case 2:
                money = 200;
                break;
            case 3:
                money = 300;
                break;
            default:
                money = 50;
                break;

        }
        return money;
    }
}
