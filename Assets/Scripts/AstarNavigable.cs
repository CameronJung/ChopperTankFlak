using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;
using TMPro;


/*
 * This object stores navigation information about a hex tile for use with A* (Astar) navigation
 */
public class AstarNavigable : MonoBehaviour
{

    public GameObject TextLocation;
    public TextMeshProUGUI debugText;

    public int stepCount { get; private set; }

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

        //debugText.text = cubePosition.ToString();
        debugText.text = gridCoords.ToString();
    }

    public void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
