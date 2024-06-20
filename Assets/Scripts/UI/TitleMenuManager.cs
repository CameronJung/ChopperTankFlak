using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class TitleMenuManager : MonoBehaviour
{

    private Vector2 BACKGROUNDRESOLUTION = new Vector2(2048f, 2048f);

    [SerializeField] private GameObject exitButton;
    [SerializeField] private RectTransform Background;
    [SerializeField] private float speed = 64f;
    [SerializeField] private Camera cam;


    [SerializeField] private RectTransform PanelsTarget;
    [SerializeField] private RectTransform ListHidingSpot;
    [SerializeField] private RectTransform DescHidingSpot;


    [SerializeField] private RectTransform ListPanel;
    [SerializeField] private RectTransform DescPanel;

    [SerializeField] private float PanelBuffer = 10.0f;
    [SerializeField] private float PanelSpeed = 0.5f;

    private const float CLOSE_ENOUGH = 1.0f;
    private bool PanelsStill = true;



    private Vector3 direction;
    private Vector3 origin;
    private Vector2 movableSpace;

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_WEBGL || UNITY_ANDROID
    exitButton.SetActive(false);
#endif
        direction = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0.0f);
        direction.Normalize();

        Rect rect = gameObject.GetComponent<RectTransform>().rect;


        movableSpace = new Vector2(Mathf.Abs(BACKGROUNDRESOLUTION.x - rect.width) * 0.5f - speed, Mathf.Abs(BACKGROUNDRESOLUTION.y - rect.height) * 0.5f - speed);
        origin = Background.anchoredPosition3D;


        ListPanel.position = ListHidingSpot.position;
        DescPanel.position = DescHidingSpot.position;

    }

    // Update is called once per frame
    void Update()
    {
        
        Vector3 change = speed * direction * Time.deltaTime;

        Vector3 pos = Background.anchoredPosition3D + change;

        

        //Check if we need to bounce
        if(pos.x < origin.x - movableSpace.x || pos.x > origin.x + movableSpace.x)
        {
            //bounce against a side
            direction.x *= -1;
            change.x *= -1;
        }
        if(pos.y < origin.y - movableSpace.y || pos.y > origin.y + movableSpace.y)
        {
            //bounce against the top or bottom
            direction.y *= -1;
            change.y *= -1;
        }
        Background.position += change;
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void ShowInstructions()
    {
        SceneManager.LoadScene(2);
    }

    public void ShowCredits()
    {
        SceneManager.LoadScene("Credits");
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;

#endif
        Application.Quit();
    }


    public void DisplayLevelSelect()
    {
        
        if (PanelsStill)
        {
            PanelsStill = false;
            StartCoroutine(BringInLevelPanels());
        }
        
    }


    public void HideLevelSelect()
    {
        
        if (PanelsStill)
        {
            PanelsStill = false;
            StartCoroutine(ShooAwayLevelPanels());
        }
        
    }

    private IEnumerator BringInLevelPanels()
    {

        float listDist = Mathf.Abs(PanelsTarget.position.x - ListPanel.position.x - PanelBuffer);
        float descDist = Mathf.Abs(PanelsTarget.position.x - DescPanel.position.x + PanelBuffer);
        float listPrev = ListPanel.position.x;
        float descPrev = DescPanel.position.x;


        bool listMoving = true;
        bool descMoving = true;

        while (listMoving || descMoving)
        {

            listDist = Mathf.Abs(PanelsTarget.position.x - ListPanel.position.x - PanelBuffer);


            //Once the remaining distance
            if(listDist > Mathf.Max(Mathf.Abs(ListPanel.position.x - listPrev), CLOSE_ENOUGH) && listMoving)
            {
                listPrev = ListPanel.position.x;
                ListPanel.position = Vector2.Lerp(ListPanel.position, PanelsTarget.position - new Vector3(PanelBuffer, 0f, 0f),
                    PanelSpeed * Time.deltaTime);
            }
            else
            {
                listMoving = false;
            }


            descDist = Mathf.Abs(PanelsTarget.position.x - DescPanel.position.x + PanelBuffer);

            if (descDist > Mathf.Max(Mathf.Abs(DescPanel.position.x - descPrev), CLOSE_ENOUGH) && descMoving)
            {
                descPrev = DescPanel.position.x;
                DescPanel.position = Vector2.Lerp(DescPanel.position, PanelsTarget.position + new Vector3(PanelBuffer, 0f, 0f),
                    PanelSpeed * Time.deltaTime);
            }
            else
            {
                descMoving = false;
            }


            yield return null;
        }


        ListPanel.position = PanelsTarget.position - new Vector3(PanelBuffer, 0.0f);
        DescPanel.position = PanelsTarget.position + new Vector3(PanelBuffer, 0.0f);
        PanelsStill = true;
    }



    private IEnumerator ShooAwayLevelPanels()
    {

        float listDist = Mathf.Abs(ListHidingSpot.position.x - ListPanel.position.x - PanelBuffer);
        float descDist = Mathf.Abs(DescHidingSpot.position.x - DescPanel.position.x + PanelBuffer);
        float listPrev = ListPanel.position.x;
        float descPrev = DescPanel.position.x;


        bool listMoving = true;
        bool descMoving = true;

        while (listMoving || descMoving)
        {

            listDist = Mathf.Abs(ListHidingSpot.position.x - ListPanel.position.x - PanelBuffer);

            if (listDist > Mathf.Max(Mathf.Abs(ListPanel.position.x - listPrev), CLOSE_ENOUGH) && listMoving)
            {
                listPrev = ListPanel.position.x;
                ListPanel.position = Vector2.Lerp(ListPanel.position, ListHidingSpot.position - new Vector3(PanelBuffer, 0f, 0f),
                    PanelSpeed * Time.deltaTime);
            }
            else
            {
                listMoving = false;
            }


            descDist = Mathf.Abs(DescHidingSpot.position.x - DescPanel.position.x + PanelBuffer);

            if (descDist > Mathf.Max(Mathf.Abs(DescPanel.position.x - descPrev), CLOSE_ENOUGH) && descMoving)
            {
                descPrev = DescPanel.position.x;
                DescPanel.position = Vector2.Lerp(DescPanel.position, DescHidingSpot.position + new Vector3(PanelBuffer, 0f, 0f),
                    PanelSpeed * Time.deltaTime);
            }
            else
            {
                descMoving = false;
            }




            yield return null;
        }


        ListPanel.position = ListHidingSpot.position - new Vector3(PanelBuffer, 0.0f);
        DescPanel.position = DescHidingSpot.position + new Vector3(PanelBuffer, 0.0f);
        PanelsStill = true;
    }

}
