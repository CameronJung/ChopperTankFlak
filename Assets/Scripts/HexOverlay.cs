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

    [SerializeField] private GameObject combatSprite;
    [SerializeField] private GameObject moveSprite;

    private TerrainTile myTile;
    private Tilemap map;
    public Vector3Int myCoords { get; private set;}

    //Represents the state of the hexagaon in a single property
    public HexState currState { get; private set; } = HexState.unreachable;

    private List<HexOverlay> adjacent;

    //these flags are used to mark tiles that have already been visited by a recursive algorythm
    public bool visited = false;
    public int distanceFrom = -1;

    // Start is called before the first frame update
    void Start()
    {
        
        Tilemap map = GameObject.Find(UniversalConstants.MAPPATH).GetComponent<Tilemap>();
        myCoords = map.WorldToCell(gameObject.transform.position);
        myTile = map.GetTile<TerrainTile>(myCoords);

        //Initialize adjacent hexes list
        adjacent = new List<HexOverlay>();

        Vector3Int neighbor = GridHelper.GetUpRight(myCoords);
        if (map.HasTile(neighbor))
        {
            adjacent.Add(map.GetInstantiatedObject(neighbor).GetComponent<HexOverlay>());
        }

        neighbor = GridHelper.GetUp(myCoords);
        if (map.HasTile(neighbor))
        {
            adjacent.Add(map.GetInstantiatedObject(neighbor).GetComponent<HexOverlay>());
        }

        neighbor = GridHelper.GetUpLeft(myCoords);
        if (map.HasTile(neighbor))
        {
            adjacent.Add(map.GetInstantiatedObject(neighbor).GetComponent<HexOverlay>());
        }

        neighbor = GridHelper.GetDownLeft(myCoords);
        if (map.HasTile(neighbor))
        {
            adjacent.Add(map.GetInstantiatedObject(neighbor).GetComponent<HexOverlay>());
        }

        neighbor = GridHelper.GetDown(myCoords);
        if (map.HasTile(neighbor))
        {
            adjacent.Add(map.GetInstantiatedObject(neighbor).GetComponent<HexOverlay>());
        }

        neighbor = GridHelper.GetDownRight(myCoords);
        if (map.HasTile(neighbor))
        {
            adjacent.Add(map.GetInstantiatedObject(neighbor).GetComponent<HexOverlay>());
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

    //reset all flags to null
    public void SetBlank()
    {
        visited = false;
        distanceFrom = -1;
        combatSprite.SetActive(false);
        moveSprite.SetActive(false);
        currState = HexState.unreachable;
    }

    public bool CanIpass(Unit me)
    {
        return myTile.CanUnitPass(me);
    }
    



    //Find what there is to do, return bool if this tile can be traversed
    public bool TravelGuide(Unit unit)
    {
        bool traversable = false;
        //firstly does this tile have a unit?
        if(this.occupiedBy != null)
        {
            if(unit.GetAllegiance() != occupiedBy.GetAllegiance())
            {
                //There is an enemy to fight here
                this.MarkCombat();
                //Enemies block movement so we return false
                return traversable;
            }
        }

        //Weather or not we can traverse this tile depends on terrain
        traversable = myTile.CanUnitPass(unit);
        if (traversable)
        {
            MarkMove();
        }
        
        return traversable;
    }


    //Explore the tile
    public List<HexOverlay> Explore(Unit unit, int travelled, List<HexOverlay> affected)
    {
        if (!visited)
        {
            //add self to list if we haven't already
            affected.Add(this);
        }

        this.visited = true;
        this.distanceFrom = travelled;

        if (TravelGuide(unit) && travelled <= unit.mobility)
        {
            if (travelled < unit.mobility)
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
                                    hex.Explore(unit, travelled + 1, affected);
                                }
                                
                            }
                            else
                            {
                                hex.Explore(unit, travelled + 1, affected);
                            }
                        }
                        else
                        {
                            hex.Explore(unit, travelled + 1, affected);
                        }
                    }
                }
            }
            else
            {
                //At the end of the units range look for enemies to attack
                foreach(HexOverlay hex in adjacent)
                {
                    if(!hex.visited && hex.GetOccupiedBy() != null)
                    {
                        if(hex.GetOccupiedBy().GetAllegiance() != unit.GetAllegiance() && this.occupiedBy == null)
                        {
                            //The unit is hostile
                            hex.MarkCombat();
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
    public List<HexOverlay> BeginExploration(Unit unit)
    {
        List<HexOverlay> affected = new List<HexOverlay>();
        visited = true;
        distanceFrom = 0;
        affected.Add(this);

        this.currState = HexState.reachable;
        foreach(HexOverlay hex in adjacent)
        {
            hex.Explore(unit, 1, affected);
            
            
        }
        return affected;
    }



    public Vector3[] MakePathToHere(Unit unit, Vector3[] path)
    {
        Vector3[] validPath = (Vector3[])path.Clone();

        foreach (HexOverlay hex in adjacent)
        {
            if (hex.distanceFrom == this.distanceFrom - 1 && hex.currState == HexState.reachable)
            {
                if (hex.ContinuePath(unit, ref validPath))
                {
                    validPath[this.distanceFrom] = gameObject.transform.position;
                }
            }
        }
        
        return validPath;
    }



    public bool ContinuePath(Unit unit, ref Vector3[] path)
    {
        bool allTheWay = false;
        
        if(this.distanceFrom > 0)
        {
            foreach (HexOverlay hex in adjacent)
            {
                if (hex.distanceFrom == this.distanceFrom - 1 && hex.currState == HexState.reachable)
                {
                    
                    if(hex.ContinuePath(unit, ref path))
                    {
                        path[this.distanceFrom] = gameObject.transform.position;
                        //Debug.Log("Found a path through: " + this.myCoords);
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

    private void MarkMove()
    {
        moveSprite.SetActive(true);
        combatSprite.SetActive(false);
        currState = HexState.reachable;
    }

    private void MarkCombat()
    {
        moveSprite.SetActive(false);
        combatSprite.SetActive(true);
        currState = HexState.attackable;
    }
}
