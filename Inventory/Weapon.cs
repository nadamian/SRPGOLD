using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Weapon : Inventory
{
    public int range;
    public int power;
    public int skill;
    public int level;
    public int kills;
    public int type; //0 = sword, 1 = axe, 2 = lance/spear, 3 = bow/sling, 4 = knife, 5 = club/mace, 6 = totem 
    public int techniqueType; //0 = strenght, 1 = dex, 2 = int

    public Weapon(int range, int power, int skill, int level, int kills, int type, string itemName, bool broken, int maxUses, int currentUses, int weight, bool canRepair, bool usable, int techniqueType)
    {
        this.range = range;
        this.power = power;
        this.skill = skill;
        this.level = level;
        this.kills = kills;
        this.type = type;
        this.itemName = itemName;
        this.broken = broken;
        this.maxUses = maxUses;
        this.currentUses = currentUses;
        this.weight = weight;
        this.canRepair = canRepair;
        this.usable = usable;
        this.techniqueType = techniqueType;
    }

    public override void Break()
    {
        broken = true;
    }
}
