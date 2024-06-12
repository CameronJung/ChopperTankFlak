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
    [SerializeField] private CommandTracer commander;
    [SerializeField] private GameObject busyIndicator;
    [SerializeField] private GameManager manager;
    [SerializeField] private MapPanel mapPanel;

    //Initialized with a nonsense coordinates
    private Vector3Int selectionPos = new Vector3Int(0,0,100);

    //The current number of touches detected
    private int Touches = 0;
    //The number of touches in the map panel that need to be tracked
    private int NumberMapTouches = 0;


    private Dictionary<int, Touch> TrackedMapTouches;
    private List<int> TouchIDs;

    //This value blocks the processing of clicks
    private bool blocked = false;



    //A record of where the mouse is
    // this is used in part to distinguish touches from an actual mouse
    private Vector3 MausPosPrev = new Vector3(0.0f, 0.0f, 100f);
    private Vector3 MapDragPoint = new Vector3(0.0f, 0.0f, 100f);
    private Vector3 PrevDragPoint = new Vector3(0.0f, 0.0f, 100f);


    //This value is used to determine if the mouse position is from an actual mouse
    private bool RealMaus = true;

    
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Is touch supported? " + Input.touchSupported);
        TrackedMapTouches = new Dictionary<int, Touch>();
        TouchIDs = new List<int>();
    }

    // Update is called once per frame
    void Update()
    {
        
        

        if (!blocked)
        {
            if (Input.GetMouseButtonDown(0))
            {


                Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int tilePos = gameBoard.WorldToCell(mousePos);
                tilePos.z = 0;

                if (commander.drawing)
                {
                    commander.SendCommand();
                }

                selecter.HandleNewSelection(tilePos);

            }
            else if (Input.GetMouseButtonDown(1))
            {
                selecter.HandleDeselect();

            }

            if(Input.touchCount > 0)
            {
                RealMaus = false;
                /*
                if (Input.touchCount != Touches)
                {
                    Touches = Input.touchCount;
                    Debug.Log("There are " + Touches + " point(s) of contact.");

                    NumberMapTouches = 0;
                    foreach (Touch touch in Input.touches)
                    {
                        if (mapPanel.IsPointOnMap(touch.position))
                        {
                            NumberMapTouches += 1;
                            TrackedMapTouches.Add(touch.fingerId, touch);
                            TouchIDs.Add(touch.fingerId);

                        }
                        Debug.Log("A touch was at: " + touch.position);
                    }
                    Debug.Log(NumberMapTouches + " touch(es) are on the map");
                }
                */

                TrackedMapTouches.Clear();
                TouchIDs.Clear();

                foreach (Touch touch in Input.touches)
                {
                    if (mapPanel.IsPointOnMap(touch.position))
                    {
                        NumberMapTouches += 1;
                        TrackedMapTouches[touch.fingerId] = touch;
                        TouchIDs.Add(touch.fingerId);

                    }
                    //Debug.Log("A touch was at: " + touch.position);
                }

                /*
                foreach (int id in TouchIDs)
                {
                    switch (TrackedMapTouches[id].phase)
                    {
                        case TouchPhase.Ended:

                            TrackedMapTouches.Remove(id);
                            TouchIDs.Remove(id);
                            break;
                        case TouchPhase.Canceled:
                            TrackedMapTouches.Remove(id);
                            TouchIDs.Remove(id);
                            break;
                    }
                }
                */

                //When there is 1 touch{
                
                //If a unit is selected{

                //}If no unit is selected{



                //}

                //When there are 2 touches{
                //Deselect

                if(TrackedMapTouches.Count == 2)
                {
                    selecter.HandleDeselect();

                    //When we start panning the map around initialize the previous point
                    if ( TrackedMapTouches[TouchIDs[0]].phase == TouchPhase.Began || TrackedMapTouches[TouchIDs[1]].phase == TouchPhase.Began)
                    {
                        PrevDragPoint = ComputeDragPoint(TrackedMapTouches[TouchIDs[0]], TrackedMapTouches[TouchIDs[1]]);
                    }
                    
                    MapDragPoint = ComputeDragPoint(TrackedMapTouches[TouchIDs[0]], TrackedMapTouches[TouchIDs[1]]);

                    mapPanel.PanMapByDrag(MapDragPoint - PrevDragPoint);

                    PrevDragPoint = MapDragPoint;
                }

                //}

            }
            else
            {

                Touches = 0;
                if (IsMouseReal())
                {
                    //If there are no touches than the mouse detected is an actual mouse not touches
                    mapPanel.MouseAtPosition(Input.mousePosition);
                }
            }

        }

        MausPosPrev = Input.mousePosition;
    }


    /*
     * This function is used to update the variable "RealMaus" it will also return the new value
     * This function should be the only means of setting "RealMause" to true, the variable can be set false whenever
     * Returns true if their is reason to expect that input is from an actual mouse
     */
    private bool IsMouseReal()
    {
        //If the mouse has moved but their are no touches than the movement must be from a real mouse
        RealMaus = RealMaus || (Input.mousePosition != MausPosPrev && Touches == 0);
        return RealMaus;
    }


    private Vector3 ComputeDragPoint(Touch a, Touch b)
    {
        return (a.position + b.position) * 0.5f;
    }
    


    public void BlockClicks()
    {
        blocked = true;
        busyIndicator.SetActive(true);
    }

    public void AllowClicks()
    {
        //Do not unblock if the battle is over
        if (!manager.IsBattleOver())
        {
            blocked = false;
            busyIndicator.SetActive(false);
        }
        
    }
}
