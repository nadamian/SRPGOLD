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
    public EnemyController enemyController;

    public Color alliedMoveColor;

    public bool attacking = false;
    public bool attackMenuActive = false;
    public bool gameMenuActive = false;

    public bool menuAction = false;
    public Node[] attackTargets;
    public bool AllyTurn = true;
    public int currentTurn = 1;
    public Unit[] allUnits; //updated each turn to account for reinforcements
    public int factions = 2; //Determines number of distinct factions that will be taking turns 

    private Unit selectedUnit;
    private Node selectedNode;
    private Unit lastUnit;
    private Node lastNode;
    private Unit[] units;
    private Node[] nodes;

    private int displayTurn = 1;

    void Awake()
    {
        enemyController = new EnemyController();
        buttonManager.map = this;
        enemyController.map = this;
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
        units = FindObjectsOfType<Unit>();
        nodes = FindObjectsOfType<Node>();
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
        gameMenuActive = false;
    }

    public void EndPlayerTurn()
    {
        EndTurn();
        enemyController.EnemyTurn(currentTurn);
    }

    public void EndTurn()
    {
        units = FindObjectsOfType<Unit>();
        foreach (Unit unit in units)
        {
            unit.hasMoved = false;
        }
        bool allyTurn = !AllyTurn;
        currentTurn++;
        displayTurn = (int)Mathf.Floor(currentTurn / factions);
        Debug.Log("It is currently turn " + displayTurn.ToString());
        AllyTurn = allyTurn;
    }

    public void ReversePath()
    {
        Debug.Log("Reverse");
        Debug.Log(selectedUnit != null);
        Destroy(buttonManager.playMenu);
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

    public void OpenGameMenu(Node node)
    {
        GameObject menu = Instantiate(gameMenuPrefab, node.transform.position, Quaternion.identity);
        buttonManager.gameMenu = menu;
        gameMenuActive = true;
    }

    public void UpdateOccupation()
    {
        UpdateUnits();
        foreach (Node node in nodes)
        {
            foreach (Unit unit in units)
            {
                if (Vector2.Distance(unit.transform.position, node.transform.position) < 0.5f)
                {
                    node.unit = unit;
                    unit.location = node;
                }
                else if (node.unit == unit)
                {
                    node.unit = null;
                }
            }
        }
    }

    public void DestroyGameMenu()
    {
        gameMenuActive = false;
        buttonManager.DestroyGameMenu();
    }

    public void UpdateUnits()
    {
        units = FindObjectsOfType<Unit>();         
    }
}
