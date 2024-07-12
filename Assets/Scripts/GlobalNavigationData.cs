using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;


//This class/ component is responsible for maintaining a record of measured distances on the game board
public class GlobalNavigationData : MonoBehaviour
{

    private Dictionary<UnitType, Dictionary<Vector3Int, Dictionary<Vector3Int, int>>> Navigation;
    private Dictionary<int, int> HashedNavigation;

    // Start is called before the first frame update
    void Start()
    {
        Navigation = new Dictionary<UnitType, Dictionary<Vector3Int, Dictionary<Vector3Int, int>>>();
        HashedNavigation = new Dictionary<int, int>();
    }


    //This method logs the distance into the Navigation Data structure, if a distance is already present than the shortest distance is kept
    public void LogDistance(UnitType unit, Vector3Int origin, Vector3Int destination, int distance)
    {
        int code = HashMapDistance(unit, origin, destination);
        int reverseCode = HashMapDistance(unit, destination, origin);

        if(this.HasDistance(code))
        {
            HashedNavigation[code] = Mathf.Min(HashedNavigation[code], distance);
            HashedNavigation[reverseCode] = HashedNavigation[code];
            //Navigation[unit][origin][destination] = Mathf.Min(Navigation[unit][origin][destination], distance);
        }
        else
        {
            //Debug.Log("adding value for key: " + unit + " Origin: " + origin + " destination: " + destination);

            HashedNavigation.TryAdd(code, distance);
            if(reverseCode != code)
            HashedNavigation.TryAdd(reverseCode, distance);

            //Navigation[unit].TryGetValue(origin).TryAdd(destination, distance);
        }
        
    }


    //returns true if a distance for the given unit has been recorded from origin to destination
    public bool HasDistance(UnitType unit, Vector3Int origin, Vector3Int destination)
    {
        //bool logged = false;
        int code = HashMapDistance(unit, origin, destination);
        /*
        if (Navigation[unit].ContainsKey(origin))
        {
            logged = (Navigation[unit][origin].ContainsKey(destination));
        }
        */


        return HashedNavigation.ContainsKey(code);
    }


    private bool HasDistance(int code)
    {
        return HashedNavigation.ContainsKey(code);
    }


    //HasDistance should be called before this method
    public int GetDistance(UnitType unit, Vector3Int origin, Vector3Int destination)
    {
        int code = HashMapDistance(unit, origin, destination);
        return HashedNavigation[code];
    }



    /*
     * HashCoordinates
     * 
     * In order to avoid a data structure so ugly that it would turn Medusa to stone it was neccesary to compress the origin and destination of a measured distance
     * into a single data type. !NOTE! this limits the size of a gameboard to 99 x 99
     * This function takes to Vector3Ints and returns an int value representing the relevent data
     * 
     * The first digit indicates the unit type, it is one more than the UnitType enumeration value (see UniversalConstants.cs)
     * The last 8 digits are divided into 4 sets of two sequential digits each being 50 more than coordinate they represent
     * Effectively The int will be a 9 digit number ### ### ### that can be read as where Ox means x coordinate of origin and Dy means y coordinate of destination:
     * 
     * 
     */
    private int HashMapDistance(UnitType unit ,Vector3Int orig, Vector3Int dest)
    {
        int code = 50505050;


        int ut = (int)unit * 100000000;
        int dY = dest.y * 1;
        int dX = dest.x * 100;
        int oY = orig.y * 10000;
        int oX = orig.x * 1000000;

        code += ut + oX + oY + dX + dY;

        return code;
    }
}
