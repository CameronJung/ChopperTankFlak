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

    [SerializeField] private GameObject availSprite;
    [SerializeField] private GameObject combatSprite;
    [SerializeField] private GameObject moveSprite;

    private TerrainTile myTile;
    private Tilemap map;
    private Vector3Int myCoords;

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
        availSprite.SetActive(unit.GetAllegiance() == Faction.PlayerTeam);
        this.occupiedBy = unit;
    }

    //reset all flags to null
    public void SetBlank()
    {
        visited = false;
        distanceFrom = -1;
        availSprite.SetActive(false);
        combatSprite.SetActive(false);
        moveSprite.SetActive(false);
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
            }// we don't do anything with an allied unit because we don't want to mess with the state
        }
            //Weather or not we can traverse this tile depends on terrain
        traversable = myTile.CanUnitPass(unit);
        if (traversable && (occupiedBy == null))
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
                        hex.Explore(unit, travelled + 1, affected);
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
                        if(hex.GetOccupiedBy().GetAllegiance() != unit.GetAllegiance())
                        {
                            //The unit is hostile
                            hex.MarkCombat();
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
        foreach(HexOverlay hex in adjacent)
        {
            hex.distanceFrom = 1;
        }
        foreach(HexOverlay hex in adjacent)
        {
             hex.Explore(unit, 1, affected);
            
            
        }
        return affected;
    }

    private void MarkMove()
    {
        moveSprite.SetActive(true);
        combatSprite.SetActive(false);
        availSprite.SetActive(false);
    }

    private void MarkCombat()
    {
        moveSprite.SetActive(false);
        combatSprite.SetActive(true);
        availSprite.SetActive(false);
    }
}
