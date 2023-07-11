using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    [Header("UI Variables")]
    public Color hoverColor;
    public Color attackColor;
    public static int nodeDist = 1;
    public bool isSelected;
    public bool isOccupied;

    [Header("Adjacent Nodes")]
    public Node north;
    public Node south;
    public Node east;
    public Node west;

    public Unit unit;
    public int terrainPenalty = 1;

    private SpriteRenderer rend;
    private Color startColor;
    private List<Node> unitMoves;

    void Start()
    {
        rend = GetComponent<SpriteRenderer>();
        startColor = rend.color;

        isSelected = false;
        isOccupied = unit != null;
        if (isOccupied)
        {
            unit.location = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        isOccupied = unit != null;
    }

    private void OnMouseEnter()
    {
        Debug.Log("Mouse enter");
        if(Global.selectedUnit == null)
        {
            upOpacity();
        }
    }

    private void OnMouseExit()
    {
        if(Global.selectedUnit == null)
        {
            resetColor();
        }
    }

    private void OnMouseDown()
    {
        if (Global.currentTurn % 2 == 0)
        {
            return;
        }
        if (Global.attacking)
        {
            if (unit != null)
            {
                MouseDownChooseTarget();
                Global.attacking = false;
            }
            return;
        }
        if (Global.selectedUnit == null)
        {
            MouseDownNoUnitSelected();
            Global.selectedNode = this;
        }

        else if(Global.selectedUnit != null /*and unit is on active player team*/)
        {
            MouseDownAllySelected();
        }

        else
        {
            MouseDownEnemySelected();
        }
    }

    //OnMouseDownHelpers
    private void MouseDownNoUnitSelected()
    {
        Debug.Log("Mouse Down, Selecting unit");
        if (!isOccupied || unit.hasMoved)
        {
            Debug.Log("No Unit");
            Global.menuBasic = true; 
            return;
        }
        if (unit.allegiance != 0 || unit.hasMoved)
        {
            Debug.Log("Enemy Unit or already moved ally: Implement look at stats");
        }
        else
        {
            Global.selectedUnit = unit;
            Debug.Log("Unit selected");
            FindMoves(this, unit.moves, unit.Movement);
            Debug.Log(unit.moves.Count.ToString());
            foreach (Node move in unit.moves)
            {
                move.upOpacity();
            }
        }
    }

    private void MouseDownAllySelected()
    {
        Debug.Log("Unit already selected");
        if (unit == null && !Global.selectedUnit.moves.Contains(this))
        {
            Debug.Log("Not in range");
            return;
        }
        if (unit == Global.selectedUnit)
        {
            Global.selectedNode.resetColor();
            foreach (Node move in unit.moves)
            {
                move.resetColor();
            }
            Global.selectedUnit = null;
        }
        if (unit == null && Global.selectedUnit.moves.Contains(this))
        {
            Global.selectedNode.unit = null;
            Global.selectedNode.resetColor();
            List<Node> path = AStar(Global.selectedNode, this);
            Global.selectedNode = this;
            Global.selectedUnit.path = path;
            return;
        }
    }

    private void MouseDownChooseTarget()
    {
        //chooses an enemy target from occupied squares within unit attack range.
        Unit attacker = Global.selectedUnit;
        Unit defender = this.unit;
        Debug.Log("attacking " + this.name);
        Global.selectedNode.resetDefenders();
        attacker.Attack(defender);
    }
    private void MouseDownEnemySelected()
    {
        //checks enemy unit stats and range.
    }

    //Miscilaneous helpers
    public void resetColor()
    {
        rend.color = startColor;
    }

    public void attackColorChange()
    {
        Debug.Log(Global.selectedUnit == null);
        rend.color = attackColor;
        upOpacity();
    }

    public void upOpacity()
    {
        rend.color = new Color(rend.color.r, rend.color.g, rend.color.b, .25f);
    }

    public void highlightDefenders()
    {
        if (north.unit != null && north.unit.allegiance > 0) north.attackColorChange();
        if (south.unit != null && south.unit.allegiance > 0) south.attackColorChange();
        if (east.unit != null && east.unit.allegiance > 0) east.attackColorChange();
        if (west.unit != null && west.unit.allegiance > 0) west.attackColorChange();
    }

    public void resetDefenders()
    {
        north.resetColor();
        south.resetColor();
        east.resetColor();
        west.resetColor();
    }
    /*Checks and assigns adjacent nodes and creates node map. Won't have to account for changes along y axis if adding hills
     * and ledges because both can be handled in movement penalty*/
    public void CheckAdjacent(Node node)
    {
        Vector2 dist = this.transform.position - node.transform.position;
        if (Mathf.Approximately(dist.x, nodeDist) && Mathf.Approximately(dist.y, 0f))
        {
            west = node;
        }
        if (Mathf.Approximately(dist.x, -nodeDist) && Mathf.Approximately(dist.y, 0f))
        {
            east = node;
        }
        if (Mathf.Approximately(dist.y, nodeDist) && Mathf.Approximately(dist.x, 0f))
        {
            south = node;
        }
        if (Mathf.Approximately(dist.y, -nodeDist) && Mathf.Approximately(dist.x, 0f))
        {
            north = node;
        }
    }

    //Recursively finds all possible moves for a unit to make from starting node.
    private void FindMoves(Node node, List<Node> moves, int movement)
    {
        if (node.unit != null && node.unit.allegiance > 0)
        {
            return;
        }
        if (!moves.Contains(node))
        {
            moves.Add(node);
        }
        if (movement <= 0 || node == null)
        {
            if (node.unit != null)
            {
                moves.Remove(node);
            }
            return;
        }
        if (node.north != null)
        {
            FindMoves(node.north, moves, movement - node.terrainPenalty);
        }
        if (node.south != null)
        {
            FindMoves(node.south, moves, movement - node.terrainPenalty);
        }
        if (node.east != null)
        {
            FindMoves(node.east, moves, movement - node.terrainPenalty);
        }
        if (node.west != null)
        {
            FindMoves(node.west, moves, movement - node.terrainPenalty);
        }
        if (node.unit != null)
        {
            moves.Remove(node);
        }
    }

    //Implementation of A* pathfinding algorithm
    private List<Node> AStar(Node start, Node goal)
    {
        List<Node> openSet = new List<Node>();
        openSet.Add(start);
        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
        Dictionary<Node, float> gScore = new Dictionary<Node, float>();
        gScore.Add(start, 0f);
        Dictionary<Node, float> fscore = new Dictionary<Node, float>();
        fscore.Add(start, Distance(start, goal));
        while (openSet.Count > 0)
        {
            //Current = node with the lowest fscore
            Node current = openSet[0];
            foreach (Node node in openSet)
            {
                if (fscore[node] < fscore[current])
                {
                    current = node;
                }
            }
            if (current == goal)
            {
                return reconstructPath(cameFrom, current);
            }
            openSet.Remove(current);
            List<Node> neighbors = new List<Node> { current.north, current.south, current.east, current.west };
            foreach (Node neighbor in neighbors)
            {
                if (neighbor != null && (neighbor.unit == null || neighbor.unit.allegiance == Global.selectedUnit.allegiance))
                {
                    float tentative_gScore = gScore[current] + current.terrainPenalty;
                    if (!gScore.ContainsKey(neighbor))
                    {
                        gScore[neighbor] = Mathf.Infinity;
                    }
                    if (tentative_gScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentative_gScore;
                        fscore[neighbor] = tentative_gScore + Distance(neighbor, goal);
                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }
        }
        return null;
    }

    //Helper method for A* algorithm
    private List<Node> reconstructPath(Dictionary<Node, Node> cameFrom, Node current)
    {
        List<Node> path = new List<Node> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }
        return path;
    }

    private float Distance(Node start, Node goal)
    {
        Vector2 origin = new Vector2(start.transform.position.x, start.transform.position.y);
        Vector2 destination = new Vector2(goal.transform.position.x, goal.transform.position.y);
        return (destination - origin).magnitude;
    }
}
