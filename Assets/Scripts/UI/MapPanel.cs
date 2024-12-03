using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;

public class MapPanel : MonoBehaviour
{
    // The distance between two hexes on the map in real world units
    private const float HEXHEIGHT = 0.86f;
    private const float HEXWIDTH = 0.75f;

    private const int PIXDISTX = 48;
    private const int PIXDISTY = 56;

    //The width of the border where the camera should try to move
    private const float BORDER = 48.0f;


    [SerializeField] private GameObject camObject;
    private Camera cam;

    private ControlsManager Controller;

    //The playable area of the game board
    [SerializeField] private Vector2Int mapSize = Vector2Int.zero;
    [SerializeField] private bool IsAsymetrical = false;

    //These numbers indicate the size of the map as the vertical and hoeizontal distances to the furthest traversable tile from the origin
    //The distanses are in the order: East, West, North, South
    [SerializeField] private int[] CardinalDistances = { 0,0,0,0 };
    [SerializeField] private RectTransform Deadzone;

    [SerializeField] private float scrollSpeed = 1.25f;

    private Rect mapRect;
    private Rect mapCentreRect;
    private Vector2 mapCentre;

    

    private Vector2 mapDimensions;

    private Rect MapFrameRect;

    private Vector2 mapPos;
    private Vector2 MapWorldSize;
    private Vector2 MapViewSize;

    //The middle of the map in world coordinates
    private Vector2 WorldCentre;

    //The distance the camera can move without going over the edge of the map
    private float moveableHeight;

    private float movableWidth;

    private bool Maus = false;

    private RectTransform MapTransform;

    private Vector3 ViewScale;

    void Start()
    {

        Controller = GameObject.Find(MANAGERPATH).GetComponent<ControlsManager>();

        MapFrameRect = gameObject.GetComponent<RectTransform>().rect;
        MapTransform = gameObject.GetComponent<RectTransform>();

        if (IsAsymetrical)
        {

            MapWorldSize = new Vector2(HEXWIDTHWORLDUNITS * (3 + CardinalDistances[0] + CardinalDistances[1]), HEXHEIGHTWORLDUNITS * (1 + CardinalDistances[2] + CardinalDistances[3]));

            mapDimensions = new Vector2((3 + CardinalDistances[0] + CardinalDistances[1]) * HEXWIDTH, (1 + CardinalDistances[2] + CardinalDistances[3]) * HEXHEIGHT);

            WorldCentre = new Vector2((CardinalDistances[0] - CardinalDistances[1]) * HEXWIDTH * 0.5f,
                (CardinalDistances[2] - CardinalDistances[3]) * HEXHEIGHT * 0.5f);

            Debug.Log("The world centre is " + WorldCentre);
        }
        else
        {
            MapWorldSize = new Vector2(UniversalConstants.HEXWIDTHWORLDUNITS * mapSize.x, UniversalConstants.HEXHEIGHTWORLDUNITS * mapSize.y);

            mapDimensions = new Vector2(mapSize.x * HEXWIDTH, mapSize.y * HEXHEIGHT);

            WorldCentre = new Vector2(0.0f, 0.0f);
        }

        //The real world size of the map



        mapCentre = Deadzone.position;
        cam = camObject.GetComponent<Camera>();

        MapViewSize = new Vector2(cam.orthographicSize * cam.aspect * 2, cam.orthographicSize * 2);


        if (MapViewSize.x > MapWorldSize.x)
        {
            cam.orthographicSize = (MapWorldSize.x)/(cam.aspect*2.0f);
            MapViewSize = new Vector2(cam.orthographicSize * cam.aspect * 2, cam.orthographicSize * 2);
        }

        mapRect = cam.pixelRect;
        
        
        mapCentreRect = new Rect(mapRect.xMin + BORDER, mapRect.yMin + BORDER, mapRect.width - 2 * BORDER, mapRect.height - 2 * BORDER);
        

        //ViewScale = new Vector3((float)cam.pixelWidth/ (float)Screen.width, (float)cam.pixelHeight/ (float)Screen.height);
        ViewScale = new Vector3(1f / 64f, 1f / 64f);

        movableWidth = Mathf.Max((mapDimensions.x - MapViewSize.x) * 0.5f, 0.0f);
        moveableHeight = Mathf.Max((mapDimensions.y - MapViewSize.y) * 0.5f, 0.0f);

        //ensure the camera is within the movable space
        PanMapBy(Vector3.zero);
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void MouseAtPosition(Vector3 mausPos)
    {
        //Each frame get the mouse position
        Vector2 mousePos = new Vector2(mausPos.x, mausPos.y);

        //mousePos = cam.ScreenToViewportPoint(mousePos);
        //mousePos.x = mousePos.x * cam.scaledPixelWidth;
        //mousePos.y = mousePos.y * cam.scaledPixelHeight;


        //bool inside = mapRect.Contains(mousePos);// && !mapCentreRect.Contains(mousePos);
        bool inside = (RectTransformUtility.RectangleContainsScreenPoint(MapTransform, mausPos)
            & !RectTransformUtility.RectangleContainsScreenPoint(Deadzone, mausPos));

        Vector2 direction = mousePos - mapCentre;

        if (inside)
        {
            
            if (!Maus)
            {
                //This condition statement is present for testing purposes. It keeps the debug log from spamming messages
                //Debug.Log("Direction is: " + direction + "Centre is:" + mapCentre + " Mouse is at: " + mousePos);
                Maus = true;
            }


            if (Mathf.Abs(direction.x) < Deadzone.rect.width * 0.5f - BORDER)
            {
                direction.x = 0.0f;
            }
            else
            {
                direction.x = Mathf.Sign(direction.x);
            }

            if (Mathf.Abs(direction.y) < Deadzone.rect.height * 0.5f - BORDER)
            {
                direction.y = 0.0f;
            }
            else
            {
                direction.y = Mathf.Sign(direction.y);
            }



            //direction = direction.normalized;
            Vector3 change = new Vector3(direction.x, direction.y);

            change *= Time.deltaTime * scrollSpeed;

            //Clamp values between movable space

            PanMapBy(change);

        }
        else
        {
            Maus = false;
        }
    }




    /*
     * IsPointOnMap
     * 
     * this simple method returns a boolean value representing if the provided point described in the parameter is in the map panel
     */
    public bool IsPointOnMap(Vector2 point)
    {
        
        return RectTransformUtility.RectangleContainsScreenPoint(MapTransform, point);
    }


    /*
     * PanMapBy
     * 
     * This method will move the camera within the bounds of the map
     * 
     */
    public void PanMapBy(Vector3 change)
    {
        if (Controller.CurrentPermissions.CheckBooleanControlPermission(BooleanControls.pan_map))
        {
            camObject.transform.position = new Vector3(
            Mathf.Clamp(camObject.transform.position.x + change.x, WorldCentre.x - movableWidth, WorldCentre.x + movableWidth),
                Mathf.Clamp(camObject.transform.position.y + change.y, WorldCentre.y - moveableHeight, WorldCentre.y + moveableHeight),
                camObject.transform.position.z);
        }

        
    }



    /*
     * PanMapByDrag
     * 
     * This method handles the panning of the map by touch input
     * 
     */
    public void PanMapByDrag(Vector3 change)
    {
        PanMapBy(Vector3.Scale(change, ViewScale) * -1f);
    }

}
