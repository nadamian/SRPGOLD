using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    public GameObject playMenu;
    public GameObject gameMenu;
    public Map map;
    public void OnAttackPressed()
    {
        map.attacking = true;
        map.GetSelectedNode().highlightDefenders();
        DestroyImmediate(playMenu, true);
        playMenu = null;
        map.attackMenuActive = false;
    }
    public void OnWaitPressed()
    {
        Debug.Log("Wait Pressed");
        map.SetSelectedUnit(null);
        DestroyImmediate(playMenu);
        map.attackMenuActive = false;
    }

    public void OnEndTurnPressed()
    {
        Debug.Log("End Turn Pressed");
        map.gameMenuActive = false;
        DestroyGameMenu();
        map.EndTurn();
    }

    public void DestroyGameMenu()
    {
        Destroy(gameMenu);
    }
}
