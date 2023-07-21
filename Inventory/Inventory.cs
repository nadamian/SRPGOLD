using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Inventory : ScriptableObject
{
    public int maxUses;
    public int currentUses;
    public int weight;
    public bool canRepair;
    public string itemName;
    public bool usable;
    public bool broken;

    public virtual void Break()
    {
        Destroy(this);
    }
}
