using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GridHelper
{
    //This class contains helper methods that assist in navigating the hexagonal game board

    public static Vector3Int GetUp(Vector3Int from)
    {
        Vector3Int destination = new Vector3Int(from.x + 1, from.y, from.z);
        return destination;
    }

    public static Vector3Int GetDown(Vector3Int from)
    {
        Vector3Int destination = new Vector3Int(from.x -1, from.y, from.z);
        return destination;
    }

    public static Vector3Int GetUpRight(Vector3Int from)
    {
        Vector3Int destination = new Vector3Int(from.x + Mathf.Abs(from.y % 2), from.y +1, from.z);
        return destination;
    }

    public static Vector3Int GetUpLeft(Vector3Int from)
    {
        Vector3Int destination = new Vector3Int(from.x + Mathf.Abs(from.y % 2), from.y - 1, from.z);
        return destination;
    }

    public static Vector3Int GetDownLeft(Vector3Int from)
    {
        //The rule for this direction is that if y is even than x is also decreased
        int conditional = 1 - Mathf.Abs(from.y % 2);
        Vector3Int destination = new Vector3Int(from.x - conditional, from.y - 1, from.z);
        return destination;
    }

    public static Vector3Int GetDownRight(Vector3Int from)
    {
        //The rule for this direction is that if y is even than x is also decreased
        int conditional = 1 - Mathf.Abs(from.y % 2);
        Vector3Int destination = new Vector3Int(from.x - conditional, from.y + 1, from.z);
        return destination;
    }

}
