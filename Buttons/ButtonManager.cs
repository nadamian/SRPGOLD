using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    public static GameObject playMenu;
    public static GameObject gameMenu;
    public void OnAttackPressed()
    {
        Debug.Log("Attack Pressed");
        Global.attacking = true;
        Global.selectedNode.highlightDefenders();
        Destroy(playMenu);
        playMenu = null;
        Global.attackMenuActive = false;
    }
    public void OnWaitPressed()
    {
        Debug.Log("Wait Pressed");
        Global.selectedUnit = null;
        Destroy(playMenu);
        Global.attackMenuActive = false;
    }

    public void OnEndTurnPressed()
    {
        Unit[] units = FindObjectsOfType<Unit>();
        Global.EndTurn(units);
    }
}
