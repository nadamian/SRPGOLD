using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Inventory
{
    public int range;
    public int power;
    public int skill;
    public int level;
    public int kills;
    public bool broken;

    public override void Break()
    {
        broken = true;
    }
}
