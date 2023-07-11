using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    public static void EnemyTurn()
    {
        Unit[] units = FindObjectsOfType<Unit>();
        Global.EndTurn(units);
    }
}
