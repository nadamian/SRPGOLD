using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour
{
    public Tilemap map;
    public int sizeX;
    public int sizeY;
    public int minX;
    public int minY;
    public GameObject NodePrefab;
    public int turn;
    public GameObject GameMenu;
    // Start is called before the first frame update
    void Awake()
    {
        Unit[] units = FindObjectsOfType<Unit>();
        Node[] nodes = FindObjectsOfType<Node>();
        foreach (Node node in nodes)
        {
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
