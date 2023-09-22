using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//This class is responsible for maintaining information related to the map's geography and player movements
public class AIIntelHandler : MonoBehaviour
{

    [SerializeField] private Vector3Int computerBaseLoc;
    [SerializeField] private Vector3Int playerBaseLoc;

    private List<HexIntel> affectedIntel;

    // Start is called before the first frame update

    private void Awake()
    {
        affectedIntel = new List<HexIntel>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void WipeOldData()
    {
        foreach (HexIntel intel in affectedIntel)
        {
            intel.WipeIntel();
        }
        affectedIntel.Clear();
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


    public void ReportAffectedHex(HexIntel intel)
    {
        if (!affectedIntel.Contains(intel))
        {
            affectedIntel.Add(intel);
        }
        
    }

    

    


}
