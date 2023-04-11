using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SelectionManager : MonoBehaviour
{
    [SerializeField] private Tilemap map;
    [SerializeField] private ReconPanel recon;
    [SerializeField] private GameObject cursor;

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
        Debug.Log("Mouse Clicked on hex at: " + tilePos + " distance from unit was: " + map.GetInstantiatedObject(tilePos).GetComponent<HexOverlay>().distanceFrom);

        HexOverlay hex = map.GetInstantiatedObject(tilePos).GetComponent<HexOverlay>();

        if (tilePos == selectedPos)
        {
            //If we have clicked on the same tile display the other type of data
            if (recon.unitSelected)
            {
                HandleUnitDeselected();
                recon.DisplayIntelAbout(map.GetTile<TerrainTile>(tilePos), tilePos);
            }
            else
            {
                HandleUnitSelected();
                recon.DisplayIntelAbout(hex.GetOccupiedBy(), tilePos);
            }
        }
        //Otherwise send the unit if one is present
        else{
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
    }

    private void HandleUnitDeselected()
    {
        //reset tiles
        foreach(HexOverlay hex in affectedHexes)
        {
            hex.SetBlank();
        }
        selectedUnit = null;
    }

    
}
