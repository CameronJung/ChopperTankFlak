using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UniversalConstants;
using static GridHelper;

public class HexOverlay : MonoBehaviour
{
    [SerializeField] private Unit occupiedBy { get; set; }
    public bool hasUnit = false;

    [SerializeField] protected GameObject combatSprite;
    [SerializeField] protected GameObject moveSprite;
    [SerializeField] protected GameObject holdSprite;

    private TerrainTile myTile;
    private Tilemap map;
    public Vector3Int myCoords { get; private set; }

    //Represents the state of the hexagaon in a single property
    public HexState currState { get; protected set; } = HexState.unreachable;

    public List<HexOverlay> adjacent { get; protected set;}

    //these flags are used to mark tiles that have already been visited by a recursive algorythm
    public bool visited = false;
    public int distanceFrom = -1;

    public HexIntel intel { get; protected set; }
    public AstarNavigable nav { get; private set; }

    // Start is called before the first frame update
    protected void Start()
    {
        
        map = GameObject.Find(UniversalConstants.MAPPATH).GetComponent<Tilemap>();
        myCoords = map.WorldToCell(gameObject.transform.position);
        gameObject.name = (myCoords.x.ToString() + "_" + myCoords.y.ToString());
        myTile = map.GetTile<TerrainTile>(myCoords);
        intel = new HexIntel(this);
        nav = gameObject.GetComponent<AstarNavigable>();
        nav.CalculateCoords(myCoords);


        //Initialize adjacent hexes list
        adjacent = new List<HexOverlay>();

        Vector3Int neighbor = GridHelper.GetUpRight(myCoords);
        if (map.HasTile(neighbor))
        {
            adjacent.Add(map.GetInstantiatedObject(neighbor).GetComponent(typeof(HexOverlay)) as HexOverlay);
        }

        neighbor = GridHelper.GetUp(myCoords);
        if (map.HasTile(neighbor))
        {
            adjacent.Add(map.GetInstantiatedObject(neighbor).GetComponent(typeof(HexOverlay)) as HexOverlay);
        }

        neighbor = GridHelper.GetUpLeft(myCoords);
        if (map.HasTile(neighbor))
        {
            adjacent.Add(map.GetInstantiatedObject(neighbor).GetComponent(typeof(HexOverlay)) as HexOverlay);
        }

        neighbor = GridHelper.GetDownLeft(myCoords);
        if (map.HasTile(neighbor))
        {
            adjacent.Add(map.GetInstantiatedObject(neighbor).GetComponent(typeof(HexOverlay)) as HexOverlay);
        }

        neighbor = GridHelper.GetDown(myCoords);
        if (map.HasTile(neighbor))
        {
            adjacent.Add(map.GetInstantiatedObject(neighbor).GetComponent(typeof(HexOverlay)) as HexOverlay);
        }

        neighbor = GridHelper.GetDownRight(myCoords);
        if (map.HasTile(neighbor))
        {
            adjacent.Add(map.GetInstantiatedObject(neighbor).GetComponent(typeof(HexOverlay)) as HexOverlay);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Unit GetOccupiedBy()
    {
        return this.occupiedBy;
    }

    public void SetOccupiedBy(Unit unit)
    {
        hasUnit = unit != null;
        this.occupiedBy = unit;
    }

    

    public bool CanIpass(Unit me)
    {
        bool pass = myTile.CanUnitPass(me);

        if(this.occupiedBy != null && pass)
        {
            pass = occupiedBy.GetAllegiance() == me.GetAllegiance();
        }

        return pass;
    }

    /*
     * RepresentsPossibleMoveTo
     * 
     * This method returns true if terrain and occupancy of this HexOverlay Represent a valid move
     * for the Unit given in the parameter: unit
     * 
     * !Note! this method specificly does not rely on HexState and does not account for weather the unit
     * can reach this tile
     */
    public bool RepresentsPossibleMoveTo(Unit unit)
    {
        bool valid = myTile.CanUnitPass(unit);

        if(this.occupiedBy != null)
        {
            valid = valid && this.occupiedBy.GetAllegiance() != unit.GetAllegiance() || unit == this.occupiedBy ;
        }

        return valid;
    }


    public bool CheckTerrainFor(Unit me)
    {
        return myTile.CanUnitPass(me);
    }


    //Returns if the given unit is able to be on this tile
    public bool CanIBeOn(Unit me)
    {
        return myTile.CanUnitPass(me) && ((occupiedBy == null) || (occupiedBy == me));
    }


    protected void CanBeAttackedBy(Unit unit)
    {
        //This function is called on a tile it means the unit can attack it
        //We only need to track where the player's units can attack
        if (unit.GetAllegiance() == Faction.PlayerTeam && unit.myState != UnitState.stalemate)
        {
            intel.AffectedBy(unit);
        }
    }
    



    //Find what there is to do, return bool if this tile can be traversed
    public virtual bool TravelGuide(Unit unit, bool doShow = true)
    {
        bool traversable = false;
        //firstly does this tile have a unit?
        if(this.occupiedBy != null)
        {
            if(unit.GetAllegiance() != occupiedBy.GetAllegiance())
            {
                //There is an enemy to fight here
                this.MarkCombat(doShow);
                //Enemies block movement so we return false
                return traversable;
            }
        }

        CanBeAttackedBy(unit);

        //Weather or not we can traverse this tile depends on terrain
        traversable = myTile.CanUnitPass(unit);
        if (traversable)
        {
            MarkMove(doShow);
        }
        
        return traversable;
    }


    //Explore the tile
    public List<HexOverlay> Explore(Unit unit, int travelled, List<HexOverlay> affected, bool doShow = true)
    {
        if (!visited)
        {
            //add self to list if we haven't already
            affected.Add(this);

            CanBeAttackedBy(unit);
        }

        this.visited = true;
        this.distanceFrom = travelled;

        if (TravelGuide(unit, doShow) && travelled <= unit.GetMobility())
        {
            if (travelled < unit.GetMobility())
            {
                foreach (HexOverlay hex in adjacent)
                {
                    if (hex.distanceFrom > travelled + 1 || !hex.visited)
                    {
                        
                        if(hex.occupiedBy != null)
                        {
                            if(hex.occupiedBy.GetAllegiance() != unit.GetAllegiance())
                            {
                                //The hex is occupied by an enemy
                                if(this.occupiedBy == null)
                                {
                                    //Tiles occupied by enemies should only be expplored from empty tiles
                                    hex.Explore(unit, travelled + 1, affected, doShow);
                                }
                                
                            }
                            else
                            {
                                hex.Explore(unit, travelled + 1, affected, doShow);
                            }
                        }
                        else
                        {
                            hex.Explore(unit, travelled + 1, affected, doShow);
                        }
                    }
                }
            }
            else
            {
                //At the end of the units range look for enemies to attack
                foreach(HexOverlay hex in adjacent)
                {
                    hex.CanBeAttackedBy(unit);
                    if(!hex.visited && hex.GetOccupiedBy() != null)
                    {
                        if(hex.GetOccupiedBy().GetAllegiance() != unit.GetAllegiance() && this.occupiedBy == null)
                        {
                            //The unit is hostile
                            hex.MarkCombat(doShow);
                            hex.distanceFrom = travelled + 1;
                            affected.Add(hex);
                        }
                    }
                }
            }
        }
        
        return affected;
        
    }



    //Used to start exploration
    public virtual List<HexOverlay> BeginExploration(Unit unit, bool doShow = true)
    {
        List<HexOverlay> affected = new List<HexOverlay>();
        visited = true;
        distanceFrom = 0;
        affected.Add(this);

        MarkHold(doShow);
        //this.currState = HexState.reachable;
        foreach(HexOverlay hex in adjacent)
        {
            hex.Explore(unit, 1, affected, doShow);
            
            
        }
        return affected;
    }


    //ABSTRACTION
    public Vector3[] MakePathToHere(Unit unit, Vector3[] path, int numberOfOrders)
    {
        Vector3[] validPath = (Vector3[])path.Clone();

        bool isAttack = this.currState == HexState.attackable;
        bool givenValidAttackPosition = false;
        int indexOfLast = numberOfOrders;

        //If the mission is to attack and there are more than 1 order than distanceFrom needn't be sequential
        //Further, the tile to attack from has likely been selected already
        if (isAttack && numberOfOrders > 1)
        {

            Debug.Log("Length is: " + validPath.Length);
            Debug.Log("Number of orders is: " + numberOfOrders);

            //check if the tile in the path is valid, if it is determine a path from that position
            HexOverlay prev = map.GetInstantiatedObject(map.WorldToCell(validPath[numberOfOrders -1])).GetComponent<HexOverlay>();
            if (adjacent.Contains(prev) && prev.CanIBeOn(unit) && prev.currState == HexState.reachable)
            {
                //This is the only opportunity to make this variable true
                Debug.Log("The suggested attack position, " + prev.myCoords + " is valid");
                givenValidAttackPosition = prev.ContinuePath(unit, ref validPath);
            }
            else
            {
                Debug.Log("Attack position was invalid");
                prev = this.FindValidNeighborFor(unit);
                indexOfLast = prev.distanceFrom + 1;
            }

        }
        
        //If a path from the second last point can not be determined draw a new path
        if(!givenValidAttackPosition)
        {
            //Check every adjacent tile
            foreach (HexOverlay hex in adjacent)
            {
                //Only consider reachable tiles that provide direct paths
                if (hex.distanceFrom == this.distanceFrom - 1 &&
                    (hex.currState == HexState.reachable || hex.currState == HexState.hold))
                {
                    /*
                    if (!(isAttack && hex.occupiedBy != null)) { 
                    }
                        if (hex.ContinuePath(unit, ref validPath))
                        {
                            validPath[this.distanceFrom] = gameObject.transform.position;
                        }*/
                    if (isAttack)
                    {
                        if (hex.CanIBeOn(unit))
                        {
                            if(hex.ContinuePath(unit, ref validPath))
                            {
                                validPath[this.distanceFrom] = gameObject.transform.position;
                            }
                        }
                    }
                    else if (hex.CanIpass(unit))
                    {
                        if (hex.ContinuePath(unit, ref validPath))
                        {
                            validPath[this.distanceFrom] = gameObject.transform.position;
                        }
                    }
                }

            }
        }
        else
        {
            validPath[numberOfOrders] = gameObject.transform.position;
        }
        
        string oldPath = map.WorldToCell(path[0]).ToString();
        string nubPath = map.WorldToCell(validPath[0]).ToString();


        for(int idx = 0; idx < path.Length; idx++)
        {
            oldPath += " -> " + map.WorldToCell(path[idx]);
            nubPath += " -> " + map.WorldToCell(validPath[idx]);
        }

        Debug.Log("Path was changed from: " + oldPath);
        Debug.Log("Path was changed To:   " + nubPath);
        
        return validPath;
    }



    public bool ContinuePath(Unit unit, ref Vector3[] path)
    {
        bool allTheWay = false;
        


        if(this.distanceFrom > 0)
        {
            foreach (HexOverlay hex in adjacent)
            {
                if (hex.distanceFrom == this.distanceFrom - 1 && 
                    (hex.currState == HexState.reachable || hex.currState == HexState.hold))
                {
                    
                    if(hex.ContinuePath(unit, ref path))
                    {
                        path[this.distanceFrom] = gameObject.transform.position;
                        //Return immediately we don't need to look at anything else
                        allTheWay = true;
                    }
                }
            }
        }
        else
        {
            
            allTheWay = (this.distanceFrom == 0);
        }
        

        return allTheWay;
    }


    /*
     * MakeDirectPathToHere
     * 
     * This method will take a path represented by the parameter path and will follow the distanceFrom property of other hexes
     * to create a path leading to the position of the Unit given in the parameter "unit" 
     * 
     * !NOTE! The path provided must have this hex's position in the element with the index of the parameter "destinationIdx"
     */
    public Vector3[] MakeDirectPathToHere(Vector3[] path, int destinationIdx, Unit unit)
    {
        Vector3[] validPath = (Vector3[])path.Clone();

        Debug.Assert(Vector3.SqrMagnitude(validPath[destinationIdx] - gameObject.transform.position) < 0.1f, "Attempted to make path with incorrect indexing");
        bool pathComplete = false;


        foreach( HexOverlay hex in adjacent)
        {
            if(hex.distanceFrom == this.distanceFrom - 1 && hex.CanIpass(unit) && 
                (hex.currState == HexState.hold || hex.currState == HexState.reachable))
            {
                
                if(!pathComplete && hex.ContinueDirectPath(ref validPath, unit))
                {
                    pathComplete = true;
                }
            }
        }
        /*
        Debug.Assert(pathComplete, "A complete path was not found");
        string oldPath = map.WorldToCell(path[0]).ToString();
        string nubPath = map.WorldToCell(validPath[0]).ToString();


        for (int idx = 0; idx < path.Length; idx++)
        {
            oldPath += " -> " + map.WorldToCell(path[idx]);
            nubPath += " -> " + map.WorldToCell(validPath[idx]);
        }

        Debug.Log("Path was changed from: " + oldPath);
        Debug.Log("Path was changed To:   " + nubPath);
        */
        return validPath;

    }

    public bool ContinueDirectPath(ref Vector3[] path, Unit unit)
    {
        bool allTheWay = false;

        if(this.distanceFrom == 0)
        {
            
            allTheWay = true;
        }
        else
        {
            foreach (HexOverlay hex in adjacent)
            {
                if (hex.distanceFrom == this.distanceFrom - 1 && hex.CanIpass(unit) &&
                    (hex.currState == HexState.hold || hex.currState == HexState.reachable))
                {
                    
                    if (!allTheWay && hex.ContinueDirectPath(ref path, unit))
                    {
                        allTheWay = true;
                    }
                }
            }
        }

        if (allTheWay)
        {
            path[this.distanceFrom] = transform.position;
            //Debug.Log("Found a path through: " + map.WorldToCell(transform.position));
        }

        return allTheWay;
    }














    public Vector3Int FindValidNeighbor()
    {

        Vector3Int neighbor = new Vector3Int();

        foreach (HexOverlay hex in adjacent)
        {
            if (hex.currState == HexState.reachable)
            {
                neighbor = hex.myCoords;
            }
        }


        return neighbor;
    }

    public HexOverlay FindValidNeighborFor(Unit unit)
    {

        HexOverlay neighbor = null;

        foreach (HexOverlay hex in adjacent)
        {
            if (hex.currState == HexState.reachable && hex.CanIBeOn(unit))
            {
                neighbor = hex;
            }
        }


        return neighbor;
    }


    public Vector3Int GetAstarCoords()
    {
        return nav.cubePosition;
    }




    //Astar compliance

    /*
     * This method returns a neigboring tile that is accessible to the selected unit
     */

    public int HexDistTo(HexOverlay hex)
    {
        int distance = 0;

        Vector3Int diff = (hex.GetAstarCoords() - this.GetAstarCoords());

        diff.x = Mathf.Abs(diff.x);
        diff.y = Mathf.Abs(diff.y);
        diff.z = Mathf.Abs(diff.z);

        distance = Mathf.Max(diff.x, diff.y, diff.z);

        return distance;

    }

    
    public bool IsHexReachable()
    {
        return this.currState != HexState.unreachable;
    }



    //These functions are used to change an overlay's state
    //The parameter doShow indicates if the changes of state should be displayed visually true by default
    //ENCAPSULATION
    protected virtual void MarkMove(bool doShow = true)
    {
        moveSprite.SetActive(doShow);
        combatSprite.SetActive(false);
        holdSprite.SetActive(false);
        currState = HexState.reachable;
    }

    protected virtual void MarkCombat(bool doShow = true)
    {
        moveSprite.SetActive(false);
        combatSprite.SetActive(doShow);
        holdSprite.SetActive(false);
        currState = HexState.attackable;
    }

    protected virtual void MarkHold(bool doShow = true)
    {
        moveSprite.SetActive(false);
        combatSprite.SetActive(false);
        holdSprite.SetActive(doShow);
        currState = HexState.hold;
    }

    //reset all flags to null
    public virtual void SetBlank()
    {
        visited = false;
        distanceFrom = -1;
        combatSprite.SetActive(false);
        moveSprite.SetActive(false);
        holdSprite.SetActive(false);
        currState = HexState.unreachable;
    }
}
