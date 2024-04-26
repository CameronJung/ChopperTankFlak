using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UniversalConstants;
using static GridHelper;



/*
 * This class's job is to conduct and manage A* searches with the parameters given to it upon insstantiation
 * 
 * 
 */
public class AStarMeasurement : MonoBehaviour
{


    private int Distance = -1;
    [SerializeField] private Tilemap Map;
    private HexOverlay Destination;
    private HexOverlay Origin;
    private Unit Agent;


    public bool Complete { get; private set; }


    //All G and H values will be stored in these dictionaries to avoid synchronisity issues
    private Dictionary<Vector3Int, int> GValues;
    private Dictionary<Vector3Int, int> HValues;


    public void Start()
    {
        this.GValues = new Dictionary<Vector3Int, int>();
        this.HValues = new Dictionary<Vector3Int, int>();
    }


    //Constructor
    public AStarMeasurement(Unit agent, Tilemap tilemap, Vector3Int goal)
    {
        this.Map = tilemap;
        this.Agent = agent;
        this.Destination = Map.GetInstantiatedObject(goal).GetComponent(typeof(HexOverlay)) as HexOverlay;
        this.Origin = Map.GetInstantiatedObject(Agent.myTilePos).GetComponent(typeof(HexOverlay)) as HexOverlay;

        this.GValues = new Dictionary<Vector3Int, int>();
        this.HValues = new Dictionary<Vector3Int, int>();


    }


    public AStarMeasurement(Tilemap tilemap)
    {
        this.Map = tilemap;

        this.GValues = new Dictionary<Vector3Int, int>();
        this.HValues = new Dictionary<Vector3Int, int>();
    }

    public Coroutine BeginMeasurement(Unit agent, Vector3Int goal)
    {
        this.Agent = agent;
        this.Destination = Map.GetInstantiatedObject(goal).GetComponent(typeof(HexOverlay)) as HexOverlay;
        this.Origin = Map.GetInstantiatedObject(Agent.myTilePos).GetComponent(typeof(HexOverlay)) as HexOverlay;

        this.GValues.Clear();
        this.HValues.Clear();

        return StartCoroutine(MeasureDistance());
    }

    /*
     * This variation of BeginMeasurement is used when a starting position other than the unit's current location is to be used
     */
    public Coroutine BeginMeasurement(Unit agent, Vector3Int goal, Vector3Int orig)
    {
        this.Agent = agent;
        this.Destination = Map.GetInstantiatedObject(goal).GetComponent(typeof(HexOverlay)) as HexOverlay;
        this.Origin = Map.GetInstantiatedObject(orig).GetComponent(typeof(HexOverlay)) as HexOverlay;

        this.GValues.Clear();
        this.HValues.Clear();

        return StartCoroutine(MeasureDistance());
    }



    private int LookupGValue(Vector3Int coords)
    {
        int value = int.MaxValue / 2;
        if (GValues.ContainsKey(coords))
        {
            value = GValues[coords];
        }

        return value;
    }


    private int LookupHValue(Vector3Int coords)
    {
        int value = int.MaxValue / 2;
        if (HValues.ContainsKey(coords))
        {
            value = HValues[coords];
        }

        return value;
    }


    private int LookupFValue(Vector3Int coords)
    {
        int h = LookupHValue(coords);
        int g = LookupGValue(coords);
        return g + h + (h / 4);
    }

    private void UpdateNavigationValuesAt(Vector3Int coords, int steps, Vector3Int target)
    {
        if (GValues.ContainsKey(coords))
        {
            if (GValues[coords] > steps + 1)
            {
                GValues[coords] = steps + 1;
            }
        }
        else
        {
            GValues[coords] = steps + 1;
        }
        if (!HValues.ContainsKey(coords))
        {
            HValues[coords] = GridHelper.CalcTilesBetweenGridCoords(coords, target);
        }
        
    }



    /*
     * This method will return the number of tiles that Agent will need to cross to reach Destination
     * 
     * If this instance hasn't finished measuring than the distance reported will be negative
     */
    public int GetMeasuredDistance()
    {
        return this.Distance;
    }



    private IEnumerator MeasureDistance()
    {
        LinkedList<HexOverlay> path = new LinkedList<HexOverlay>();

        UpdateNavigationValuesAt(this.Origin.myCoords, -1, this.Destination.myCoords);

        List<HexOverlay> closedList = new List<HexOverlay>();
        List<HexOverlay> openList = new List<HexOverlay>();
        List<HexOverlay> possibilities = new List<HexOverlay>();
        openList.Add(Origin);
        Debug.Assert(openList.Count >= 1, "The open list has " + openList.Count + " hexes in it");

        //float started = Time.realtimeSinceStartup;
        bool reached = false;
        int maxLoops = 10000;
        int iterations = 0;

        HexOverlay chosen = null;
        //Debug.Log(" measuring distance from: " + Origin.myCoords + " To " + Destination.myCoords + " using a Coroutine");

        do
        {
            possibilities.Clear();

            chosen = null;
            int lowestF = int.MaxValue / 2;




            // Take a tile from the openList
            foreach (HexOverlay hex in openList)
            {

                //If we find the target choose it outright and stop looking
                if (hex == this.Destination)
                {
                    chosen = hex;
                    reached = true;
                }
                else if (!reached)
                {
                    if (!(closedList.Contains(hex)))
                    {

                        possibilities.Add(hex);
                        int fValue = LookupFValue(hex.myCoords);
                        if (fValue <= lowestF)
                        {
                            if (fValue == lowestF)
                            {
                                if (chosen != null)
                                {
                                    if (LookupGValue(chosen.myCoords) < LookupGValue(hex.myCoords))
                                    {
                                        chosen = hex;
                                    }
                                }
                            }
                            else
                            {
                                chosen = hex;
                                lowestF = fValue;
                            }
                        }
                    }
                }

            }

            if (!reached)
            {
                // Add chosen tile to closed list
                closedList.Add(chosen);
                possibilities.Remove(chosen);
                Debug.Assert(chosen != null, "Chosen Hex was somehow null at: " + iterations + " iteration");
                // For every walkable tile adjacent to the chosen tile:
                foreach (HexOverlay hex in chosen.adjacent)
                {
                    //  If adjacent tile is in closed list, ignore it
                    if (!closedList.Contains(hex) && hex.CheckTerrainFor(Agent))
                    {
                        //  If adjacent tile is not in the open list, add it to the open list
                        if (!(openList.Contains(hex)))
                        {
                            openList.Add(hex);
                            UpdateNavigationValuesAt(hex.myCoords, LookupGValue(chosen.myCoords), this.Destination.myCoords);
                        }
                        else
                        {
                            //  If adjacent tile is already in the open list - update its g value should the chosen tile be a faster route to it
                            UpdateNavigationValuesAt(hex.myCoords, LookupGValue(chosen.myCoords), this.Destination.myCoords);
                        }

                    }

                }

                //
            }

            iterations++;
            if (iterations % 8 == 0)
            {
                yield return null;
            }
        }
        while ((possibilities.Count > 0 || !reached) && (iterations < maxLoops));
        //int searchIterations = iterations;
        //At this point either the target has been found and is in the variable "chosen" or every tile has been inspected without finding the target

        Debug.Assert(possibilities.Count == 0 || chosen == this.Destination, "The Algorithm has failed");

        if (chosen == this.Destination)
        {
            iterations = 0;
            //The algorithm found the target now assemble a path
            while (chosen != this.Origin && iterations < 200)
            {
                HexOverlay closest = null;
                int lowestG = int.MaxValue / 2;
                foreach (HexOverlay hex in chosen.adjacent)
                {
                    if (LookupGValue(hex.myCoords) < lowestG)
                    {
                        lowestG = LookupGValue(hex.myCoords);
                        closest = hex;
                    }
                }
                chosen = closest;
                path.AddFirst(chosen);
                iterations++;
            }
        }
        else
        {
            Debug.Log("A* algorithm failed to find a path for " + Agent.GetUnitType() + " " + Agent.gameObject.name);
        }


        //Debug.Log("Calculation took: " + (Time.realtimeSinceStartup - started) + " seconds and " + searchIterations + " iterations to measure a distance of " + path.Count + " tiles away.");
        
        this.Distance = path.Count;
    }


}
