using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SelectionManager : MonoBehaviour
{
    [SerializeField] private Tilemap map;
    [SerializeField] private ReconPanel recon;
    [SerializeField] private GameObject cursor;
    [SerializeField] private CommandTracer commander;

    public Unit selectedUnit { get; private set; }


    private List<HexOverlay> affectedHexes;
    private Vector3Int selectedPos = new Vector3Int();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HandleNewSelection(Vector3Int tilePos)
    {

        GameObject obj = map.GetInstantiatedObject(tilePos);


        if(obj != null)
        {
            HexOverlay hex = obj.GetComponent(typeof(HexOverlay)) as HexOverlay;


            if (tilePos == selectedPos)
            {
                //If we have clicked on the same tile display the other type of data
                if (selectedUnit != null)
                {
                    HandleUnitDeselected();
                    recon.DisplayIntelAbout(map.GetTile<TerrainTile>(tilePos), tilePos);
                }
                else if (hex.GetOccupiedBy() != null)
                {
                    HandleUnitSelected();
                    recon.DisplayIntelAbout(hex.GetOccupiedBy(), tilePos);
                }
            }
            //Otherwise send the unit if one is present
            else
            {
                selectedPos = tilePos;
                if (hex.GetOccupiedBy() != null)
                {
                    HandleUnitDeselected();
                    recon.DisplayIntelAbout(hex.GetOccupiedBy(), tilePos);
                    HandleUnitSelected();
                }
                else
                {
                    HandleUnitDeselected();
                    recon.DisplayIntelAbout(map.GetTile<TerrainTile>(tilePos), tilePos);
                }
            }
            cursor.SetActive(true);
            cursor.transform.position = map.CellToWorld(tilePos);
        }
        
    }

    public void HandleDeselect()
    {
        cursor.SetActive(false);
        if(selectedUnit != null)
        {
            HandleUnitDeselected();
        }
        recon.DisplayNone();
    }

    private void HandleUnitSelected()
    {
        selectedUnit = map.GetInstantiatedObject(selectedPos).GetComponent<HexOverlay>().GetOccupiedBy();
        affectedHexes = selectedUnit.OnSelected();
        commander.StartDrawingCommand(selectedUnit);
    }



    private void HandleUnitDeselected()
    {
        if(selectedUnit != null)
        {
            commander.StopDrawingCommand();
            //reset tiles
            foreach (HexOverlay hex in affectedHexes)
            {
                hex.SetBlank();
            }
            selectedUnit = null;
        }
        
    }

    public bool IsUnitSelected()
    {
        return selectedUnit != null;
    }



    //Returns a list of hexes the selected unit can move to
    //this is mostly used by the AI
    public List<HexOverlay> GetPossibilities()
    {
        Debug.Assert(selectedUnit != null, "There is no unit selected");
        List<HexOverlay> possibilities = new List<HexOverlay>();

        bool viable;
        foreach(HexOverlay hex in affectedHexes)
        {
            viable = false;
            
            if (hex.CanIpass(selectedUnit))
            {
                Unit occupier = hex.GetOccupiedBy();
                if (occupier != null)
                {
                    //Throw hexes that are occupied by other allied units
                    viable = (occupier.GetAllegiance() != selectedUnit.GetAllegiance()
                        || occupier == selectedUnit);
                }
                else
                {
                    viable = true;
                }

            }

            if (viable)
            {
                possibilities.Add(hex);
            }
            
        }


        return possibilities;
    }
    
}
