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
    [SerializeField] private SelectionManager selecter;

    //Initialized with a nonsense coordinates
    private Vector3Int selectionPos = new Vector3Int(0,0,100);
    
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

            selecter.HandleNewSelection(tilePos);

            
            
        }
        else if (Input.GetMouseButtonDown(1))
        {
            selecter.HandleDeselect();
            
        }
    }

}
