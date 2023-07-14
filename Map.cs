using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour
    //TODO: Undo move when rightclick menu. Implement game menu. Convert globals to map 
{
    [SerializeField] Node nodePrefab;
    [SerializeField] GameObject gameMenuPrefab; 
    public Tilemap map;
    public List<Node> lastPath;

    public Color alliedMoveColor;
    public Unit selectedUnit;
    public Node selectedNode;
    public bool attacking = false;
    public bool attackMenuActive = false;
    public bool menuAction = false;
    public bool menuBasic = false;
    public Node[] attackTargets;
    public bool AllyTurn = true;
    public int currentTurn = 1;
    public Unit[] allUnits; //updated each turn to account for reinforcements

    void Awake()
    {
        if (map == null) { map = GetComponent<Tilemap>(); }
        List<Vector3> positions = new List<Vector3>();
        for (int x = map.cellBounds.xMin; x < map.cellBounds.xMax; x++)
        {
            for (int y = map.cellBounds.yMin; y < map.cellBounds.yMax; y++)
            {
                Vector3Int localPosition = new Vector3Int(x, y, (int)map.transform.position.z);
                Vector3 position = map.CellToWorld(localPosition);
                if (map.HasTile(localPosition))
                {
                    Instantiate(nodePrefab, position, Quaternion.identity);
                }
            }
        }
        Unit[] units = FindObjectsOfType<Unit>();
        Node[] nodes = FindObjectsOfType<Node>();
        foreach (Node node in nodes)
        {
            node.map = this; 
            foreach (Node secondNode in nodes)
            {
                node.CheckAdjacent(secondNode);
            }

            foreach (Unit unit in units)
            {
                if (Vector2.Distance(unit.transform.position, node.transform.position) < 0.5f)
                {
                    node.unit = unit;
                }
            }
        }
    }
    public void resetGlobals()
    {
        selectedUnit = null;
        selectedNode = null;
        attacking = false;
        menuAction = false;
        menuBasic = false;
    }

    public void EndTurn(Unit[] units)
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
