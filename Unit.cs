using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] GameObject playMenuPrefab;
    public int moveColor;
    public float moveSpeed = 5f;
    private float distanceApprox = .005f; //The distance threshold to determine if the unit occupies a given node

    [Header("Backend")]
    public int allegiance; //Unit's team
    public List<Node> moves; //Populates with possible moves when pathfinding
    public List<Node> path; //Holds a path if one is selected for unit to travel. Emptied after reaching its end. 
    public bool hasMoved = false; //Returned to false at the end of turn
    public Node location;
    public Node lastLocation;
    private bool isAlive = true;
    public bool undo = false; //True if unit is undoing a previous move

    //Constants for the logistic function that models experience gain 
    private float k = -((Mathf.Log(0.005f) - Mathf.Log(4.0f)) / 20);
    private float x_0 = -((20 * Mathf.Log(4.0f)) / (Mathf.Log(.005f) - Mathf.Log(4.0f)));
    private float maxExp = 100.0f;

    [Header("Equipment")]
    public Weapon weapon;
    public Armor head;
    public Armor chest;
    public Armor arms;
    public Armor legs;
    public List<Inventory> inventory;

    [Header("Stats")]
    public int level;
    public int experience;

    public int HP = 10;
    public int Movement = 6;

    public int attackPower = 5; //Will eventually be calculated from stats and weapon stats
    public int attackRange = 1; //Will be made a property of weapon
    public int endurance; //Governs HP, Fatigue, and half of the equip load formula 
    public int strength; //Half of equip load formula, hard caps total equip load, governs damage dealt with strength-based weapons. 
    public int dexterity; //Governs crit chance, half of dodge equation, and speed of learning weapon skills. Damage dealt with dex-based weapons
    public int speed; //governs half of dodge equation and ability to multi-attack
    public int intelligence; //governs magic damage, magic accuracy, and ability to learn non-weapon skills
    public int willpower; //governs resistance to magic damage and is a minimum requirement to increase weapon level
    //Weapon skill to be implemented
    public int[] stats;

    [Header("Growth Rates")]
    public int enduranceGrowth = 50;
    public int strengthGrowth = 50;
    public int dexterityGrowth = 50;
    public int speedGrowth = 50;
    public int intelligenceGrowth = 50;
    public int willpowerGrowth = 50;
    public int[] growths;

    private void Start()
    {
        stats = new int[] { endurance, strength, dexterity, speed, intelligence, willpower };
        growths = new int[] { enduranceGrowth, strengthGrowth, dexterityGrowth, speedGrowth, intelligenceGrowth, willpowerGrowth };
        if (head == null)
        {
            head = new Armor("No Helmet", 0, 0, 0, 0, 0);
        }
        if (chest == null)
        {
            chest = new Armor("No Chest", 0, 1, 0, 0, 0);
        }
        if (arms == null)
        {
            arms = new Armor("No Arms", 0, 2, 0, 0, 0);
        }
        if (legs == null)
        {
            legs = new Armor("No Legs", 0, 3, 0, 0, 0);
        }
    }
    // Update is called once per frame
    void Update()
    {
        //Moves the unit if a path has been set for them
        if (path != null && path.Count > 1)
        {
            ResetMovesHighlight();
            Vector2 dir = path[1].transform.position - transform.position;
            transform.Translate(dir.normalized * moveSpeed * Time.deltaTime, Space.World);
            path[path.Count - 1].unit = this;
            //Determines that unit has reached the next node on their path
            if (FastApproximately(transform.position.x, path[1].transform.position.x, distanceApprox) && FastApproximately(transform.position.y, path[1].transform.position.y, distanceApprox))
            {
                path.RemoveAt(1);
                //If unit has reached the end of their path, sets path to blank and instantiates action menu.
                if (path.Count == 1 && !undo)
                {
                    OpenMenu();
                    hasMoved = true;
                    lastLocation = location;
                    location = path[0];
                    path = null;
                    moves = null;
                    path = new List<Node>();
                    moves = new List<Node>();
                    location.map.UpdateOccupation();
                }
                //If unit has reached the end of their path which was undoing previous movement, finishes resetting position.
                if (path.Count == 1 && undo)
                {
                    hasMoved = false;
                    location = path[0];
                    path = null;
                    moves = null;
                    path = new List<Node>();
                    moves = new List<Node>();
                    location.map.UpdateOccupation();
                    undo = false;
                    location.SelectUnitToMove();
                }
            }
        }
    }

    //Opens the unit action menu
    public void OpenMenu()
    {
        GameObject menu = Instantiate(playMenuPrefab, this.transform.position, Quaternion.identity);
        location.map.buttonManager.playMenu = menu;
        location.map.attackMenuActive = true;
    }

    public void Attack(Unit defender)
    {
        //as the combat system matures there will be greater calculation required in determining these values. 
        int damageDealt = attackPower;
        int damageTaken = defender.attackPower;
        int defendLevel = defender.level; //saved so that we can computed exp gain after destroying defender in case they die.
        //Leave room for repetition when multi attacks implemented 
        bool attackKilled = defender.TakeDamage(damageDealt);
        if(defender.IsAlive() /*&& in range of counter*/)
        {
            bool defendKilled = TakeDamage(damageTaken);
            defender.GainExp(level, defendKilled);
        }
        location.map.resetGlobals();
        if (!defender.isAlive)
        {
            defender.Die();
        }
        if (!isAlive)
        {
            Die();
        }
        else
        {
            GainExp(defendLevel, attackKilled);
        }
    }

    public void GainExp(int enemyLevel, bool combatResult)
    {
        int levelDifference = enemyLevel - level;
        int baseExp = Mathf.RoundToInt(maxExp / 1 + Mathf.Exp(-k * (levelDifference - x_0)));
        int expGain =  combatResult ? baseExp : baseExp / 4;
        experience += expGain;
        if (experience >= 100)
        {
            LevelUp();
            experience -= 100;
        }
        Debug.Log("attacker gained " + expGain.ToString() + " experience and now has a total of " + experience.ToString());
    }

    private void LevelUp()
    {
        level++;
        for(int i = 0; i < stats.Length; i++)
        {
            float randomNumber = UnityEngine.Random.Range(0, 100);
            if (randomNumber <= growths[i])
            {
                stats[i]++;
                Debug.Log(this.name.ToString() + " leveled " + stats[i].ToString());
            }
        }   
    }

    public bool TakeDamage(int damage)
    {
        HP -= damage;
        Debug.Log(name + " took " + damage.ToString() + " damage and has " + HP.ToString() + "HP remaining");
        if (HP <= 0)
        {
            isAlive = false;
            return true;
        }
        return false;
    }

    //Kills unit
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
    public void ResetMovesHighlight()
    {
        foreach (Node move in moves)
        {
            move.resetColor();
        }
    }

    private bool FastApproximately(float a, float b, float threshold)
    {
        return ((a - b) < 0 ? ((a - b) * -1) : (a - b)) <= threshold;
    }

}
