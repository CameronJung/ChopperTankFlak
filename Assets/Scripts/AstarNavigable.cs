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

        DebugText.gameObject.SetActive(Application.isEditor);
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

    
    

    public void ChangeDebugTextTo(string words)
    {
        if (Application.isEditor)
        {
            DebugText.text = words;
        }
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

        if (Application.isEditor)
        {
            DebugText.text = (OnHex.myCoords).ToString();
        }
        
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
