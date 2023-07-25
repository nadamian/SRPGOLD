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

    public int CurrentHP;
    public int Movement = 6;

    public int endurance; //Governs Max HP, Fatigue, and half of the equip load formula 
    public int strength; //Half of equip load formula, hard caps total equip load, governs damage dealt with strength-based weapons. 
    public int dexterity; //Governs crit chance, half of dodge equation, and speed of learning weapon skills. Damage dealt with dex-based weapons
    public int speed; //governs half of dodge equation and ability to multi-attack
    public int intelligence; //governs magic damage, magic accuracy, and ability to learn non-weapon skills
    public int willpower; //governs resistance to magic damage and is a minimum requirement to increase weapon level
    //Weapon skill to be implemented
    public int defense;
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
        SetUnequippedArmor();
        CurrentHP = endurance * 2;
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

    public void Attack(Unit defender, bool canCounter)
    {
        //Combat variables
        bool attackType = GetAttackType();
        bool defenderAttackType = defender.GetAttackType();
        int attackPower = GetAttackPower();
        int defenderAttackPower = defender.GetAttackPower();
        int defendLevel = defender.level; //saved so that we can computed exp gain after destroying defender in case they die.
        int speedDif = GetAttackSpeed() - defender.GetAttackSpeed();
        Debug.Log("Speed Difference: " + speedDif.ToString());
        int attackerTimesToAttack = speedDif >= 0 ? Mathf.FloorToInt(speedDif / location.map.extraAttackThreshold) + 1 : 1; //we store these like this so that weapon effects granting bonus attacks can be added after
        int defenderTimesToAttack = speedDif <= 0 ? Mathf.FloorToInt(MathF.Abs(speedDif / location.map.extraAttackThreshold)) + 1 : 1;
        int maxAttacks = Math.Max(attackerTimesToAttack, defenderTimesToAttack);
        int attackerAttacksRemaining = attackerTimesToAttack;
        int defenderAttacksRemaining = defenderTimesToAttack;
        Debug.Log("Attacker attacks: " + attackerAttacksRemaining.ToString());
        Debug.Log("Defender attacks: " + defenderAttacksRemaining.ToString());
        bool attackKilled = false; // these are used to determine experience gain b/c units gain more for a kill
        bool defendKilled = false;
        for (int i = 0; i <= maxAttacks; i++)
        {
            Debug.Log("attack #" + i.ToString());
            if (attackerAttacksRemaining > 0)
            {
                attackKilled = defender.TakeDamage(attackPower, attackType);
                attackerAttacksRemaining--;
            }
            if (defender.IsAlive() && canCounter && defenderAttacksRemaining > 0)
            {
                defendKilled = TakeDamage(defenderAttackPower, defenderAttackType);
                defenderAttacksRemaining--;
            }
        }
        location.map.resetGlobals();
        if (!defender.isAlive)
        {
            defender.Die();
        }
        else
        {
            defender.GainExp(level, defendKilled);
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
            for (int j = growths[i]; j > 0; j -= 100) //Potential to level stats multiple times if growth exceedes 100
            {
                if (randomNumber <= j)
                {
                    stats[i]++;
                    Debug.Log(this.name.ToString() + " leveled " + stats[i].ToString());
                }
            }
        }   
    }

    //Combat Helpers
    public bool TakeDamage(int incomingAttackPower, bool attackType)
    {
        int defendStat = attackType ? defense: willpower;
        int damage = incomingAttackPower - defendStat;
        CurrentHP -= damage;
        Debug.Log(name + " took " + damage.ToString() + " damage and has " + CurrentHP.ToString() + " HP remaining");
        if (CurrentHP <= 0)
        {
            isAlive = false;
            return true;
        }
        return false;
    }

    //Getters and Setters
    public bool IsAlive()
    {
        return isAlive;
    }
    public int GetAttackPower()
    {
        //Potential to add item bonuses
        if (weapon == null) { return 0; }
        if (weapon.techniqueType == 0) { return weapon.power + strength; }
        if (weapon.techniqueType == 1) { return weapon.power + dexterity; }
        if (weapon.techniqueType == 2) { return weapon.power + intelligence; }
        else { Debug.LogError("Weapon Technique Value Incorrectly Assigned"); return -1; }
    }

    //Subtracts equip load in excess of capacity from speed
    public int GetAttackSpeed()
    {
        //Potential to add item bonuses
        int mitigator = strength + endurance;
        int equipWeight = GetTotalCarryWeight();
        int overWeight = equipWeight - mitigator;
        if (overWeight < 0) { overWeight = 0; }
        return speed - overWeight;
    }

    public int GetMinRange()
    {
        if (weapon != null)
        {
            return weapon.minRange;
        }
        else
        {
            return 0;
        }
    }

    public int GetMaxRange()
    {
        if (weapon != null)
        {
            return weapon.maxRange;
        }
        else
        {
            return 0;
        }
    }

    public int GetPhysicalDefense()
    {
        //Potential to add item bonuses 
        defense = head.defense + chest.defense + arms.defense + legs.defense;
        return defense;
    }

    public int GetMagicalDefense()
    {
        //Potential to add item bonuses
        return willpower;
    }

    public int GetTotalCarryWeight()
    {
        SetUnequippedArmor();
        return weapon.weight + head.weight + chest.weight + arms.weight + legs.weight;
    }

    public bool GetAttackType()
    {
        return weapon.techniqueType < 2;
    }

    //Kills unit
    public void Die()
    {
        location.unit = null;
        Debug.Log(name + " has shuffled off this mortal coil");
        isAlive = false;
        GetComponent<SpriteRenderer>().enabled = false;
        Destroy(this);
    }

    public void ResetMovesHighlight()
    {
        foreach (Node move in moves)
        {
            move.resetColor();
        }
    }

    public void SetUnequippedArmor()
    {
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


    private bool FastApproximately(float a, float b, float threshold)
    {
        return ((a - b) < 0 ? ((a - b) * -1) : (a - b)) <= threshold;
    }

}
