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
     * This Method returns the number of tiles it will take "unit" to get to "destination" from this position
     */
    public int MeasureTrueDistance(Unit unit, HexOverlay target)
    {
        LinkedList<HexOverlay> path = new LinkedList<HexOverlay>();
        this.HValue = OnHex.HexDistTo(target);
        this.GValue = 0;
        path.AddLast(OnHex);
        List<HexOverlay> visited = new List<HexOverlay>();
        visited.Add(this.OnHex);

        int steps = 0;

        this.ContinueMeasuring(unit, target, ref path, ref visited, steps + 1);

        
        

        /*
        HexOverlay next = this.GetAdjacentLowestH(target, unit, ref path);
        if(next != null)
        {
            next.nav.ContinueMeasuring(unit, target, ref path, ref visited, steps +1);


        }
        else
        {
            //This part of the code shouldn't execute
            Debug.Log("!Error! Somehow ended up with no neigbors");
        }
        */
        return path.Count;
    }

    public int ContinueMeasuring(Unit unit, HexOverlay target, ref LinkedList<HexOverlay> path, ref List<HexOverlay> visited, int steps)
    {
        visited.Add(OnHex);
        GValue = steps;
        HValue = OnHex.HexDistTo(target);


        foreach (HexOverlay hex in OnHex.adjacent)
        {
            if (!(visited.Contains(hex)))
            {
                if (!(path.Contains(hex)))
                {

                }
            }
        }

        return steps;
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
