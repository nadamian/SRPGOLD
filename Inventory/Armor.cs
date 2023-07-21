using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Armor : Inventory
{
    public int defense;
    public int bodypart; //0 = head, 1 = chest, 2 = arms, 3 = legs

    public Armor(string itemName, int defense, int bodypart, int maxUses, int currentUses, int weight, bool canRepair=true, bool usable=false, bool broken = false)
    {
        this.defense = defense;
        this.bodypart = bodypart;
        this.itemName = itemName;
        this.broken = broken;
        this.maxUses = maxUses;
        this.currentUses = currentUses;
        this.weight = weight;
        this.canRepair = canRepair;
        this.usable = usable;
    }
}
