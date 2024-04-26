using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;

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

    public static Vector3Int GetAstarCoordsOf(Vector3Int gridCoords)
    {
        Vector3Int cubePosition = new Vector3Int(gridCoords.x, gridCoords.y, 0);

        cubePosition.z = gridCoords.y / -2 + gridCoords.x;
        if (gridCoords.y < 0)
        {
            cubePosition.z -= gridCoords.y % 2;
        }


        cubePosition.x = -(cubePosition.y + cubePosition.z);

        return cubePosition;
    }

    /*
     * This method returns the number of tiles between two grid coordinates
     * Note that this distance is as the crow flies (which is apparently better than helicopters), it does not account for obstacles
     */
    public static int CalcTilesBetweenGridCoords(Vector3Int origin, Vector3Int destination)
    {
        int dist = 0;

        Vector3Int posA = GetAstarCoordsOf(origin);
        Vector3Int posB = GetAstarCoordsOf(destination);

        dist = CalcTilesBetweenAstarCoords(posA, posB);

        return dist;
    }

    public static int CalcTilesBetweenAstarCoords(Vector3Int posA, Vector3Int posB)
    {
        int distance = 0;

        Vector3Int diff = (posA - posB);

        diff.x = Mathf.Abs(diff.x);
        diff.y = Mathf.Abs(diff.y);
        diff.z = Mathf.Abs(diff.z);

        distance = Mathf.Max(diff.x, diff.y, diff.z);

        return distance;
    }


    /*
     * This method returns true if the vector parameters are at an appropriate distance to be tiles next to eachother
     */
    public static bool CoarseAdjacencyTest(Vector3 a, Vector3 b)
    {
        return (Vector3.SqrMagnitude(a - b) < HEXDISTANCESQUARED && (Vector3.SqrMagnitude(a - b) > 0.1f));
    }

}
