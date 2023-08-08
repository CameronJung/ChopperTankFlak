using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//This class is responsible for maintaining information related to the map's geography and player movements
public class AIIntelHandler : MonoBehaviour
{

    [SerializeField] private Vector3Int computerBaseLoc;
    [SerializeField] private Vector3Int playerBaseLoc;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    /*                  ACCESSORS                   */

    public Vector3Int GetComputerBaseLoc()
    {
        return this.computerBaseLoc;
    }

    public Vector3Int GetPlayerBaseLoc()
    {
        return this.playerBaseLoc;
    }


}
