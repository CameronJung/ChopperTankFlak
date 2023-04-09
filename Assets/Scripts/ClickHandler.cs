using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ClickHandler : MonoBehaviour
{
    [SerializeField] private GridLayout gameBoard;
    [SerializeField] private Tilemap map;

    [SerializeField] private Camera cam;
    [SerializeField] private ReconPanel recon;
    [SerializeField] private GameObject cursor;
    
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            
            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int tilePos = gameBoard.WorldToCell(mousePos);
            tilePos.z = 0;

            Debug.Log("Mouse Clicked on hex at: " + tilePos);
            HexOverlay hex = map.GetInstantiatedObject(tilePos).GetComponent<HexOverlay>();


            if(hex.GetOccupiedBy() != null)
            {
                recon.DisplayIntelAbout(hex.GetOccupiedBy());
            }
            else
            {
                recon.DisplayIntelAbout(map.GetTile<TerrainTile>(tilePos));
            }
            cursor.SetActive(true);
            cursor.transform.position = map.CellToWorld(tilePos);
            
        }
        else if (Input.GetMouseButtonDown(1))
        {
            cursor.SetActive(false);
            recon.DisplayNone();
        }
    }

}
