using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    //Instancing various versions of menus into PM and GM allows us space to create different context-dependant
    //versions of menus later if we so choose 
    public static ButtonManager instance;
    public GameObject PlayMenu;
    public static GameObject PM;
    public GameObject GeneralMenu;
    public static GameObject GM; 

    private void Awake()
    {
        instance = this;
    }
    public static void PlayMenuStart()
    {
        PM = instance.PlayMenu;
        PM.SetActive(true); //Playmenu not showing up after second unit move
    }

    public static void GeneralMenuStart()
    {
        GM = instance.GeneralMenu;
        GM.SetActive(true);
    }
    public void OnAttackPressed()
    {
        Debug.Log("Attack Pressed");
        PM.SetActive(false);
        Global.attacking = true;
        Global.selectedNode.highlightDefenders();
    }
    public void OnWaitPressed()
    {
        Debug.Log("Wait Pressed");
        PM.SetActive(false);
        Global.selectedUnit = null;
    }

    public void OnEndTurnPressed()
    {
        Unit[] units = FindObjectsOfType<Unit>();
        Global.EndTurn(units);
    }
}
