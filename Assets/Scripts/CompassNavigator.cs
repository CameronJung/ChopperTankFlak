using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UniversalConstants;
using static GridHelper;

public class CompassNavigator : MonoBehaviour
{
    private int Distance = -1;
    [SerializeField] private Tilemap Map;
    private HexOverlay Destination;
    private HexOverlay Origin;
    private Unit Agent;
    private float TimeBudget;

    private float TimeSpentThisFrame;

    private GlobalNavigationData NavigationData;

    public bool Complete { get; private set; }


    //All G and H values will be stored in these dictionaries to avoid synchronisity issues
    private Dictionary<Vector3Int, int> GValues;
    private Dictionary<Vector3Int, int> HValues;

    public HashSet<Vector3Int> DiscoveredDestinations { get; private set; } = new HashSet<Vector3Int>();
    public Vector3Int Waypoint { get; private set; }



    public void Start()
    {

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
        //return g + h;
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

}
