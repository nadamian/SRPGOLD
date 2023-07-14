using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Global
{
    public static Color alliedMoveColor;
    public static Unit selectedUnit;
    public static Node selectedNode;
    public static bool attacking = false;
    public static bool attackMenuActive = false;
    public static bool menuAction = false;
    public static bool menuBasic = false;
    public static Node[] attackTargets;
    public static bool AllyTurn = true;
    public static int currentTurn = 1;
    public static Unit[] allUnits; //updated each turn to account for reinforcements
    public static List<Node> lastPath;

    public static void resetGlobals()
    {
        selectedUnit = null;
        selectedNode = null;
        attacking = false;
        menuAction = false;
        menuBasic = false;
    }

    public static void EndTurn(Unit[] units)
    {
        allUnits = units;
        foreach (Unit unit in units)
        {
            unit.hasMoved = false;
        }
        bool allyTurn = !Global.AllyTurn;
        if (allyTurn)
        {
            Global.currentTurn++;
        }
        Debug.Log("It is currently turn " + currentTurn);
        Global.AllyTurn = allyTurn;
    }
}
