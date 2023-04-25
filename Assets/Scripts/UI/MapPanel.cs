using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    //The playable area of the game board
    [SerializeField] private Vector2Int mapSize = Vector2Int.zero;

    [SerializeField] private float scrollSpeed = 0.8f;

    private Rect mapRect;
    private Rect mapCentreRect;
    private Vector2 mapCentre;

    private Vector2 mapDimensions;

    private bool insideMap = false;

    private Vector2 mapPos;

    //The distance the camera can move without going over the edge of the map
    private float moveableHeight;

    private float movableWidth;

    void Start()
    {
        
        mapDimensions = new Vector2(mapSize.x * HEXWIDTH, mapSize.y * HEXHEIGHT);
        cam = camObject.GetComponent<Camera>();
        mapRect = cam.pixelRect;
        mapCentreRect = new Rect(mapRect.xMin + BORDER, mapRect.yMin + BORDER, mapRect.width - 2 * BORDER, mapRect.height - 2 * BORDER);
        mapCentre = new Vector2(mapRect.xMin + 0.5f * cam.pixelWidth, mapRect.yMin + 0.5f * cam.pixelHeight);

        movableWidth = (mapDimensions.x - cam.pixelWidth / PIXDISTX * HEXWIDTH) * 0.5f;
        moveableHeight = (mapDimensions.y - cam.pixelHeight / PIXDISTY * HEXHEIGHT) * 0.5f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.mousePresent)
        {
            //Each frame get the mouse position
            Vector2 mousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

            mousePos = cam.ScreenToViewportPoint(mousePos);
            mousePos.x = mousePos.x * cam.pixelWidth;
            mousePos.y = mousePos.y * cam.pixelHeight;


            bool inside = mapRect.Contains(mousePos) && !mapCentreRect.Contains(mousePos);

            Vector2 direction = mousePos - mapCentre;

            if (inside)
            {
                
                if(Mathf.Abs(direction.x) < cam.pixelWidth * 0.5f - BORDER)
                {
                    direction.x = 0.0f;
                }
                else
                {
                    direction.x = Mathf.Sign(direction.x);
                }
                if (Mathf.Abs(direction.y) < cam.pixelHeight * 0.5f -BORDER)
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

                camObject.transform.position = new Vector3(Mathf.Clamp(camObject.transform.position.x + change.x, -movableWidth, movableWidth),
                    Mathf.Clamp(camObject.transform.position.y + change.y, -moveableHeight, moveableHeight),
                    camObject.transform.position.z);
                Debug.Log("Camera is at: " + camObject.transform.position);
            }


        }
    }
}
