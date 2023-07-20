using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : ScriptableObject
{
    public int maxUses;
    public int currentUses;
    public int weight;
    public bool canRepair;

    public virtual void Break()
    {
        Destroy(this);
    }
}
