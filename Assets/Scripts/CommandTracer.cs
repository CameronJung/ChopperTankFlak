using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UniversalConstants;
using static GridHelper;


//When the player selects a unit this class begins following the mouse movements and starts drawing a path
public class CommandTracer : MonoBehaviour
{

    [SerializeField] private Camera cam;

    [SerializeField] private Tilemap map;

    [SerializeField] private GameManager manager;

    private LineRenderer liner;

    private Vector3Int currTilePos = new Vector3Int(0, 0, 100);
    private Vector3Int prevTilePos = new Vector3Int(0, 0, 100);
    public bool drawing { get; private set; } = false;

    //indicates if the tile hovered over displays a valid move for the commandee
    private bool validTile = false;
    
    private Unit commandee = null;
    private Vector3[] points;
    //This is the index of the last order
    private int endPoint = 0;
    

    // Start is called before the first frame update
    void Start()
    {
        liner = gameObject.GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (drawing && Input.mousePresent && manager.WhosTurn() == Faction.PlayerTeam)
        {
            Vector3Int rawPos = map.WorldToCell(cam.ScreenToWorldPoint(Input.mousePosition));
            rawPos.z = 0;

            HandleMouseAtTile(rawPos);

            
            
        }
        
    }



    private void HandleMouseAtTile(Vector3Int mouseTilePos)
    {
        currTilePos = mouseTilePos;


        if (currTilePos != prevTilePos && map.HasTile(currTilePos))
        {
            HexOverlay currHex = map.GetInstantiatedObject(currTilePos).GetComponent<HexOverlay>();
            if (currHex.currState > 0)
            {
                validTile = currHex.currState == HexState.attackable || currHex.GetOccupiedBy() == null;


                liner.enabled = true;
                liner.positionCount = currHex.distanceFrom + 1;

                points[currHex.distanceFrom] = map.GetCellCenterWorld(currTilePos);
                if (!validateLine(currHex.distanceFrom))
                {
                    points = currHex.MakePathToHere(commandee, points);

                }
                liner.SetPositions(points);
                endPoint = currHex.distanceFrom;
            }
            else
            {
                liner.enabled = false;
                validTile = false;
            }
            prevTilePos = currTilePos;
        }
    }



    public void StartDrawingCommand(Unit unit)
    {
        if (unit.GetAllegiance() == manager.WhosTurn() && (unit.myState == UnitState.ready))
        {
            //The most tiles any unit could have will be mobility + 2
            points = new Vector3[unit.GetMobility() + 2];
            points[0] = map.GetCellCenterWorld(unit.myTilePos);
            commandee = unit;
            liner.enabled = true;
            drawing = true;
        }
        
    }

    public void StopDrawingCommand()
    {
        if (drawing)
        {
            //Check if the click would be a valid command, if it is than execut it
            
            //SendCommand();
            drawing = false;
            commandee = null;
            liner.enabled = false;
        }
        
    }

    private bool validateLine(int endDist)
    {
        bool valid = true;
        bool prevWasAttack = false;

        for(int idx = endDist; idx > 0; idx--)
        {
            valid = valid && (Vector3.SqrMagnitude(points[idx - 1] - points[idx]) < HEXDISTANCESQUARED);
            HexOverlay hex = map.GetInstantiatedObject(map.WorldToCell(points[idx])).GetComponent<HexOverlay>();

            if (prevWasAttack && valid)
            {
                //If the previous tile was an attack then that means the unit stops at this tile and thus needs to be empty
                valid = valid && hex.GetOccupiedBy() == null;
            }

            if(idx < endDist && valid)
            {
                //We shouldn't draw paths through enemy units
                valid = valid && hex.currState != HexState.attackable;
            }

            prevWasAttack = hex.currState == HexState.attackable;
        }

        return valid;
    }


    //This function translates the points array into a stack of orders that will be sent to the selected unit
    public void SendCommand()
    {
        if (validTile) {
            Stack<Order> orders = new Stack<Order>();

            //Check if the last order is an attack
            bool attacks = map.GetInstantiatedObject(currTilePos).GetComponent<HexOverlay>().currState == HexState.attackable;

            for (int idx = endPoint; idx >= 1; idx--)
            {
                if(idx == endPoint && attacks)
                {
                    orders.Push(new AttackOrder(points[idx - 1], points[idx], this.commandee));
                }
                else
                {
                    orders.Push(new MoveOrder(points[idx - 1], points[idx], this.commandee));
                }
                
            }
            endPoint = 0;
            commandee.RecieveOrders(orders);
        }
    }




    //This method sets up the command tracer as though the mouse moved to a specific tile
    //This allows the code to be used by the AI
    public void SpoofSendCommand(Vector3Int fakeMouseTile)
    {
        prevTilePos = new Vector3Int(0, 0, -100);
        HandleMouseAtTile(fakeMouseTile);
        SendCommand();
    }
}
