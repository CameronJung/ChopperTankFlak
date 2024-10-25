using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UniversalConstants;




/*
 * UNIT LEADER
 * 
 * The unit leader component is resposible for analyzing what its associated unit is able to do
 * 
 * Mainly, this component recieves a suggested objective from the strategist
 * Taking this suggestion the unit leader tries to plot a path to the goal accounting for terrain and the position of enemy units
 * While doing this, the leader takes note of other objectives that it could reach
 * The leader then returns a collection of objectives that it can reach
 */
public class UnitLeader : MonoBehaviour
{
    private int Distance = -1;
    private Tilemap Map;
    private HexOverlay Destination;
    private HexOverlay Origin;
    private float TimeBudget;

    private float TimeSpentThisFrame;

    private GlobalNavigationData NavigationData;

    public bool Complete { get; private set; }


    //All G and H values will be stored in these dictionaries to avoid synchronisity issues
    private Dictionary<Vector3Int, int> GValues;
    private Dictionary<Vector3Int, int> HValues;

    public HashSet<Vector3Int> DiscoveredDestinations { get; private set; } = new HashSet<Vector3Int>();
    public Unit Detatchment { get; private set; }
    private Strategist Tactician;


    
    private List<ObjectiveAssignment> PossibleObjectives;
    public HexOverlay WayPoint { get; private set; }
    public Objective WaypointTowards { get; private set; } = null;

    public void Start()
    {
        Tactician = GameObject.Find(UniversalConstants.AIPATH).GetComponent<Strategist>();
        Detatchment = gameObject.GetComponent<Unit>();

        this.GValues = new Dictionary<Vector3Int, int>();
        this.HValues = new Dictionary<Vector3Int, int>();

        //Currently these time budget is a at half value because the game has two measurement components in its setup
        if (Application.isMobilePlatform)
        {
            this.TimeBudget = 0.01f;
            //Mobile applications target 30 FPS by default so we can double the normal budget
        }
        else
        {
            //Standard FPS is 60, so we need to end the frame by 0.0167ms
            this.TimeBudget = 0.005f;
        }

        NavigationData = GameObject.Find(MAPPATH).GetComponent<GlobalNavigationData>();
        Map = GameObject.Find(MAPPATH).GetComponent<Tilemap>();
        PossibleObjectives = new List<ObjectiveAssignment>();
        WayPoint = null;
        WaypointTowards = null;
    }

    public bool HasWaypoint()
    {
        return WayPoint != null;
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
        //return g + h + (h / 4);
        return g + h;
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


    public List<ObjectiveAssignment> GetPossibleAssignments()
    {
        PossibleObjectives.Sort();
        return this.PossibleObjectives;
    }


    public int GetMeasuredDistance()
    {
        return this.Distance;
    }


    public Coroutine BeginPlanning( Vector3Int goal)
    {


        this.Destination = Map.GetInstantiatedObject(goal).GetComponent(typeof(HexOverlay)) as HexOverlay;
        this.Origin = Map.GetInstantiatedObject(Detatchment.myTilePos).GetComponent(typeof(HexOverlay)) as HexOverlay;
        this.PossibleObjectives.Clear();
        this.GValues.Clear();
        this.HValues.Clear();
        this.WayPoint = null;
        this.WaypointTowards = null;
        return StartCoroutine(Strategize());
    }


    private IEnumerator Strategize() 
    { 

        LinkedList<HexOverlay> path = new LinkedList<HexOverlay>();

        UpdateNavigationValuesAt(this.Origin.myCoords, -1, this.Destination.myCoords);

        //HashSet<HexOverlay> closedList = new HashSet<HexOverlay>();
        Dictionary<Vector3Int, HexOverlay> closedList = new Dictionary<Vector3Int, HexOverlay>();

        List<HexOverlay> pois = new List<HexOverlay>();
        List<HexOverlay> openList = new List<HexOverlay>();
        //Dictionary<Vector3Int, HexOverlay> openList = new Dictionary<Vector3Int, HexOverlay>();
        openList.Add(Origin);
        Debug.Assert(openList.Count >= 1, "The open list has " + openList.Count + " hexes in it");

        //float started = Time.realtimeSinceStartup;
        bool reached = false;
        int maxLoops = 2000;
        int iterations = 0;
        float timeUsed = 0.0f;
        float itterStart = 0.0f;


        HexOverlay chosen = null;
        //Debug.Log(" measuring distance from: " + Origin.myCoords + " To " + Destination.myCoords + " using a Coroutine");

        do
        {
            
            itterStart = Time.realtimeSinceStartup;
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
                    //ignore any hex already in the closed list
                    if (!(closedList.ContainsKey(hex.myCoords)))
                    {


                        int fValue = LookupFValue(hex.myCoords);
                        //int fValue = LookupFValue(pair.Key);
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
                closedList.Add(chosen.myCoords, chosen);

                openList.Remove(chosen);

                //chosen.nav.ChangeDebugTextTo("H = " + LookupHValue(chosen.myCoords)+ "\nG = " + LookupGValue(chosen.myCoords)+ "\nF = " + LookupFValue(chosen.myCoords));


                Debug.Assert(chosen != null, "Chosen Hex was somehow null at: " + iterations + " iteration");
                // For every walkable tile adjacent to the chosen tile:
                foreach (HexOverlay hex in chosen.adjacent)
                {

                    bool isAttackable = Tactician.IsHexDestroyUnitDestination(hex);

                    //  If adjacent tile is in closed list, ignore it
                    // Make an exception for the destination
                    if (!closedList.ContainsKey(hex.myCoords) && (hex.CanIpass(Detatchment) || hex.myCoords == Destination.myCoords))
                    {
                        //Don't add hex to the open list if hex is the destination for attack, but we can't be on the hex we add it from
                        if(!( (Destination == hex && isAttackable) && !chosen.CanIBeOn(Detatchment) ))
                        {
                            //  If adjacent tile is not in the open list, add it to the open list
                            if (!(openList.Contains(hex)))
                            //if (!(openList.ContainsKey(hex.myCoords)))
                            {
                                openList.Add(hex);
                                //openList.Add(hex.myCoords, hex);
                                UpdateNavigationValuesAt(hex.myCoords, LookupGValue(chosen.myCoords), this.Destination.myCoords);
                            }
                            else
                            {
                                //  If adjacent tile is already in the open list - update its g value should the chosen tile be a faster route to it
                                UpdateNavigationValuesAt(hex.myCoords, LookupGValue(chosen.myCoords), this.Destination.myCoords);
                            }
                        }
                        

                    }
                    
                    if (Tactician.Destinations.ContainsKey(hex.myCoords) && !pois.Contains(hex))
                    {
                        if (!(isAttackable) || chosen.CanIBeOn(Detatchment))
                        {
                            pois.Add(hex);

                        }
                        
                        
                    }

                }

                
            }


            timeUsed += Time.realtimeSinceStartup - itterStart;
            TimeSpentThisFrame += Time.realtimeSinceStartup - itterStart;
            iterations++;
            //if (iterations % 1 == 0)
            if (TimeSpentThisFrame >= TimeBudget)
            {

                yield return null;
                //Debug.Log("measurement needed another frame");
                timeUsed = 0.0f;
                TimeSpentThisFrame = 0.0f;
            }
        }
        while ((openList.Count > 0 && !reached) && (iterations < maxLoops));


        //int searchIterations = iterations;
        //At this point either the target has been found and is in the variable "chosen" or every tile has been inspected without finding the target

        Debug.Assert(openList.Count == 0 || chosen == this.Destination, "The Algorithm has failed");
        Debug.Assert(iterations <= maxLoops - 50, "Took way to many iterations");
        if (chosen == this.Destination)
        {
            int FurthestWaypointDist = -3;

            iterations = 0;
            //The algorithm found the target now assemble a path
            while (chosen != this.Origin && iterations < 200)
            {
                int chosenG = LookupGValue(chosen.myCoords);
                bool isAttackable = Tactician.IsHexDestroyUnitDestination(chosen);
                if ((LookupGValue(chosen.myCoords) <= Detatchment.GetMobility()) || (LookupGValue(chosen.myCoords) <= Detatchment.GetMobility() +1 && isAttackable) )
                {
                    if(LookupGValue(chosen.myCoords) > FurthestWaypointDist)
                    {
                        WayPoint = chosen;
                        WaypointTowards = Tactician.Destinations[this.Destination.myCoords];
                        FurthestWaypointDist = chosenG;
                    }
                    
                }

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




            //NavigationData.LogDistance(Detatchment.GetUnitType(), Origin.myCoords, Destination.myCoords, path.Count);

        }
        else
        {
            Debug.Log("Unit leader failed to find a path for " + Detatchment.ToString() + " To " + Destination.myCoords);
            WayPoint = null;
            //this.Distance = -1;
        }

        foreach(HexOverlay hex in pois)
        {
            int closestDistance = int.MaxValue;

            foreach(HexOverlay adj in hex.adjacent)
            {
                if (GValues.ContainsKey(adj.myCoords))
                {
                    closestDistance = Mathf.Min(closestDistance, LookupGValue(adj.myCoords) + 1);
                }


            }

            ObjectiveAssignment oa = new ObjectiveAssignment(Detatchment, Tactician.Destinations[hex.myCoords], closestDistance);

            PossibleObjectives.Add(oa);
        }


        this.Distance = path.Count;
        //Debug.Log("Found " + PossibleObjectives.Count + " potential objectives for " + Detatchment.ToString());

    }



    public void LogObjectivesList()
    {
        string oas = "";

        for(int i = 0; i < this.PossibleObjectives.Count; i++)
        {
            oas += PossibleObjectives[i].ToString() + " , ";
        }

        Debug.Log(oas);
    }


}
