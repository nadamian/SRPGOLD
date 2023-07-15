using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour
    //TODO: Undo move when rightclick menu. Implement game menu. Convert globals to map 
{
    [SerializeField] Node nodePrefab;
    [SerializeField] GameObject gameMenuPrefab;

    public Tilemap tileMap;
    public List<Node> lastPath;

    public ButtonManager buttonManager;
    public Color alliedMoveColor;
    public bool attacking = false;
    public bool attackMenuActive = false;
    public bool menuAction = false;
    public bool menuBasic = false;
    public Node[] attackTargets;
    public bool AllyTurn = true;
    public int currentTurn = 1;
    public Unit[] allUnits; //updated each turn to account for reinforcements

    private Unit selectedUnit;
    private Node selectedNode;
    private Unit lastUnit;
    private Node lastNode;

    void Awake()
    {
        buttonManager.map = this;
        Debug.Log(buttonManager.map != null); 
        if (tileMap == null) { tileMap = GetComponent<Tilemap>(); }
        List<Vector3> positions = new List<Vector3>();
        for (int x = tileMap.cellBounds.xMin; x < tileMap.cellBounds.xMax; x++)
        {
            for (int y = tileMap.cellBounds.yMin; y < tileMap.cellBounds.yMax; y++)
            {
                Vector3Int localPosition = new Vector3Int(x, y, (int)tileMap.transform.position.z);
                Vector3 position = tileMap.CellToWorld(localPosition);
                if (tileMap.HasTile(localPosition))
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
        ButtonManager[] managers = FindObjectsOfType<ButtonManager>();
        Debug.Log(managers.Length);
    }
    public void resetGlobals()
    {
        lastUnit = selectedUnit;
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

    public void ReversePath()
    {
        Debug.Log("Reverse");
        Debug.Log(selectedUnit != null);
        selectedNode = selectedUnit.lastLocation;
        selectedUnit.path = lastPath;
        selectedUnit.undo = true;
    }

    public Unit GetSelectedUnit()
    {
        return selectedUnit;
    }

    public Node GetSelectedNode()
    {
        return selectedNode;
    }

    public void SetSelectedUnit(Unit unit)
    {
        lastUnit = selectedUnit;
        selectedUnit = unit;
    }

    public void SetSelectedNode(Node node)
    {
        lastNode = selectedNode;
        selectedNode = node;
    }
}
