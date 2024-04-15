using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UniversalConstants;
using static GridHelper;
using TMPro;


/*
 * This object stores navigation information about a hex tile for use with A* (Astar) navigation
 */
public class AstarNavigable : MonoBehaviour
{

    public GameObject TextLocation;
    public TextMeshProUGUI DebugText;
    
    private Tilemap Map;

    //The HexOverlay that this navigable object is associated with.
    public HexOverlay OnHex { get; private set; }

    public int stepCount { get; private set; }
    public int GValue { get; private set; }
    public int HValue { get; private set; }



    //The cube grid cooridantes of the tile
    public Vector3Int cubePosition;

    public void CalculateCoords(Vector3Int gridCoords)
    {
        
        cubePosition = new Vector3Int(gridCoords.x, gridCoords.y, 0);

        cubePosition.z = gridCoords.y/-2 + gridCoords.x;
        if(gridCoords.y < 0)
        {
            cubePosition.z -= gridCoords.y % 2;
        }


        cubePosition.x = -(cubePosition.y + cubePosition.z);

        //DebugText.text = cubePosition.ToString();
        DebugText.text = gridCoords.ToString();
    }

    public void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        this.GValue = int.MaxValue;
        this.HValue = int.MaxValue;
        this.Map = Map = GameObject.Find(UniversalConstants.MAPPATH).GetComponent<Tilemap>();

        OnHex = Map.GetInstantiatedObject(Map.WorldToCell(gameObject.transform.position)).GetComponent<HexOverlay>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*
     * MeasureTrueDistance
     * 
     * This Method returns the number of tiles it will take "unit" to get to "target" from this position
     */
    public int MeasureTrueDistance(Unit unit, HexOverlay target)
    {
        LinkedList<HexOverlay> path = new LinkedList<HexOverlay>();
        this.HValue = OnHex.HexDistTo(target);
        this.GValue = 0;
        List<HexOverlay> closedList = new List<HexOverlay>();
        List<HexOverlay> openList = new List<HexOverlay>();
        List<HexOverlay> possibilities = new List<HexOverlay>();
        openList.Add(this.OnHex);
        this.OnHex.nav.UpdateNavigationParameters(0, target);

        int steps = 0;
        float started = Time.realtimeSinceStartup;
        bool reached = false;
        int maxLoops = 10000;
        int iterations = 0;

        HexOverlay chosen = null;
        Debug.Log(" measuring distance from: " + this.OnHex.myCoords + " To " + target.myCoords);

        //this.ContinueMeasuring(unit, target, ref path, ref openList, ref closedList, steps + 1);
        do
        {
            possibilities.Clear();

            chosen = null;
            int lowestF = int.MaxValue;




            // Take a tile from the openList
            foreach(HexOverlay hex in openList) 
            {

                //If we find the target choose it outright and stop looking
                if (hex == target)
                {
                    chosen = hex;
                    reached = true;
                }
                else if (!reached) 
                {
                    if (!(closedList.Contains(hex)))
                    {

                        possibilities.Add(hex);
                        if (hex.nav.GetFValue() <= lowestF)
                        {
                            if (hex.nav.GetFValue() == lowestF)
                            {
                                if (chosen != null)
                                {
                                    if (chosen.nav.GValue < hex.nav.GValue)
                                    {
                                        chosen = hex;
                                    }
                                }
                            }
                            else
                            {
                                chosen = hex;
                                lowestF = hex.nav.GetFValue();
                            }
                        }
                    }
                }
                
            }

            if(!reached)
            {
                // Add chosen tile to closed list
                closedList.Add(chosen);
                possibilities.Remove(chosen);
                Debug.Assert(chosen != null, "Chosen Hex was somehow null at: " + iterations + " iteration");
                // For every walkable tile adjacent to the chosen tile:
                foreach(HexOverlay hex in chosen.adjacent)
                {
                    //  If adjacent tile is in closed list, ignore it
                    if(!closedList.Contains(hex) && hex.CheckTerrainFor(unit))
                    {
                        //  If adjacent tile is not in the open list, add it to the open list
                        if (!(openList.Contains(hex)))
                        {
                            openList.Add(hex);
                            hex.nav.UpdateNavigationParameters(chosen.nav.GValue, target);
                        }
                        else
                        {
                            //  If adjacent tile is already in the open list - update its g value should the chosen tile be a faster route to it
                            hex.nav.UpdateNavigationParameters(chosen.nav.GValue, target);
                        }

                    }

                }

                //
            }




            iterations++;
        }
        while ((possibilities.Count > 0 || !reached) && (iterations < maxLoops));

        //At this point either the target has been found and is in the variable "chosen" or every tile has been inspected without finding the target

        Debug.Assert(possibilities.Count == 0 || chosen == target, "The Algorithm has failed");

        if(chosen == target)
        {
            //The algorithm found the target now assemble a path
            while(chosen.nav != this)
            {
                
                int lowestG = int.MaxValue;
                foreach (HexOverlay hex in chosen.adjacent)
                {
                    if(hex.nav.GValue < lowestG)
                    {
                        lowestG = hex.nav.GValue;
                        chosen = hex;
                    }
                }
                path.AddFirst(chosen);
            }
        }
        else
        {
            Debug.Log("A* algorithm failed to find a path for " + unit.GetUnitType()+ " " + unit.gameObject.name);
        }


        Debug.Log("Calculation took: " + (Time.realtimeSinceStartup - started) + " time to measure a distance of " + path.Count + " tiles away.");
        foreach(HexOverlay hex in openList)
        {
            hex.nav.Reset();
        }
        return path.Count;
    }



    public int MeasureTrueDistance(Unit unit, Vector3Int goal)
    {
        LinkedList<HexOverlay> path = new LinkedList<HexOverlay>();
        HexOverlay target = Map.GetInstantiatedObject(goal).GetComponent(typeof(HexOverlay)) as HexOverlay;
        this.HValue = OnHex.HexDistTo(target);
        this.GValue = 0;
        List<HexOverlay> closedList = new List<HexOverlay>();
        List<HexOverlay> openList = new List<HexOverlay>();
        List<HexOverlay> possibilities = new List<HexOverlay>();
        openList.Add(this.OnHex);
        this.OnHex.nav.UpdateNavigationParameters(0, target);

        int steps = 0;
        float started = Time.realtimeSinceStartup;
        bool reached = false;
        int maxLoops = 10000;
        int iterations = 0;

        HexOverlay chosen = null;
        Debug.Log(" measuring distance from: " + this.OnHex.myCoords + " To " + target.myCoords);

        //this.ContinueMeasuring(unit, target, ref path, ref openList, ref closedList, steps + 1);
        do
        {
            possibilities.Clear();

            chosen = null;
            int lowestF = int.MaxValue;




            // Take a tile from the openList
            foreach (HexOverlay hex in openList)
            {

                //If we find the target choose it outright and stop looking
                if (hex == target)
                {
                    chosen = hex;
                    reached = true;
                }
                else if (!reached)
                {
                    if (!(closedList.Contains(hex)))
                    {

                        possibilities.Add(hex);
                        if (hex.nav.GetFValue() <= lowestF)
                        {
                            if (hex.nav.GetFValue() == lowestF)
                            {
                                if (chosen != null)
                                {
                                    if (chosen.nav.GValue < hex.nav.GValue)
                                    {
                                        chosen = hex;
                                    }
                                }
                            }
                            else
                            {
                                chosen = hex;
                                lowestF = hex.nav.GetFValue();
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
                    if (!closedList.Contains(hex) && hex.CheckTerrainFor(unit))
                    {
                        //  If adjacent tile is not in the open list, add it to the open list
                        if (!(openList.Contains(hex)))
                        {
                            openList.Add(hex);
                            hex.nav.UpdateNavigationParameters(chosen.nav.GValue, target);
                        }
                        else
                        {
                            //  If adjacent tile is already in the open list - update its g value should the chosen tile be a faster route to it
                            hex.nav.UpdateNavigationParameters(chosen.nav.GValue, target);
                        }

                    }

                }

                //
            }




            iterations++;
        }
        while ((possibilities.Count > 0 || !reached) && (iterations < maxLoops));

        //At this point either the target has been found and is in the variable "chosen" or every tile has been inspected without finding the target

        Debug.Assert(possibilities.Count == 0 || chosen == target, "The Algorithm has failed");

        if (chosen == target)
        {
            //The algorithm found the target now assemble a path
            while (chosen.nav != this)
            {

                int lowestG = int.MaxValue;
                foreach (HexOverlay hex in chosen.adjacent)
                {
                    if (hex.nav.GValue < lowestG)
                    {
                        lowestG = hex.nav.GValue;
                        chosen = hex;
                    }
                }
                path.AddFirst(chosen);
            }
        }
        else
        {
            Debug.Log("A* algorithm failed to find a path for " + unit.GetUnitType() + " " + unit.gameObject.name);
        }


        Debug.Log("Calculation took: " + (Time.realtimeSinceStartup - started) + " time to measure a distance of " + path.Count + " tiles away.");
        foreach (HexOverlay hex in openList)
        {
            hex.nav.Reset();
        }
        return path.Count;
    }



    public void UpdateNavigationParameters(int steps, HexOverlay target)
    {
        if(this.GValue > steps + 1)
        {
            this.GValue = steps + 1;
        }

        this.HValue = this.OnHex.HexDistTo(target);
    }



    public void Reset()
    {
        GValue = int.MaxValue;
        HValue = int.MaxValue;
    }

    public int GetFValue()
    {
        return this.GValue + this.HValue;
    }


    /*\
     * GetAdjacentLowestH
     * this method returns a HexOverlay adjacent to this hex that can be treversed by "unit"
     * if no adjacent hex Qualifies this method returns null
    \*/
    public HexOverlay GetAdjacentLowestH(HexOverlay destination, Unit unit, ref LinkedList<HexOverlay> path)
    {
        
        int lowestH = int.MaxValue;
        List<HexOverlay> adjacent = OnHex.adjacent;
        HexOverlay neighbor = null;
        int dist = 0;
        foreach( HexOverlay hex in adjacent){
            if (hex.CanIpass(unit))
            {
                if (!(path.Contains(hex)))
                {
                    dist = hex.HexDistTo(destination);
                    if (dist <= lowestH)
                    {
                        neighbor = hex;
                        lowestH = dist;
                    }
                }
                
            }
            

        }

        return neighbor;
    }







}
