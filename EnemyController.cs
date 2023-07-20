using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : ScriptableObject
{
    public Map map;
    public void EnemyTurn(int turn)
    {
        int allegiance = turn % map.factions;
        Unit[] units = FindObjectsOfType<Unit>();
        List<Unit> thisTurnUnits = new List<Unit>();
        foreach (Unit unit in units)
        {
            if (unit.allegiance == allegiance)
            {
                thisTurnUnits.Add(unit);
            }
        }
        map.EndTurn();
    }
}
