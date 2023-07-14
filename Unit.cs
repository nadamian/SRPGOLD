using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] GameObject playMenuPrefab; 

    [Header("Backend")]
    public int allegiance;
    public int moveColor;
    public List<Node> moves;
    public List<Node> path;
    public float moveSpeed = 5f;
    public bool hasMoved = false;
    public Node location;
    private bool isAlive = true;
    private float distanceApprox = .005f;
    private Node lastLocation;

    [Header("Stats")]
    public int HP = 10;
    public int Movement = 6;
    public int attackPower = 5;
    public int attackRange = 1;


    // Update is called once per frame
    void Update()
    {
        if (path != null && path.Count > 1)
        {
            foreach (Node move in moves)
            {
                move.resetColor();
            }
            Vector2 dir = path[1].transform.position - transform.position;
            transform.Translate(dir.normalized * moveSpeed * Time.deltaTime, Space.World);
            path[path.Count - 1].unit = this;
            if (FastApproximately(transform.position.x, path[1].transform.position.x, distanceApprox) && FastApproximately(transform.position.y, path[1].transform.position.y, distanceApprox))
            {
                path.RemoveAt(1);
                if (path.Count == 1)
                {
                    OpenMenu();
                    hasMoved = true;
                    //TODO Action menu. Either attack, use item, do other action or wait.
                    lastLocation = location;
                    location = path[0];
                    path = null;
                    moves = null;
                    path = new List<Node>();
                    moves = new List<Node>();
                }
            }
        }
    }

    public void OpenMenu()
    {
        GameObject menu = Instantiate(playMenuPrefab, this.transform.position, Quaternion.identity);
        ButtonManager.playMenu = menu;
        Global.attackMenuActive = true;
    }

    public void Attack(Unit defender)
    {
        //as the combat system matures there will be greater calculation required in determining these values. 
        int damageDealt = attackPower;
        int damageTaken = defender.attackPower;
        //Leave room for repetition when multi attacks implemented 
        defender.TakeDamage(damageDealt);
        if(defender.IsAlive())
        {
            TakeDamage(damageTaken);
        }
        Global.resetGlobals();
    }
    
    public void TakeDamage(int damage)
    {
        HP -= damage;
        Debug.Log(name + " took " + damage.ToString() + " damage and has " + HP.ToString() + "HP remaining");
        if (HP <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log(name + " has shuffled off this mortal coil");
        isAlive = false;
        GetComponent<SpriteRenderer>().enabled = false;
        Destroy(this);
    }

    public bool IsAlive()
    {
        return isAlive;
    }
    private bool FastApproximately(float a, float b, float threshold)
    {
        return ((a - b) < 0 ? ((a - b) * -1) : (a - b)) <= threshold;
    }
}
